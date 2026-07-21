using EduHub.Domain.Entities.Identity;

namespace EduHub.Application.Interfaces.Authentication;

/// <summary>
/// Ghi chú: IJwtTokenService là service kỹ thuật dùng để ijwt token.
/// </summary>
public interface IJwtTokenService
{
    JwtTokenResult CreateAccessToken(User user);
}
