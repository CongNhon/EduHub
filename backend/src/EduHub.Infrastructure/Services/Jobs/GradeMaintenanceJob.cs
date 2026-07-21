using System.Text.Json;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduHub.Infrastructure.Services.Jobs;

/// <summary>
/// Ghi chú: GradeMaintenanceJob khóa grade Published đã quá hạn và ghi outbox GradebookLocked.
/// </summary>
public sealed class GradeMaintenanceJob(
    ApplicationDbContext dbContext,
    IConfiguration configuration,
    TimeProvider timeProvider)
{
    /// <summary>
    /// Ghi chú: LockPublishedGradesAsync khóa các grade Published quá số ngày cấu hình, chạy lại không đổi kết quả.
    /// </summary>
    public async Task LockPublishedGradesAsync(CancellationToken cancellationToken = default)
    {
        var lockAfterDays = int.TryParse(configuration["Grades:AutoLockAfterDays"], out var configuredDays)
            ? configuredDays
            : 7;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var cutoff = now.AddDays(-lockAfterDays);

        var entries = await dbContext.GradeEntries
            .Where(entry =>
                entry.Status == GradeStatus.Published &&
                entry.PublishedAtUtc != null &&
                entry.PublishedAtUtc <= cutoff)
            .ToListAsync(cancellationToken);

        foreach (var entry in entries)
        {
            entry.Lock(now);
        }

        foreach (var assignmentId in entries.Select(entry => entry.AssignmentId).Distinct())
        {
            dbContext.OutboxMessages.Add(new OutboxMessage(
                "GradebookLocked",
                JsonSerializer.Serialize(new { assignmentId, status = GradeStatus.Locked.ToString() }),
                now));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
