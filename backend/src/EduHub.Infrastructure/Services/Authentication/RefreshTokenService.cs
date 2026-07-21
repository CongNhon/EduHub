using System.Security.Cryptography;
using System.Text;
using EduHub.Application.Interfaces.Authentication;

namespace EduHub.Infrastructure.Services.Authentication;

/// <summary>
/// Ghi chú: RefreshTokenService là service kỹ thuật dùng để xoay vòng refresh token và cấp token mới.
/// </summary>
public sealed class RefreshTokenService : IRefreshTokenService
{
    /// <summary>
    /// Ghi chú: GenerateToken thực hiện phần xử lý của xoay vòng refresh token và cấp token mới.
    /// </summary>
    public string GenerateToken() => Base64UrlEncode(RandomNumberGenerator.GetBytes(64));

    /// <summary>
    /// Ghi chú: HashToken thực hiện phần xử lý của xoay vòng refresh token và cấp token mới.
    /// </summary>
    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    /// <summary>
    /// Ghi chú: Base64UrlEncode đổi bytes random thành chuỗi token an toàn cho URL.
    /// </summary>
    private static string Base64UrlEncode(byte[] value) =>
        Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
