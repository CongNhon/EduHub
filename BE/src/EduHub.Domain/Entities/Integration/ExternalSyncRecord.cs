using EduHub.Domain.Common;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Integration;

/// <summary>
/// Ghi chú: ExternalSyncRecord lưu trạng thái sync Ministry API idempotent theo aggregate/version.
/// </summary>
public sealed class ExternalSyncRecord : AuditableEntity
{
    private ExternalSyncRecord()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo record sync pending với idempotency key aggregateId:version.
    /// </summary>
    public ExternalSyncRecord(
        string aggregateType,
        Guid aggregateId,
        int version,
        string payload,
        DateTime createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(aggregateType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);

        AggregateType = aggregateType.Trim();
        AggregateId = aggregateId;
        Version = version;
        IdempotencyKey = $"{AggregateId:N}:{Version}";
        Payload = payload;
        Status = ExternalSyncStatus.Pending;
        CreatedAtUtc = UtcDateTime.Require(createdAtUtc, nameof(createdAtUtc));
    }

    public string AggregateType { get; private set; } = null!;

    public Guid AggregateId { get; private set; }

    public int Version { get; private set; }

    public string IdempotencyKey { get; private set; } = null!;

    public string Payload { get; private set; } = null!;

    public ExternalSyncStatus Status { get; private set; }

    public int Attempts { get; private set; }

    public string? ExternalId { get; private set; }

    public string? ExternalVersion { get; private set; }

    public string? LastError { get; private set; }

    public DateTime? NextRetryAtUtc { get; private set; }

    public DateTime? SucceededAtUtc { get; private set; }

    public Guid? LastManualRetryByUserId { get; private set; }

    public User? LastManualRetryByUser { get; private set; }

    public string? LastManualRetryReason { get; private set; }

    /// <summary>
    /// Ghi chú: MarkProcessing tăng attempt và chuyển sync record sang Processing.
    /// </summary>
    public void MarkProcessing(DateTime updatedAtUtc)
    {
        Attempts++;
        Status = ExternalSyncStatus.Processing;
        NextRetryAtUtc = null;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: MarkSucceeded lưu external id/version khi Ministry API nhận payload thành công.
    /// </summary>
    public void MarkSucceeded(string externalId, string externalVersion, DateTime succeededAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalVersion);

        ExternalId = externalId.Trim();
        ExternalVersion = externalVersion.Trim();
        Status = ExternalSyncStatus.Succeeded;
        SucceededAtUtc = UtcDateTime.Require(succeededAtUtc, nameof(succeededAtUtc));
        LastError = null;
        NextRetryAtUtc = null;
        MarkUpdated(SucceededAtUtc.Value);
    }

    /// <summary>
    /// Ghi chú: ScheduleRetry lưu lỗi transient và thời điểm retry tiếp theo.
    /// </summary>
    public void ScheduleRetry(string error, DateTime nextRetryAtUtc, DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        LastError = error.Trim();
        NextRetryAtUtc = UtcDateTime.Require(nextRetryAtUtc, nameof(nextRetryAtUtc));
        Status = ExternalSyncStatus.RetryScheduled;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: MarkFailedPermanent lưu lỗi permanent để admin xem và manual retry.
    /// </summary>
    public void MarkFailedPermanent(string error, DateTime failedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        LastError = error.Trim();
        Status = ExternalSyncStatus.FailedPermanent;
        NextRetryAtUtc = null;
        MarkUpdated(failedAtUtc);
    }

    /// <summary>
    /// Ghi chú: MarkManualRetry reset trạng thái để admin retry cùng idempotency key cũ.
    /// </summary>
    public void MarkManualRetry(Guid actorUserId, string reason, DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        LastManualRetryByUserId = actorUserId;
        LastManualRetryReason = reason.Trim();
        Status = ExternalSyncStatus.Pending;
        ExternalId = null;
        ExternalVersion = null;
        LastError = null;
        NextRetryAtUtc = null;
        SucceededAtUtc = null;
        MarkUpdated(updatedAtUtc);
    }
}
