namespace EduHub.Application.Interfaces.Services.Integrations;

/// <summary>
/// Ghi chú: IExternalSyncJobScheduler enqueue job xử lý Ministry sync record.
/// </summary>
public interface IExternalSyncJobScheduler
{
    /// <summary>
    /// Ghi chú: EnqueueSyncRecord enqueue background job với payload chỉ chứa sync record id.
    /// </summary>
    string EnqueueSyncRecord(Guid syncRecordId);

    /// <summary>
    /// Ghi chú: ScheduleSyncRecord đặt lịch chạy lại Ministry sync record tại thời điểm retry.
    /// </summary>
    string ScheduleSyncRecord(Guid syncRecordId, DateTimeOffset enqueueAt);
}
