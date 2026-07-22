using EduHub.Application.Contracts.Monitoring;
using EduHub.WebApi.Dtos.Monitoring;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: SystemMonitoringMappings chuyển Application monitoring response thành API DTO.
/// </summary>
public static class SystemMonitoringMappings
{
    /// <summary>
    /// Ghi chú: ToDto chuyển toàn bộ snapshot monitoring sang DTO cho Portal SystemAdmin.
    /// </summary>
    public static SystemMonitoringDto ToDto(this SystemMonitoringResponse response) =>
        new(
            response.GeneratedAtUtc,
            new CacheMonitoringDto(response.Cache.Hits, response.Cache.Misses, response.Cache.Failures, response.Cache.HitRatePercentage),
            new HangfireMonitoringDto(response.Hangfire.Servers, response.Hangfire.Recurring, response.Hangfire.Enqueued, response.Hangfire.Scheduled, response.Hangfire.Processing, response.Hangfire.Succeeded, response.Hangfire.Failed, response.Hangfire.Deleted),
            new OutboxMonitoringDto(response.Outbox.Pending, response.Outbox.Retried, response.Outbox.OldestPendingAtUtc),
            response.ExternalSyncs.Select(item => new OperationalStatusCountDto(item.Status, item.Count)).ToList(),
            response.EmailDigests.Select(item => new OperationalStatusCountDto(item.Status, item.Count)).ToList(),
            response.ReportJobs.Select(item => new OperationalStatusCountDto(item.Status, item.Count)).ToList(),
            response.NotificationsLast24Hours);
}
