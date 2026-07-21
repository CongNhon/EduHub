namespace EduHub.Application.Interfaces.Authentication;

/// <summary>
/// Ghi chú: JwtTokenResult đại diện cho jwt token result trong hệ thống EduHub.
/// </summary>
public sealed record JwtTokenResult(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string JwtId);
