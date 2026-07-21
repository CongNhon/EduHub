using EduHub.Domain.Common;

namespace EduHub.Domain.Entities.Identity;

/// <summary>
/// Ghi chú: RefreshToken đại diện cho xoay vòng refresh token và cấp token mới trong hệ thống EduHub.
/// </summary>
public sealed class RefreshToken : AuditableEntity
{
    private RefreshToken()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor khởi tạo xoay vòng refresh token và cấp token mới và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public RefreshToken(Guid userId, string tokenHash, Guid familyId, DateTime expiresAtUtc, string? deviceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        UserId = userId;
        TokenHash = tokenHash;
        FamilyId = familyId;
        ExpiresAtUtc = UtcDateTime.Require(expiresAtUtc, nameof(expiresAtUtc));
        DeviceId = deviceId;
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public string TokenHash { get; private set; } = null!;

    public Guid FamilyId { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public string? DeviceId { get; private set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;

    /// <summary>
    /// Ghi chú: IsExpired thực hiện phần xử lý của xoay vòng refresh token và cấp token mới.
    /// </summary>
    public bool IsExpired(DateTime utcNow) => ExpiresAtUtc <= UtcDateTime.Require(utcNow, nameof(utcNow));

    /// <summary>
    /// Ghi chú: IsActive thực hiện phần xử lý của xoay vòng refresh token và cấp token mới.
    /// </summary>
    public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);

    /// <summary>
    /// Ghi chú: Revoke thực hiện phần xử lý của xoay vòng refresh token và cấp token mới.
    /// </summary>
    public void Revoke(DateTime revokedAtUtc)
    {
        RevokedAtUtc = UtcDateTime.Require(revokedAtUtc, nameof(revokedAtUtc));
        MarkUpdated(RevokedAtUtc.Value);
    }

    /// <summary>
    /// Ghi chú: Rotate thực hiện phần xử lý của xoay vòng refresh token và cấp token mới.
    /// </summary>
    public void Rotate(Guid replacementTokenId, DateTime revokedAtUtc)
    {
        ReplacedByTokenId = replacementTokenId;
        Revoke(revokedAtUtc);
    }
}
