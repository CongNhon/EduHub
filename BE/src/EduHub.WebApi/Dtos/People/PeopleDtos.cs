using EduHub.Domain.Enums;

namespace EduHub.WebApi.Dtos.People;

/// <summary>
/// Ghi chú: ListUserAccountsRequest chứa bộ lọc danh sách giáo viên, phụ huynh và tài khoản khác.
/// </summary>
public sealed record ListUserAccountsRequest(UserRole? Role, bool? IsActive, string? Search, int? Page, int? PageSize);

/// <summary>
/// Ghi chú: CreateUserAccountRequest chứa dữ liệu SystemAdmin nhập khi tạo tài khoản người dùng.
/// </summary>
public sealed record CreateUserAccountRequest(string Email, string Password, string FullName, string? ReferenceCode, string? PhoneNumber, UserRole Role);

/// <summary>
/// Ghi chú: UpdateUserAccountRequest chứa hồ sơ, role và trạng thái tài khoản cần cập nhật.
/// </summary>
public sealed record UpdateUserAccountRequest(string FullName, string? ReferenceCode, string? PhoneNumber, UserRole Role, bool IsActive);

/// <summary>
/// Ghi chú: UserAccountDto trả thông tin tài khoản dùng trong màn hình quản lý con người.
/// </summary>
public sealed record UserAccountDto(Guid Id, string Email, string FullName, string? ReferenceCode, string? PhoneNumber, string Role, bool IsActive);
