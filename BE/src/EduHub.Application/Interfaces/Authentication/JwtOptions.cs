namespace EduHub.Application.Interfaces.Authentication;

/// <summary>
/// Ghi chú: JwtOptions chứa cấu hình cho JWT access token.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Auth:Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 7;
}
