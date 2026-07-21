using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using EduHub.Application.Interfaces.Services.Profiles;
using EduHub.Infrastructure.Options;

namespace EduHub.Infrastructure.Services.Profiles;

/// <summary>
/// Ghi chú: ProfileEvidenceStorage tạo presigned URL Cloudflare R2 và fallback thư mục local trong development.
/// </summary>
public sealed class ProfileEvidenceStorage : IProfileEvidenceStorage, IDisposable
{
    private readonly ProfileEvidenceStorageOptions _options;
    private readonly AmazonS3Client? _s3Client;

    /// <summary>
    /// Ghi chú: Constructor khởi tạo S3-compatible client khi đủ secret R2, nếu không dùng local storage.
    /// </summary>
    public ProfileEvidenceStorage(ProfileEvidenceStorageOptions options)
    {
        _options = options;
        if (options.R2Enabled)
        {
            _s3Client = new AmazonS3Client(
                new BasicAWSCredentials(options.AccessKey, options.SecretKey),
                new AmazonS3Config
                {
                    ServiceURL = options.ServiceUrl,
                    ForcePathStyle = true,
                    AuthenticationRegion = "auto"
                });
        }
    }

    /// <summary>
    /// Ghi chú: CreateUploadGrantAsync tạo object key riêng theo user và URL PUT hết hạn sau mười phút.
    /// </summary>
    public async Task<ProfileEvidenceUploadGrant> CreateUploadGrantAsync(Guid ownerUserId, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var objectKey = $"profile-evidence/{ownerUserId:N}/{Guid.NewGuid():N}{extension}";
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(10);
        if (_s3Client is not null)
        {
            var url = await _s3Client.GetPreSignedURLAsync(new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                ContentType = contentType,
                Expires = expiresAtUtc
            });
            return new ProfileEvidenceUploadGrant(objectKey, url, expiresAtUtc, true);
        }

        var localUrl = $"/api/v1/student-profile/evidence/local?objectKey={Uri.EscapeDataString(objectKey)}";
        return new ProfileEvidenceUploadGrant(objectKey, localUrl, expiresAtUtc, false);
    }

    /// <summary>
    /// Ghi chú: StoreLocalAsync ghi ảnh bằng chứng vào thư mục riêng khi development chưa cấu hình R2.
    /// </summary>
    public async Task StoreLocalAsync(string objectKey, string contentType, byte[] content, CancellationToken cancellationToken)
    {
        if (_s3Client is not null) throw new InvalidOperationException("Direct R2 upload is enabled.");
        var path = ResolveLocalPath(objectKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, content, cancellationToken);
        await File.WriteAllTextAsync($"{path}.content-type", contentType, cancellationToken);
    }

    /// <summary>
    /// Ghi chú: ReadLocalAsync đọc byte ảnh local và content type để trả đúng định dạng cho trình duyệt.
    /// </summary>
    public async Task<(byte[] Content, string ContentType)> ReadLocalAsync(string objectKey, CancellationToken cancellationToken)
    {
        if (_s3Client is not null) throw new InvalidOperationException("Evidence is stored in R2.");
        var path = ResolveLocalPath(objectKey);
        if (!File.Exists(path)) throw new FileNotFoundException("Profile evidence was not found.", path);
        var content = await File.ReadAllBytesAsync(path, cancellationToken);
        var contentTypePath = $"{path}.content-type";
        var contentType = File.Exists(contentTypePath) ? await File.ReadAllTextAsync(contentTypePath, cancellationToken) : "application/octet-stream";
        return (content, contentType);
    }

    /// <summary>
    /// Ghi chú: CreateReadUrlAsync tạo presigned GET cho R2 hoặc endpoint đọc file local có kiểm tra authorization.
    /// </summary>
    public async Task<string> CreateReadUrlAsync(string objectKey, CancellationToken cancellationToken)
    {
        if (_s3Client is null)
        {
            return $"/api/v1/student-profile/evidence/local?objectKey={Uri.EscapeDataString(objectKey)}";
        }

        return await _s3Client.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(10)
        });
    }

    /// <summary>
    /// Ghi chú: ResolveLocalPath chuẩn hóa object key và ngăn path traversal ra ngoài thư mục evidence.
    /// </summary>
    private string ResolveLocalPath(string objectKey)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(_options.LocalRootPath)) + Path.DirectorySeparatorChar;
        var relativePath = objectKey.Replace('/', Path.DirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Invalid evidence object key.");
        return path;
    }

    /// <summary>
    /// Ghi chú: Dispose đóng S3 client khi application dừng.
    /// </summary>
    public void Dispose() => _s3Client?.Dispose();
}
