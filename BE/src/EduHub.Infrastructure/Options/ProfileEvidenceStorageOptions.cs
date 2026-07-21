using Microsoft.Extensions.Configuration;

namespace EduHub.Infrastructure.Options;

/// <summary>
/// Ghi chú: ProfileEvidenceStorageOptions chứa cấu hình Cloudflare R2 hoặc thư mục local lưu ảnh bằng chứng hồ sơ.
/// </summary>
public sealed class ProfileEvidenceStorageOptions
{
    public bool R2Enabled { get; init; }
    public string ServiceUrl { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = "eduhub-profile-evidence";
    public string LocalRootPath { get; init; } = Path.Combine(AppContext.BaseDirectory, "data", "profile-evidence");

    /// <summary>
    /// Ghi chú: FromConfiguration đọc secret R2 và vị trí fallback local từ app configuration.
    /// </summary>
    public static ProfileEvidenceStorageOptions FromConfiguration(IConfiguration configuration)
    {
        var serviceUrl = configuration["EvidenceStorage:R2:ServiceUrl"] ?? string.Empty;
        var accessKey = configuration["EvidenceStorage:R2:AccessKey"] ?? string.Empty;
        var secretKey = configuration["EvidenceStorage:R2:SecretKey"] ?? string.Empty;
        return new ProfileEvidenceStorageOptions
        {
            R2Enabled = !string.IsNullOrWhiteSpace(serviceUrl) && !string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey),
            ServiceUrl = serviceUrl,
            AccessKey = accessKey,
            SecretKey = secretKey,
            BucketName = configuration["EvidenceStorage:R2:BucketName"] ?? "eduhub-profile-evidence",
            LocalRootPath = configuration["EvidenceStorage:LocalRootPath"] ?? Path.Combine(AppContext.BaseDirectory, "data", "profile-evidence")
        };
    }
}
