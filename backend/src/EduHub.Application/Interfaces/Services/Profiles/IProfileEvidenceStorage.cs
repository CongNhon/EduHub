namespace EduHub.Application.Interfaces.Services.Profiles;

/// <summary>
/// Ghi chú: ProfileEvidenceUploadGrant chứa URL upload trực tiếp R2 hoặc endpoint local dùng trong development.
/// </summary>
public sealed record ProfileEvidenceUploadGrant(string ObjectKey, string UploadUrl, DateTime ExpiresAtUtc, bool UsesDirectCloudUpload);

/// <summary>
/// Ghi chú: IProfileEvidenceStorage là interface lưu và đọc ảnh bằng chứng của yêu cầu sửa hồ sơ học sinh.
/// </summary>
public interface IProfileEvidenceStorage
{
    Task<ProfileEvidenceUploadGrant> CreateUploadGrantAsync(Guid ownerUserId, string fileName, string contentType, CancellationToken cancellationToken);
    Task StoreLocalAsync(string objectKey, string contentType, byte[] content, CancellationToken cancellationToken);
    Task<(byte[] Content, string ContentType)> ReadLocalAsync(string objectKey, CancellationToken cancellationToken);
    Task<string> CreateReadUrlAsync(string objectKey, CancellationToken cancellationToken);
}
