using System.IdentityModel.Tokens.Jwt;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Domain.Entities.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EduHub.Infrastructure.Services.Authentication;

/// <summary>
/// Ghi chú: JwtTokenService là service kỹ thuật dùng để JWT access token.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : IJwtTokenService
{
    /// <summary>
    /// Ghi chú: CreateAccessToken thực hiện phần xử lý của JWT access token.
    /// </summary>
    public JwtTokenResult CreateAccessToken(User user)
    {
        var jwtOptions = options.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiresAtUtc = now.AddMinutes(jwtOptions.AccessTokenMinutes);
        var jwtId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("security_stamp", user.SecurityStamp.ToString())
        };

        var token = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            now,
            expiresAtUtc,
            new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new JwtTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc, jwtId);
    }
}
