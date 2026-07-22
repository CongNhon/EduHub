using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;

namespace EduHub.Application.Contracts.Monitoring;

/// <summary>
/// Ghi chú: GetSystemMonitoringQuery đọc trạng thái vận hành Redis, Hangfire và các hàng đợi tích hợp cho SystemAdmin.
/// </summary>
public sealed record GetSystemMonitoringQuery : IQuery<Result<SystemMonitoringResponse>>;

/// <summary>
/// Ghi chú: CacheMonitoringResponse chứa hit, miss, failure và tỷ lệ hit của Redis cache adapter.
/// </summary>
public sealed record CacheMonitoringResponse(long Hits, long Misses, long Failures, decimal? HitRatePercentage);

/// <summary>
/// Ghi chú: HangfireMonitoringResponse chứa số worker và job theo trạng thái trong Hangfire storage.
/// </summary>
public sealed record HangfireMonitoringResponse(
    long Servers,
    long Recurring,
    long Enqueued,
    long Scheduled,
    long Processing,
    long Succeeded,
    long Failed,
    long Deleted);

/// <summary>
/// Ghi chú: OutboxMonitoringResponse chứa backlog Outbox chưa xử lý và thời điểm message cũ nhất.
/// </summary>
public sealed record OutboxMonitoringResponse(int Pending, int Retried, DateTime? OldestPendingAtUtc);

/// <summary>
/// Ghi chú: OperationalStatusCountResponse chứa số bản ghi của một hàng đợi nghiệp vụ theo trạng thái.
/// </summary>
public sealed record OperationalStatusCountResponse(string Status, int Count);

/// <summary>
/// Ghi chú: OperationalMonitoringSnapshot chứa số liệu PostgreSQL và Hangfire trước khi service ghép thêm cache metrics.
/// </summary>
public sealed record OperationalMonitoringSnapshot(
    HangfireMonitoringResponse Hangfire,
    OutboxMonitoringResponse Outbox,
    IReadOnlyList<OperationalStatusCountResponse> ExternalSyncs,
    IReadOnlyList<OperationalStatusCountResponse> EmailDigests,
    IReadOnlyList<OperationalStatusCountResponse> ReportJobs,
    int NotificationsLast24Hours);

/// <summary>
/// Ghi chú: SystemMonitoringResponse chứa snapshot vận hành đầy đủ cho dashboard giám sát SystemAdmin.
/// </summary>
public sealed record SystemMonitoringResponse(
    DateTime GeneratedAtUtc,
    CacheMonitoringResponse Cache,
    HangfireMonitoringResponse Hangfire,
    OutboxMonitoringResponse Outbox,
    IReadOnlyList<OperationalStatusCountResponse> ExternalSyncs,
    IReadOnlyList<OperationalStatusCountResponse> EmailDigests,
    IReadOnlyList<OperationalStatusCountResponse> ReportJobs,
    int NotificationsLast24Hours);
