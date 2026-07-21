using EduHub.Application.Contracts.Auth;
using EduHub.WebApi.Dtos.Auth;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: AuthMappings chứa mapping giữa Auth DTO của API và command/response của Application.
/// </summary>
public static class AuthMappings
{
    /// <summary>
    /// Ghi chú: ToCommand chuyển LoginRequest API thành LoginCommand application.
    /// </summary>
    public static LoginCommand ToCommand(this LoginRequest request) =>
        new(request.Email, request.Password, request.DeviceId);

    /// <summary>
    /// Ghi chú: ToCommand chuyển RefreshTokenRequest API thành RefreshTokenCommand application.
    /// </summary>
    public static RefreshTokenCommand ToCommand(this RefreshTokenRequest request) =>
        new(request.RefreshToken, request.DeviceId);

    /// <summary>
    /// Ghi chú: ToCommand chuyển LogoutRequest API thành LogoutCommand application.
    /// </summary>
    public static LogoutCommand ToCommand(this LogoutRequest request) =>
        new(request.RefreshToken);

    /// <summary>
    /// Ghi chú: ToQuery chuyển CurrentUserRequest API thành GetCurrentUserQuery application.
    /// </summary>
    public static GetCurrentUserQuery ToQuery(this CurrentUserRequest request) =>
        new();

    /// <summary>
    /// Ghi chú: ToDto chuyển AuthTokenResponse application thành AuthTokenDto API.
    /// </summary>
    public static AuthTokenDto ToDto(this AuthTokenResponse response) =>
        new(
            response.AccessToken,
            response.AccessTokenExpiresAtUtc,
            response.RefreshToken,
            response.RefreshTokenExpiresAtUtc);

    /// <summary>
    /// Ghi chú: ToDto chuyển CurrentUserResponse application thành CurrentUserDto API.
    /// </summary>
    public static CurrentUserDto ToDto(this CurrentUserResponse response) =>
        new(response.Email, response.FullName, response.Role);
}
