namespace EduHub.Domain.Enums;

/// <summary>
/// Ghi chú: ExternalSyncStatus liệt kê trạng thái đồng bộ Ministry API của một aggregate/version.
/// </summary>
public enum ExternalSyncStatus
{
    Pending,
    Processing,
    Succeeded,
    RetryScheduled,
    FailedPermanent
}
