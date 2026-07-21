using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Domain.Enums;

namespace EduHub.Application.Contracts.People;

/// <summary>
/// Ghi chú: UserAccountResponse chứa hồ sơ tài khoản giáo viên, phụ huynh, học sinh hoặc quản trị viên.
/// </summary>
public sealed record UserAccountResponse(Guid Id, string Email, string FullName, string? ReferenceCode, string? PhoneNumber, string Role, bool IsActive);

/// <summary>
/// Ghi chú: ListUserAccountsQuery đọc danh sách tài khoản theo role, trạng thái, từ khóa và phân trang.
/// </summary>
public sealed record ListUserAccountsQuery(UserRole? Role, bool? IsActive, string? Search, int Page, int PageSize)
    : IQuery<Result<PagedResult<UserAccountResponse>>>;

/// <summary>
/// Ghi chú: CreateUserAccountCommand tạo tài khoản đăng nhập mới do SystemAdmin quản lý.
/// </summary>
public sealed record CreateUserAccountCommand(string Email, string Password, string FullName, string? ReferenceCode, string? PhoneNumber, UserRole Role)
    : ICommand<Result<UserAccountResponse>>;

/// <summary>
/// Ghi chú: UpdateUserAccountCommand cập nhật hồ sơ, role và trạng thái tài khoản hiện có.
/// </summary>
public sealed record UpdateUserAccountCommand(Guid Id, string FullName, string? ReferenceCode, string? PhoneNumber, UserRole Role, bool IsActive)
    : ICommand<Result<UserAccountResponse>>;
