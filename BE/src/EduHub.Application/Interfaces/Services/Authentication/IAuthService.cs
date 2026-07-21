using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Auth;

namespace EduHub.Application.Interfaces.Services.Authentication;

/// <summary>
/// Ghi chú: IAuthService là interface cho nghiệp vụ đăng nhập, refresh token, logout và đọc người dùng hiện tại.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Ghi chú: LoginAsync xử lý đăng nhập email/password và cấp access token, refresh token.
    /// </summary>
    Task<Result<AuthTokenResponse>> LoginAsync(LoginCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: RefreshAsync xử lý xoay vòng refresh token và cấp cặp token mới.
    /// </summary>
    Task<Result<AuthTokenResponse>> RefreshAsync(RefreshTokenCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: LogoutAsync xử lý đăng xuất bằng cách thu hồi refresh token hiện tại.
    /// </summary>
    Task<Result> LogoutAsync(LogoutCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetCurrentUserAsync trả thông tin người dùng hiện tại đang có trong JWT.
    /// </summary>
    Task<Result<CurrentUserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken);
}
