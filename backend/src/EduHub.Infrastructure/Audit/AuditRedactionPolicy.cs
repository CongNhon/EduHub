using System.Security.Cryptography;
using System.Text;

namespace EduHub.Infrastructure.Audit;

/// <summary>
/// Ghi chú: AuditRedactionPolicy che secret/PII nhạy cảm trước khi ghi audit log.
/// </summary>
public static class AuditRedactionPolicy
{
    private static readonly string[] SensitiveKeyParts =
    [
        "password",
        "token",
        "secret",
        "apikey",
        "api_key",
        "authorization",
        "connectionstring",
        "refresh"
    ];

    /// <summary>
    /// Ghi chú: RedactByKey che giá trị audit theo tên field/header/property nhạy cảm.
    /// </summary>
    public static string RedactByKey(string key, string? value) =>
        IsSensitiveKey(key) ? "[REDACTED]" : RedactValue(value);

    /// <summary>
    /// Ghi chú: RedactValue che chuỗi audit nếu có dấu hiệu chứa secret.
    /// </summary>
    public static string RedactValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Contains("Bearer ", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("Password=", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("ApiKey", StringComparison.OrdinalIgnoreCase)
            ? "[REDACTED]"
            : value;
    }

    /// <summary>
    /// Ghi chú: HashIp tạo hash IP để audit không lưu raw IP.
    /// </summary>
    public static string HashIp(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return string.Empty;
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ipAddress));
        return Convert.ToHexString(bytes)[..16];
    }

    private static bool IsSensitiveKey(string key)
    {
        var normalized = key.Replace("-", string.Empty, StringComparison.Ordinal).Replace("_", string.Empty, StringComparison.Ordinal);
        return SensitiveKeyParts.Any(part => normalized.Contains(part, StringComparison.OrdinalIgnoreCase));
    }
}
