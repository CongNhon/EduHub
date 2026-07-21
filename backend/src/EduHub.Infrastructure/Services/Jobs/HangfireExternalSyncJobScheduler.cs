using EduHub.Application.Interfaces.Services.Integrations;
using Hangfire;

namespace EduHub.Infrastructure.Services.Jobs;

/// <summary>
/// Ghi chú: HangfireExternalSyncJobScheduler enqueue MinistrySyncJob cho ExternalSyncRecord.
/// </summary>
public sealed class HangfireExternalSyncJobScheduler(IBackgroundJobClient backgroundJobClient)
    : IExternalSyncJobScheduler
{
    /// <summary>
    /// Ghi chú: EnqueueSyncRecord đưa sync record vào Hangfire để gọi Ministry API bất đồng bộ.
    /// </summary>
    public string EnqueueSyncRecord(Guid syncRecordId) =>
        backgroundJobClient.Enqueue<MinistrySyncJob>(
            job => job.ProcessSyncRecordAsync(syncRecordId, CancellationToken.None));

    /// <summary>
    /// Ghi chú: ScheduleSyncRecord đặt lịch Hangfire retry cho sync record ở thời điểm đã tính.
    /// </summary>
    public string ScheduleSyncRecord(Guid syncRecordId, DateTimeOffset enqueueAt) =>
        backgroundJobClient.Schedule<MinistrySyncJob>(
            job => job.ProcessSyncRecordAsync(syncRecordId, CancellationToken.None),
            enqueueAt);
}
