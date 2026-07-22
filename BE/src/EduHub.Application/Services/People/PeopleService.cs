using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.People;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.People;
using EduHub.Application.Interfaces.Services.People;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace EduHub.Application.Services.People;

/// <summary>
/// Ghi chú: PeopleService xử lý tạo, cập nhật và tìm kiếm tài khoản người dùng của trường.
/// </summary>
public sealed partial class PeopleService(
    IPeopleRepository repository,
    IPasswordHashService passwordHashService,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<PeopleService> logger)
    : IPeopleService
{
    /// <summary>
    /// Ghi chú: ListAsync trả danh sách tài khoản đã lọc để AcademicAdmin chọn giáo viên hoặc phụ huynh.
    /// </summary>
    public async Task<Result<PagedResult<UserAccountResponse>>> ListAsync(ListUserAccountsQuery request, CancellationToken cancellationToken)
    {
        var page = PageRequest.Create(request.Page, request.PageSize, request.Search);
        return page.IsFailure
            ? Result.Failure<PagedResult<UserAccountResponse>>(page.Error!)
            : Result.Success(await repository.ListAsync(request.Role, request.IsActive, request.Search, page.Value, cancellationToken));
    }

    /// <summary>
    /// Ghi chú: CreateAsync tạo tài khoản mới với mật khẩu đã hash và email duy nhất.
    /// </summary>
    public async Task<Result<UserAccountResponse>> CreateAsync(CreateUserAccountCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();
        if (await repository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return Result.Failure<UserAccountResponse>(new Error("People.EmailExists", "Email already exists.", ErrorType.Conflict));
        }

        var user = new User(email, normalizedEmail, passwordHashService.HashPassword(request.Password), request.Role, request.FullName, request.ReferenceCode, request.PhoneNumber);
        repository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToResponse(user));
    }

    /// <summary>
    /// Ghi chú: UpdateAsync sửa hồ sơ, role và trạng thái; SecurityStamp mới làm token cũ hết hiệu lực.
    /// </summary>
    public async Task<Result<UserAccountResponse>> UpdateAsync(UpdateUserAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.GetAsync(request.Id, cancellationToken);
        if (user is null) return Result.Failure<UserAccountResponse>(new Error("People.NotFound", "User account was not found.", ErrorType.NotFound));

        var removesAdminAccess = request.Role != UserRole.SystemAdmin || !request.IsActive;
        if (currentUser.UserId == user.Id && removesAdminAccess)
        {
            return Result.Failure<UserAccountResponse>(new Error(
                "People.SelfAdminChangeForbidden",
                "System administrators cannot remove their own administrative access.",
                ErrorType.Conflict));
        }

        if (user.Role == UserRole.SystemAdmin && user.IsActive && removesAdminAccess &&
            await repository.CountActiveSystemAdminsAsync(cancellationToken) <= 1)
        {
            return Result.Failure<UserAccountResponse>(new Error(
                "People.LastSystemAdmin",
                "The last active system administrator cannot be disabled or demoted.",
                ErrorType.Conflict));
        }

        var removesCurrentRole = request.Role != user.Role || !request.IsActive;
        if (removesCurrentRole && await repository.HasActiveRoleDependenciesAsync(user.Id, user.Role, cancellationToken))
        {
            return Result.Failure<UserAccountResponse>(new Error(
                "People.ActiveRoleDependencies",
                "Deactivate active assignments or links before changing this account role or status.",
                ErrorType.Conflict));
        }

        var previousRole = user.Role;
        var previousActiveState = user.IsActive;
        var securityChanged = previousRole != request.Role || previousActiveState != request.IsActive;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        user.UpdateProfile(request.FullName, request.ReferenceCode, request.PhoneNumber, request.Role, request.IsActive, now);
        if (securityChanged)
        {
            await repository.RevokeActiveRefreshTokensAsync(user.Id, now, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        LogUserAccountChanged(
            logger,
            user.Id,
            currentUser.UserId,
            previousRole,
            user.Role,
            previousActiveState,
            user.IsActive,
            request.ChangeReason.Trim());
        return Result.Success(ToResponse(user));
    }

    /// <summary>
    /// Ghi chú: LogUserAccountChanged ghi audit thay đổi tài khoản gồm người thao tác, đối tượng, role, trạng thái và lý do nhưng không ghi secret.
    /// </summary>
    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Warning,
        Message = "User account {TargetUserId} changed by {ActorUserId}. Role {PreviousRole} -> {CurrentRole}; active {PreviousActiveState} -> {CurrentActiveState}; reason: {ChangeReason}")]
    private static partial void LogUserAccountChanged(
        ILogger logger,
        Guid targetUserId,
        Guid? actorUserId,
        UserRole previousRole,
        UserRole currentRole,
        bool previousActiveState,
        bool currentActiveState,
        string changeReason);

    private static UserAccountResponse ToResponse(User user) =>
        new(user.Id, user.Email, user.FullName, user.ReferenceCode, user.PhoneNumber, user.Role.ToString(), user.IsActive);
}
