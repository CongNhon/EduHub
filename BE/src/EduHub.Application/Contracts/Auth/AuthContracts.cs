using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;

namespace EduHub.Application.Contracts.Auth;

/// <summary>
/// Ghi chú: AuthTokenResponse là dữ liệu trả về cho cặp access token và refresh token.
/// </summary>
public sealed record AuthTokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);

/// <summary>
/// Ghi chú: CurrentUserResponse là dữ liệu trả về cho người dùng hiện tại đọc từ HttpContext/JWT.
/// </summary>
public sealed record CurrentUserResponse(
    string Email,
    string FullName,
    string Role);

/// <summary>
/// Ghi chú: LoginCommand là command để xử lý đăng nhập bằng email/password.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password,
    string? DeviceId) : ICommand<Result<AuthTokenResponse>>;

/// <summary>
/// Ghi chú: RefreshTokenCommand là command để xoay vòng refresh token và cấp token mới.
/// </summary>
public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? DeviceId) : ICommand<Result<AuthTokenResponse>>;

/// <summary>
/// Ghi chú: LogoutCommand là command để đăng xuất và thu hồi refresh token hiện tại.
/// </summary>
public sealed record LogoutCommand(string RefreshToken) : ICommand<Result>;

/// <summary>
/// Ghi chú: GetCurrentUserQuery là query để đọc thông tin người dùng hiện tại từ JWT.
/// </summary>
public sealed record GetCurrentUserQuery : IQuery<Result<CurrentUserResponse>>;
