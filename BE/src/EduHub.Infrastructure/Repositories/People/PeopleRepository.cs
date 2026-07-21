using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.People;
using EduHub.Application.Interfaces.Repositories.People;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.People;

/// <summary>
/// Ghi chú: PeopleRepository dùng EF Core để tìm kiếm và lưu tài khoản người dùng của trường.
/// </summary>
public sealed class PeopleRepository(ApplicationDbContext dbContext) : IPeopleRepository
{
    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    /// <summary>
    /// Ghi chu: CountActiveSystemAdminsAsync dem tai khoan SystemAdmin active de ngan khoa admin cuoi cung.
    /// </summary>
    public Task<int> CountActiveSystemAdminsAsync(CancellationToken cancellationToken) =>
        dbContext.Users.CountAsync(user => user.Role == UserRole.SystemAdmin && user.IsActive, cancellationToken);

    /// <summary>
    /// Ghi chu: HasActiveRoleDependenciesAsync kiem tra assignment/link active cua Teacher, Parent hoac Student truoc khi doi role.
    /// </summary>
    public Task<bool> HasActiveRoleDependenciesAsync(Guid userId, UserRole role, CancellationToken cancellationToken) =>
        role switch
        {
            UserRole.Teacher => dbContext.TeachingAssignments.AnyAsync(
                assignment => assignment.TeacherId == userId && assignment.IsActive,
                cancellationToken),
            UserRole.Parent => dbContext.ParentStudents.AnyAsync(
                link => link.ParentUserId == userId && link.IsActive,
                cancellationToken),
            UserRole.Student => dbContext.Students.AnyAsync(
                student => student.UserId == userId && student.Status == StudentStatus.Active,
                cancellationToken),
            _ => Task.FromResult(false)
        };

    /// <summary>
    /// Ghi chu: RevokeActiveRefreshTokensAsync thu hoi moi refresh token active khi role hoac trang thai dang nhap cua tai khoan thay doi.
    /// </summary>
    public async Task RevokeActiveRefreshTokensAsync(Guid userId, DateTime revokedAtUtc, CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(token => token.RevokedAtUtc, revokedAtUtc),
                cancellationToken);
    }

    public void Add(User user) => dbContext.Users.Add(user);

    /// <summary>
    /// Ghi chú: ListAsync tìm tài khoản theo email, họ tên, mã tham chiếu, role và trạng thái.
    /// </summary>
    public async Task<PagedResult<UserAccountResponse>> ListAsync(UserRole? role, bool? isActive, string? search, PageRequest pageRequest, CancellationToken cancellationToken)
    {
        var query = dbContext.Users.AsNoTracking();
        if (role.HasValue) query = query.Where(user => user.Role == role.Value);
        if (isActive.HasValue) query = query.Where(user => user.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(user => EF.Functions.ILike(user.Email, pattern) || EF.Functions.ILike(user.FullName, pattern) || (user.ReferenceCode != null && EF.Functions.ILike(user.ReferenceCode, pattern)));
        }

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.OrderBy(user => user.FullName).Skip(pageRequest.Skip).Take(pageRequest.PageSize)
            .Select(user => new UserAccountResponse(user.Id, user.Email, user.FullName, user.ReferenceCode, user.PhoneNumber, user.Role.ToString(), user.IsActive))
            .ToListAsync(cancellationToken);
        return new PagedResult<UserAccountResponse>(items, pageRequest.Page, pageRequest.PageSize, total);
    }
}
