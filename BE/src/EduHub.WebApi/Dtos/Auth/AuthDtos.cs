namespace EduHub.WebApi.Dtos.Auth;

/// <summary>
/// Ghi chú: LoginRequest là DTO request API dùng cho đăng nhập bằng email/password.
/// </summary>
public sealed record LoginRequest(string Email, string Password, string? DeviceId);

/// <summary>
/// Ghi chú: RefreshTokenRequest là DTO request API dùng cho xoay vòng refresh token.
/// </summary>
public sealed record RefreshTokenRequest(string RefreshToken, string? DeviceId);

/// <summary>
/// Ghi chú: LogoutRequest là DTO request API dùng cho đăng xuất và thu hồi refresh token.
/// </summary>
public sealed record LogoutRequest(string RefreshToken);

/// <summary>
/// Ghi chú: CurrentUserRequest là DTO request API dùng để lấy user hiện tại từ JWT.
/// </summary>
public sealed record CurrentUserRequest;

/// <summary>
/// Ghi chú: AuthTokenDto là DTO response API chứa access token và refresh token sau login/refresh.
/// </summary>
public sealed record AuthTokenDto(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);

/// <summary>
/// Ghi chú: CurrentUserDto là DTO response API chứa thông tin user hiện tại từ JWT.
/// </summary>
public sealed record CurrentUserDto(
    string Email,
    string FullName,
    string Role);
