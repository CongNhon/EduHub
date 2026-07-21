using System.Security.Cryptography;
using System.Globalization;
using EduHub.Application.Interfaces.Authentication;

namespace EduHub.Infrastructure.Services.Authentication;

/// <summary>
/// Ghi chú: PasswordHashService là service kỹ thuật dùng để hash và verify password.
/// </summary>
public sealed class PasswordHashService : IPasswordHashService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 220_000;
    private const string Format = "pbkdf2-sha512";

    /// <summary>
    /// Ghi chú: HashPassword thực hiện phần xử lý của hash và verify password.
    /// </summary>
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            KeySize);

        return string.Join(
            '$',
            Format,
            Iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    /// <summary>
    /// Ghi chú: VerifyPassword thực hiện phần xử lý của hash và verify password.
    /// </summary>
    public bool VerifyPassword(string passwordHash, string password)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var parts = passwordHash.Split('$');
        if (parts.Length != 4 || parts[0] != Format || !int.TryParse(parts[1], CultureInfo.InvariantCulture, out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Ghi chu: NeedsRehash yeu cau tao hash moi neu PBKDF2 iteration cua tai khoan thap hon policy hien tai.
    /// </summary>
    public bool NeedsRehash(string passwordHash)
    {
        var parts = passwordHash.Split('$');
        return parts.Length != 4 ||
            parts[0] != Format ||
            !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations) ||
            iterations < Iterations;
    }
}
