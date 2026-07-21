namespace EduHub.Application.Interfaces.Authentication;

/// <summary>
/// Ghi chú: IRefreshTokenService là service kỹ thuật dùng để irefresh token.
/// </summary>
public interface IRefreshTokenService
{
    string GenerateToken();

    string HashToken(string token);
}
