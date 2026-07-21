using System.Text.Json;
using EduHub.Application.Contracts.Notifications;
using EduHub.Application.Interfaces.Services.Caching;
using EduHub.Application.Interfaces.Services.Integrations;
using EduHub.Application.Interfaces.Services.Notifications;
using EduHub.Application.Interfaces.Services.Reports;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Entities.Notifications;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Integrations.Ministry;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduHub.Infrastructure.Services.Messaging;

/// <summary>
/// Ghi chú: OutboxProcessor xử lý outbox message sau commit để tạo notification và push SignalR.
/// </summary>
public sealed partial class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger,
    TimeProvider timeProvider)
    : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const int BatchSize = 20;
    private const int MaxRetryCount = 10;

    /// <summary>
    /// Ghi chú: ExecuteAsync chạy vòng lặp đọc outbox pending theo chu kỳ.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken);
            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    /// <summary>
    /// Ghi chú: ProcessBatchAsync lấy một batch outbox pending và xử lý từng message.
    /// </summary>
    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();
        var syncJobScheduler = scope.ServiceProvider.GetRequiredService<IExternalSyncJobScheduler>();
        var reportJobScheduler = scope.ServiceProvider.GetRequiredService<IReportJobScheduler>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var cacheKeyPolicy = scope.ServiceProvider.GetRequiredService<ICacheKeyPolicy>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var messages = await dbContext.OutboxMessages
            .FromSqlInterpolated($$"""
                SELECT *
                FROM outbox_messages
                WHERE processed_at_utc IS NULL AND retry_count < {{MaxRetryCount}}
                ORDER BY occurred_at_utc
                LIMIT {{BatchSize}}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            await ProcessMessageAsync(
                dbContext,
                notifier,
                syncJobScheduler,
                reportJobScheduler,
                cacheService,
                cacheKeyPolicy,
                message,
                cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Ghi chú: ProcessMessageAsync xử lý một outbox message và mark processed hoặc tăng retry khi lỗi.
    /// </summary>
    private async Task ProcessMessageAsync(
        ApplicationDbContext dbContext,
        IRealtimeNotifier notifier,
        IExternalSyncJobScheduler syncJobScheduler,
        IReportJobScheduler reportJobScheduler,
        ICacheService cacheService,
        ICacheKeyPolicy cacheKeyPolicy,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid? syncRecordId = null;
            if (message.Type == "ReportJobRequested")
            {
                await ProcessReportJobRequestedAsync(dbContext, reportJobScheduler, message, cancellationToken);
            }

            if (message.Type is "GradebookPublished" or "GradebookReopened" or "GradebookLocked")
            {
                await ProcessGradebookNotificationAsync(dbContext, notifier, message, cancellationToken);
            }

            if (message.Type is "ReportRequested" or "ReportRequestApproved" or "ReportRequestRejected" or "ReportCompleted" or "ReportFailed")
            {
                await ProcessReportNotificationAsync(dbContext, notifier, message, cancellationToken);
            }

            if (message.Type is "GradebookPublished" or "GradebookLocked")
            {
                syncRecordId = await ProcessGradebookExternalSyncAsync(dbContext, message, cancellationToken);
            }

            if ((message.Type is "GradebookPublished" or "GradebookReopened" or "GradebookLocked") &&
                TryGetAssignmentId(message.Payload, out var cacheAssignmentId))
            {
                await cacheService.BumpVersionAsync(
                    cacheKeyPolicy.PublishedGradesScope(cacheAssignmentId),
                    cancellationToken);
            }

            message.MarkProcessed(timeProvider.GetUtcNow().UtcDateTime);
            await dbContext.SaveChangesAsync(cancellationToken);

            if (syncRecordId.HasValue)
            {
                _ = syncJobScheduler.EnqueueSyncRecord(syncRecordId.Value);
            }
        }
        catch (Exception ex)
        {
            message.MarkFailed();
            await dbContext.SaveChangesAsync(cancellationToken);
            LogOutboxMessageFailed(logger, ex, message.Id, message.Type, message.RetryCount);
        }
    }

    /// <summary>
    /// Ghi chu: ProcessReportJobRequestedAsync enqueue Hangfire sau khi report job va outbox da commit trong PostgreSQL.
    /// </summary>
    private static async Task ProcessReportJobRequestedAsync(
        ApplicationDbContext dbContext,
        IReportJobScheduler reportJobScheduler,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        using var payload = JsonDocument.Parse(message.Payload);
        if (!payload.RootElement.TryGetProperty("reportJobId", out var value) || !value.TryGetGuid(out var reportJobId))
        {
            throw new InvalidOperationException("ReportJobRequested payload does not contain reportJobId.");
        }

        var reportJob = await dbContext.ReportJobs.SingleOrDefaultAsync(job => job.Id == reportJobId, cancellationToken);
        if (reportJob is null)
        {
            throw new InvalidOperationException("Report job was not found for the outbox message.");
        }

        if (!string.IsNullOrWhiteSpace(reportJob.HangfireJobId) || reportJob.Status == ReportJobStatus.Completed)
        {
            return;
        }

        var hangfireJobId = reportJobScheduler.EnqueueReportJob(reportJob.Id);
        reportJob.MarkEnqueued(hangfireJobId, message.OccurredAtUtc);
    }

    /// <summary>
    /// Ghi chú: ProcessGradebookNotificationAsync tạo notification cho phụ huynh từ event gradebook.
    /// </summary>
    private static async Task ProcessGradebookNotificationAsync(
        ApplicationDbContext dbContext,
        IRealtimeNotifier notifier,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        if (!TryGetAssignmentId(message.Payload, out var assignmentId))
        {
            return;
        }

        var recipients = await dbContext.GradeEntries
            .AsNoTracking()
            .Where(entry => entry.AssignmentId == assignmentId)
            .Join(
                dbContext.ParentStudents.AsNoTracking().Where(link => link.IsActive),
                entry => entry.StudentId,
                link => link.StudentId,
                (entry, link) => new GradebookNotificationRecipient(link.ParentUserId, entry.StudentId, entry.Student.FullName, entry.Assignment.Subject.Name, entry.Assignment.ClassRoom.Name, entry.Assignment.Semester.Name))
            .Distinct()
            .ToListAsync(cancellationToken);

        var notifications = new List<Notification>();
        foreach (var recipient in recipients)
        {
            var exists = await dbContext.Notifications.AnyAsync(
                notification =>
                    notification.OutboxMessageId == message.Id &&
                    notification.RecipientUserId == recipient.ParentUserId,
                cancellationToken);
            if (exists)
            {
                continue;
            }

            notifications.Add(CreateNotification(message, recipient, assignmentId));
        }

        if (notifications.Count == 0)
        {
            return;
        }

        dbContext.Notifications.AddRange(notifications);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            await notifier.SendNotificationAsync(
                notification.RecipientUserId,
                new RealtimeNotificationPayload(
                    notification.Id,
                    notification.Type,
                    notification.StudentId,
                    notification.OccurredAtUtc),
                cancellationToken);
        }
    }

    /// <summary>
    /// Ghi chú: CreateNotification tạo notification DB cho một phụ huynh từ gradebook outbox message.
    /// </summary>
    private static Notification CreateNotification(
        OutboxMessage message,
        GradebookNotificationRecipient recipient,
        Guid assignmentId)
    {
        var (title, body) = message.Type switch
        {
            "GradebookPublished" => ($"Đã công bố điểm {recipient.SubjectName}", $"{recipient.StudentName} · {recipient.ClassName} · {recipient.SemesterName}"),
            "GradebookReopened" => ($"Điểm {recipient.SubjectName} đang được điều chỉnh", $"Nhà trường đang điều chỉnh kết quả của {recipient.StudentName}. Phiên bản công bố trước vẫn là dữ liệu tham chiếu."),
            "GradebookLocked" => ($"Đã khóa sổ điểm {recipient.SubjectName}", $"Kết quả của {recipient.StudentName} tại {recipient.ClassName} đã được nhà trường khóa."),
            _ => ("Cập nhật sổ điểm", $"Sổ điểm của {recipient.StudentName} vừa thay đổi trạng thái.")
        };

        return new Notification(
            recipient.ParentUserId,
            message.Id,
            message.Type,
            title,
            body,
            recipient.StudentId,
            assignmentId,
            message.OccurredAtUtc);
    }

    /// <summary>
    /// Ghi chú: ProcessReportNotificationAsync gửi yêu cầu mới tới AcademicAdmin và kết quả duyệt/tạo PDF tới phụ huynh.
    /// </summary>
    private static async Task ProcessReportNotificationAsync(ApplicationDbContext dbContext, IRealtimeNotifier notifier, OutboxMessage message, CancellationToken cancellationToken)
    {
        using var payload = JsonDocument.Parse(message.Payload);
        var root = payload.RootElement;
        var studentId = root.TryGetProperty("studentId", out var studentValue) && studentValue.TryGetGuid(out var parsedStudentId) ? parsedStudentId : (Guid?)null;
        List<Guid> recipients;
        if (message.Type == "ReportRequested")
        {
            recipients = await dbContext.Users.AsNoTracking()
                .Where(user => user.IsActive && (user.Role == UserRole.AcademicAdmin || user.Role == UserRole.SystemAdmin))
                .Select(user => user.Id)
                .ToListAsync(cancellationToken);
        }
        else if (root.TryGetProperty("recipientUserId", out var recipientValue) && recipientValue.TryGetGuid(out var recipientUserId))
        {
            recipients = [recipientUserId];
        }
        else
        {
            return;
        }

        var reason = root.TryGetProperty("reason", out var reasonValue) ? reasonValue.GetString() : null;
        var (title, body) = message.Type switch
        {
            "ReportRequested" => ("Yêu cầu báo cáo mới", "Phụ huynh vừa gửi một yêu cầu báo cáo học tập cần duyệt."),
            "ReportRequestApproved" => ("Yêu cầu báo cáo đã được duyệt", "Nhà trường đã duyệt yêu cầu và đang tạo file PDF."),
            "ReportRequestRejected" => ("Yêu cầu báo cáo bị từ chối", string.IsNullOrWhiteSpace(reason) ? "Nhà trường đã từ chối yêu cầu báo cáo." : $"Lý do: {reason}"),
            "ReportCompleted" => ("Báo cáo đã sẵn sàng", "Báo cáo học tập đã hoàn thành và có thể tải xuống."),
            _ => ("Không thể tạo báo cáo", "Quá trình tạo PDF gặp lỗi. Nhà trường sẽ kiểm tra và thử lại.")
        };

        foreach (var recipientId in recipients.Distinct())
        {
            if (await dbContext.Notifications.AnyAsync(notification => notification.OutboxMessageId == message.Id && notification.RecipientUserId == recipientId, cancellationToken)) continue;
            var notification = new Notification(recipientId, message.Id, message.Type, title, body, studentId, null, message.OccurredAtUtc);
            dbContext.Notifications.Add(notification);
            await dbContext.SaveChangesAsync(cancellationToken);
            await notifier.SendNotificationAsync(recipientId, new RealtimeNotificationPayload(notification.Id, notification.Type, notification.StudentId, notification.OccurredAtUtc), cancellationToken);
        }
    }

    /// <summary>
    /// Ghi chú: ProcessGradebookExternalSyncAsync tạo ExternalSyncRecord cho sổ điểm Published/Locked để sync Ministry API.
    /// </summary>
    private static async Task<Guid?> ProcessGradebookExternalSyncAsync(
        ApplicationDbContext dbContext,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        if (!TryGetAssignmentId(message.Payload, out var assignmentId))
        {
            return null;
        }

        var gradeEntries = await dbContext.GradeEntries
            .AsNoTracking()
            .Where(entry =>
                entry.AssignmentId == assignmentId &&
                (entry.Status == GradeStatus.Published || entry.Status == GradeStatus.Locked))
            .OrderBy(entry => entry.StudentId)
            .ThenBy(entry => entry.ComponentId)
            .Select(entry => new
            {
                entry.StudentId,
                entry.ComponentId,
                entry.Score,
                entry.PublicationVersion
            })
            .ToListAsync(cancellationToken);

        if (gradeEntries.Count == 0)
        {
            return null;
        }

        var publicationVersion = gradeEntries.Max(entry => entry.PublicationVersion);
        var exists = await dbContext.ExternalSyncRecords.AnyAsync(
            record =>
                record.AggregateType == "Gradebook" &&
                record.AggregateId == assignmentId &&
                record.Version == publicationVersion,
            cancellationToken);
        if (exists)
        {
            return null;
        }

        var request = new MinistryGradebookRequest(
            "ministry-gradebook-v1",
            assignmentId,
            publicationVersion,
            gradeEntries
                .Select(entry => new MinistryGradeItem(entry.StudentId, entry.ComponentId, entry.Score))
                .ToList());
        var record = new ExternalSyncRecord(
            "Gradebook",
            assignmentId,
            publicationVersion,
            JsonSerializer.Serialize(request, JsonOptions),
            message.OccurredAtUtc);

        dbContext.ExternalSyncRecords.Add(record);
        return record.Id;
    }

    /// <summary>
    /// Ghi chú: TryGetAssignmentId đọc assignment id từ payload JSON của outbox message.
    /// </summary>
    private static bool TryGetAssignmentId(string payload, out Guid assignmentId)
    {
        using var document = JsonDocument.Parse(payload);
        if (document.RootElement.TryGetProperty("assignmentId", out var value) &&
            value.TryGetGuid(out assignmentId))
        {
            return true;
        }

        assignmentId = Guid.Empty;
        return false;
    }

    private sealed record GradebookNotificationRecipient(Guid ParentUserId, Guid StudentId, string StudentName, string SubjectName, string ClassName, string SemesterName);

    [LoggerMessage(
        EventId = 30,
        Level = LogLevel.Warning,
        Message = "Outbox message failed. MessageId: {MessageId}; Type: {MessageType}; RetryCount: {RetryCount}")]
    private static partial void LogOutboxMessageFailed(
        ILogger logger,
        Exception exception,
        Guid messageId,
        string messageType,
        int retryCount);
}
