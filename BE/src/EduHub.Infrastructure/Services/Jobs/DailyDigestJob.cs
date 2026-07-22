using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Services.Jobs;

/// <summary>
/// Ghi chú: DailyDigestJob gửi fake email cuối ngày cho phụ huynh có grade Published/Locked trong ngày.
/// </summary>
public sealed class DailyDigestJob(
    ApplicationDbContext dbContext,
    IEmailSender emailSender,
    TimeProvider timeProvider)
{
    private const string TemplateVersion = "daily-digest-v1";

    /// <summary>
    /// Ghi chú: SendDailyDigestAsync gửi fake email theo recipient/ngày/template version, không chứa Draft grade.
    /// </summary>
    public async Task SendDailyDigestAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var periodStart = now.Date;
        var periodEnd = periodStart.AddDays(1);

        var recipients = await dbContext.ParentStudents
            .AsNoTracking()
            .Where(link => link.IsActive)
            .Join(
                dbContext.GradeEntries.AsNoTracking().Where(entry =>
                    (entry.Status == GradeStatus.Published || entry.Status == GradeStatus.Locked) &&
                    entry.PublishedAtUtc >= periodStart &&
                    entry.PublishedAtUtc < periodEnd),
                link => link.StudentId,
                entry => entry.StudentId,
                (link, entry) => link.ParentUserId)
            .Distinct()
            .Join(
                dbContext.Users.AsNoTracking(),
                parentUserId => parentUserId,
                user => user.Id,
                (parentUserId, user) => new { parentUserId, user.Email, user.IsActive })
            .Where(recipient => recipient.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var recipient in recipients)
        {
            var items = await GetDigestItemsAsync(recipient.parentUserId, periodStart, periodEnd, cancellationToken);
            if (items.Count == 0)
            {
                continue;
            }

            var idempotencyKey = $"{recipient.parentUserId:N}:{periodStart:yyyyMMdd}:{periodEnd:yyyyMMdd}:{TemplateVersion}";
            var delivery = await dbContext.EmailDigestDeliveries.SingleOrDefaultAsync(
                delivery => delivery.IdempotencyKey == idempotencyKey,
                cancellationToken);
            if (delivery?.Status == EmailDigestDeliveryStatus.Sent)
            {
                continue;
            }

            if (delivery is null)
            {
                delivery = new EmailDigestDelivery(
                    recipient.parentUserId,
                    recipient.Email,
                    idempotencyKey,
                    periodStart,
                    periodEnd,
                    TemplateVersion);
                dbContext.EmailDigestDeliveries.Add(delivery);
            }

            delivery.BeginAttempt(now);
            await dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                var email = GradeDigestEmailTemplate.Render("daily", periodStart, periodEnd, items);
                await emailSender.SendAsync(recipient.Email, email.Subject, email.HtmlBody, email.TextBody, idempotencyKey, cancellationToken);
                delivery.MarkSent(timeProvider.GetUtcNow().UtcDateTime);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                delivery.MarkFailed(exception.Message);
                await dbContext.SaveChangesAsync(cancellationToken);
                throw;
            }
        }
    }

    /// <summary>
    /// Ghi chú: GetDigestItemsAsync lấy danh sách điểm Published/Locked của các học sinh thuộc một phụ huynh trong ngày.
    /// </summary>
    private async Task<IReadOnlyList<GradeDigestEmailItem>> GetDigestItemsAsync(
        Guid parentUserId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken)
    {
        var rows = await dbContext.ParentStudents
            .AsNoTracking()
            .Where(link => link.ParentUserId == parentUserId && link.IsActive)
            .Join(
                dbContext.GradeEntries.AsNoTracking().Where(entry =>
                    (entry.Status == GradeStatus.Published || entry.Status == GradeStatus.Locked) &&
                    entry.PublishedAtUtc >= periodStart &&
                    entry.PublishedAtUtc < periodEnd),
                link => link.StudentId,
                entry => entry.StudentId,
                (link, entry) => new { link.StudentId, Entry = entry })
            .Join(
                dbContext.Students.AsNoTracking(),
                row => row.StudentId,
                student => student.Id,
                (row, student) => new { row.Entry, StudentName = student.FullName })
            .Join(
                dbContext.GradeComponents.AsNoTracking(),
                row => row.Entry.ComponentId,
                component => component.Id,
                (row, component) => new
                {
                    row.StudentName,
                    ComponentName = component.Name,
                    row.Entry.Score,
                    component.MaxScore,
                    row.Entry.Status,
                    PublishedAtUtc = row.Entry.PublishedAtUtc!.Value
                })
            .OrderBy(row => row.StudentName)
            .ThenBy(row => row.ComponentName)
            .ToListAsync(cancellationToken);

        return rows.Select(row => new GradeDigestEmailItem(
            row.StudentName,
            row.ComponentName,
            row.Score,
            row.MaxScore,
            row.Status.ToString(),
            row.PublishedAtUtc)).ToList();
    }
}
