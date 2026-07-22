using EduHub.Application.Contracts.Monitoring;
using EduHub.Application.Interfaces.Repositories.Monitoring;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Monitoring;

/// <summary>
/// Ghi chú: SystemMonitoringRepository đọc PostgreSQL queues và Hangfire monitoring API cho dashboard vận hành.
/// </summary>
public sealed class SystemMonitoringRepository(ApplicationDbContext dbContext, JobStorage jobStorage) : ISystemMonitoringRepository
{
    /// <summary>
    /// Ghi chú: GetSnapshotAsync tổng hợp backlog Outbox, integration, email digest, report jobs và Hangfire.
    /// </summary>
    public async Task<OperationalMonitoringSnapshot> GetSnapshotAsync(DateTime nowUtc, CancellationToken cancellationToken)
    {
        var pendingOutbox = dbContext.OutboxMessages.AsNoTracking().Where(message => message.ProcessedAtUtc == null);
        var pendingOutboxCount = await pendingOutbox.CountAsync(cancellationToken);
        var retriedOutboxCount = await pendingOutbox.CountAsync(message => message.RetryCount > 0, cancellationToken);
        var oldestPendingAtUtc = await pendingOutbox
            .OrderBy(message => message.OccurredAtUtc)
            .Select(message => (DateTime?)message.OccurredAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var externalSyncs = await ReadStatusCountsAsync(
            dbContext.ExternalSyncRecords.AsNoTracking().GroupBy(record => record.Status)
                .Select(group => new StatusCountRow<ExternalSyncStatus>(group.Key, group.Count())),
            Enum.GetValues<ExternalSyncStatus>(),
            cancellationToken);
        var emailDigests = await ReadStatusCountsAsync(
            dbContext.EmailDigestDeliveries.AsNoTracking().GroupBy(delivery => delivery.Status)
                .Select(group => new StatusCountRow<EmailDigestDeliveryStatus>(group.Key, group.Count())),
            Enum.GetValues<EmailDigestDeliveryStatus>(),
            cancellationToken);
        var reportJobs = await ReadStatusCountsAsync(
            dbContext.ReportJobs.AsNoTracking().GroupBy(job => job.Status)
                .Select(group => new StatusCountRow<ReportJobStatus>(group.Key, group.Count())),
            Enum.GetValues<ReportJobStatus>(),
            cancellationToken);
        var notificationsLast24Hours = await dbContext.Notifications.CountAsync(
            notification => notification.OccurredAtUtc >= nowUtc.AddHours(-24),
            cancellationToken);

        var statistics = jobStorage.GetMonitoringApi().GetStatistics();
        var hangfire = new HangfireMonitoringResponse(
            statistics.Servers,
            statistics.Recurring,
            statistics.Enqueued,
            statistics.Scheduled,
            statistics.Processing,
            statistics.Succeeded,
            statistics.Failed,
            statistics.Deleted);

        return new OperationalMonitoringSnapshot(
            hangfire,
            new OutboxMonitoringResponse(pendingOutboxCount, retriedOutboxCount, oldestPendingAtUtc),
            externalSyncs,
            emailDigests,
            reportJobs,
            notificationsLast24Hours);
    }

    /// <summary>
    /// Ghi chú: ReadStatusCountsAsync bổ sung trạng thái có count bằng 0 để chart không mất cột.
    /// </summary>
    private static async Task<IReadOnlyList<OperationalStatusCountResponse>> ReadStatusCountsAsync<TStatus>(
        IQueryable<StatusCountRow<TStatus>> query,
        IReadOnlyList<TStatus> statuses,
        CancellationToken cancellationToken)
        where TStatus : struct, Enum
    {
        var rows = await query.ToListAsync(cancellationToken);
        var counts = rows.ToDictionary(row => row.Status, row => row.Count);
        return statuses.Select(status => new OperationalStatusCountResponse(status.ToString(), counts.GetValueOrDefault(status))).ToList();
    }

    /// <summary>
    /// Ghi chú: StatusCountRow giữ enum và count trung gian do EF Core trả về từ GroupBy.
    /// </summary>
    private sealed record StatusCountRow<TStatus>(TStatus Status, int Count) where TStatus : struct, Enum;
}
