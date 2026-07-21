namespace EduHub.WebApi.Dtos.Integrations;

/// <summary>
/// Ghi chú: RetryGradeSyncRequest là DTO API để admin nhập lý do retry Ministry sync cho assignment.
/// </summary>
public sealed record RetryGradeSyncRequest(string Reason);

/// <summary>
/// Ghi chú: ExternalSyncRecordDto là DTO API trả trạng thái sync Ministry cho admin.
/// </summary>
public sealed record ExternalSyncRecordDto(
    Guid Id,
    string AggregateType,
    Guid AggregateId,
    int Version,
    string IdempotencyKey,
    string Status,
    int Attempts,
    string? ExternalId,
    string? ExternalVersion,
    string? LastError,
    DateTime? NextRetryAtUtc,
    DateTime? SucceededAtUtc);
