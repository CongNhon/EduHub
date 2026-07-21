using EduHub.Application.Common.Models;

namespace EduHub.Application.Features.Auth.Common;

/// <summary>
/// Ghi chú: AuthErrors gom các lỗi dùng khi xử lý mã lỗi đăng nhập/refresh/logout.
/// </summary>
public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        "auth.invalid_credentials",
        "Invalid email or password.",
        ErrorType.Unauthorized);

    public static readonly Error InvalidRefreshToken = new(
        "auth.invalid_refresh_token",
        "Invalid refresh token.",
        ErrorType.Unauthorized);

    public static readonly Error Unauthorized = new(
        "auth.unauthorized",
        "Authentication is required.",
        ErrorType.Unauthorized);
}
