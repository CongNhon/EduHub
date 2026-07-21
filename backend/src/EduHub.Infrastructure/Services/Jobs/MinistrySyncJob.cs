using EduHub.Application.Interfaces.Services.Integrations;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Refit;

namespace EduHub.Infrastructure.Services.Jobs;

/// <summary>
/// Ghi chú: MinistrySyncJob xử lý ExternalSyncRecord bằng cách gọi Ministry API sau local commit.
/// </summary>
public sealed class MinistrySyncJob(
    ApplicationDbContext dbContext,
    IEducationMinistryGateway ministryGateway,
    IExternalSyncJobScheduler syncJobScheduler,
    TimeProvider timeProvider)
{
    private const int MaxJobAttempts = 3;

    /// <summary>
    /// Ghi chú: ProcessSyncRecordAsync gửi một sync record sang Ministry API và lưu trạng thái retry/succeeded.
    /// </summary>
    public async Task ProcessSyncRecordAsync(Guid syncRecordId, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.ExternalSyncRecords
            .SingleOrDefaultAsync(item => item.Id == syncRecordId, cancellationToken);
        if (record is null || record.Status == ExternalSyncStatus.Succeeded)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        record.MarkProcessing(now);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var result = await ministryGateway.SyncGradebookAsync(record, cancellationToken);
            record.MarkSucceeded(result.ExternalId, result.ExternalVersion, timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (ApiException ex) when (IsPermanentHttpFailure(ex))
        {
            record.MarkFailedPermanent(CreateFailureMessage(ex), timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (Exception ex)
        {
            ScheduleRetry(record, ex);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ghi chú: IsPermanentHttpFailure xác định lỗi Ministry API không retry tự động như validation/auth/not-found.
    /// </summary>
    private static bool IsPermanentHttpFailure(ApiException exception) =>
        exception.StatusCode is
            System.Net.HttpStatusCode.BadRequest or
            System.Net.HttpStatusCode.Unauthorized or
            System.Net.HttpStatusCode.Forbidden or
            System.Net.HttpStatusCode.NotFound or
            System.Net.HttpStatusCode.UnprocessableEntity;

    /// <summary>
    /// Ghi chú: ScheduleRetry đặt lịch retry tiếp theo cho sync record khi Ministry API lỗi transient.
    /// </summary>
    private void ScheduleRetry(ExternalSyncRecord record, Exception exception)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (record.Attempts >= MaxJobAttempts)
        {
            record.MarkFailedPermanent(CreateFailureMessage(exception), now);
            return;
        }

        var delayMinutes = Math.Min(60, Math.Pow(2, record.Attempts));
        var nextRetryAtUtc = now.AddMinutes(delayMinutes);
        record.ScheduleRetry(CreateFailureMessage(exception), nextRetryAtUtc, now);
        syncJobScheduler.ScheduleSyncRecord(record.Id, new DateTimeOffset(nextRetryAtUtc, TimeSpan.Zero));
    }

    /// <summary>
    /// Ghi chú: CreateFailureMessage rút gọn lỗi Ministry API để lưu vào ExternalSyncRecord.
    /// </summary>
    private static string CreateFailureMessage(Exception exception) =>
        exception is ApiException apiException
            ? $"HTTP {(int)apiException.StatusCode}: {apiException.Message}"
            : exception.Message;
}
