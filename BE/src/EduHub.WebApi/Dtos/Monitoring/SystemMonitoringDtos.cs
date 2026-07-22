namespace EduHub.WebApi.Dtos.Monitoring;

/// <summary>
/// Ghi chú: CacheMonitoringDto trả hit, miss, failure và tỷ lệ hit Redis cho dashboard.
/// </summary>
public sealed record CacheMonitoringDto(long Hits, long Misses, long Failures, decimal? HitRatePercentage);

/// <summary>
/// Ghi chú: HangfireMonitoringDto trả số worker và job theo trạng thái Hangfire.
/// </summary>
public sealed record HangfireMonitoringDto(long Servers, long Recurring, long Enqueued, long Scheduled, long Processing, long Succeeded, long Failed, long Deleted);

/// <summary>
/// Ghi chú: OutboxMonitoringDto trả backlog Outbox và message chưa xử lý cũ nhất.
/// </summary>
public sealed record OutboxMonitoringDto(int Pending, int Retried, DateTime? OldestPendingAtUtc);

/// <summary>
/// Ghi chú: OperationalStatusCountDto trả số bản ghi của một queue theo trạng thái.
/// </summary>
public sealed record OperationalStatusCountDto(string Status, int Count);

/// <summary>
/// Ghi chú: SystemMonitoringDto trả snapshot vận hành Redis, Hangfire, Outbox, integrations, email và reports.
/// </summary>
public sealed record SystemMonitoringDto(
    DateTime GeneratedAtUtc,
    CacheMonitoringDto Cache,
    HangfireMonitoringDto Hangfire,
    OutboxMonitoringDto Outbox,
    IReadOnlyList<OperationalStatusCountDto> ExternalSyncs,
    IReadOnlyList<OperationalStatusCountDto> EmailDigests,
    IReadOnlyList<OperationalStatusCountDto> ReportJobs,
    int NotificationsLast24Hours);
