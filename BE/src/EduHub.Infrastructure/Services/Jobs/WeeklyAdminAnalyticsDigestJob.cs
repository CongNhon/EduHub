using System.Net;
using System.Globalization;
using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Services.Analytics;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduHub.Infrastructure.Services.Jobs;

/// <summary>
/// Ghi chú: WeeklyAdminAnalyticsDigestJob gửi email HTML tổng hợp KPI và lỗi dữ liệu cho SystemAdmin mỗi tuần.
/// </summary>
public sealed class WeeklyAdminAnalyticsDigestJob(
    IAdminAnalyticsSnapshotService snapshotService,
    ApplicationDbContext dbContext,
    IEmailSender emailSender,
    TimeProvider timeProvider,
    IConfiguration configuration)
{
    private const string TemplateVersion = "admin-analytics-weekly-v1";

    /// <summary>
    /// Ghi chú: SendAsync tạo snapshot học kỳ active và gửi một email idempotent cho từng SystemAdmin active.
    /// </summary>
    public async Task SendAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await snapshotService.ReadAsync(null, cancellationToken);
        if (snapshot.IsFailure)
        {
            return;
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var periodEndUtc = nowUtc.Date;
        var periodStartUtc = periodEndUtc.AddDays(-7);
        var recipients = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Role == UserRole.SystemAdmin && user.IsActive)
            .Select(user => new { user.Id, user.Email, user.FullName })
            .ToListAsync(cancellationToken);

        foreach (var recipient in recipients)
        {
            var idempotencyKey = $"{recipient.Id:N}:{periodStartUtc:yyyyMMdd}:{periodEndUtc:yyyyMMdd}:{TemplateVersion}";
            var delivery = await dbContext.EmailDigestDeliveries.SingleOrDefaultAsync(
                item => item.IdempotencyKey == idempotencyKey,
                cancellationToken);
            if (delivery?.Status == EmailDigestDeliveryStatus.Sent)
            {
                continue;
            }

            if (delivery is null)
            {
                delivery = new EmailDigestDelivery(
                    recipient.Id,
                    recipient.Email,
                    idempotencyKey,
                    periodStartUtc,
                    periodEndUtc,
                    TemplateVersion);
                dbContext.EmailDigestDeliveries.Add(delivery);
            }

            delivery.BeginAttempt(nowUtc);
            await dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                var email = RenderEmail(recipient.FullName, snapshot.Value);
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
    /// Ghi chú: RenderEmail tạo HTML/text digest có KPI, backlog và link mở DevExpress report viewer.
    /// </summary>
    private AdminAnalyticsDigestEmail RenderEmail(
        string recipientName,
        AdminAnalyticsReportData data)
    {
        var portalBaseUrl = (configuration["Frontend:PortalBaseUrl"] ?? "http://localhost:3001").TrimEnd('/');
        var reportUrl = $"{portalBaseUrl}/admin/reports";
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeSemester = WebUtility.HtmlEncode($"{data.Overview.Semester.AcademicYearName} · {data.Overview.Semester.Name}");
        var issueRows = string.Join(string.Empty, data.DataQuality.Issues
            .Where(issue => issue.Count > 0)
            .OrderByDescending(issue => issue.Severity == "Critical")
            .ThenByDescending(issue => issue.Count)
            .Take(6)
            .Select(issue => $"<tr><td style=\"padding:8px;border-bottom:1px solid #e2e8f0\">{WebUtility.HtmlEncode(issue.Title)}</td><td style=\"padding:8px;border-bottom:1px solid #e2e8f0;text-align:right\"><b>{issue.Count}</b></td></tr>"));
        var average = data.Academic.AverageNormalizedScore?.ToString("0.00", CultureInfo.InvariantCulture) ?? "N/A";
        var passRate = data.Academic.PassRatePercentage.HasValue ? $"{data.Academic.PassRatePercentage.Value:0.00}%" : "N/A";
        var subject = $"EduHub weekly analytics · {data.Overview.Semester.Name}";
        var html = $"""
            <!doctype html><html><body style="font-family:Arial,sans-serif;color:#1f2937;background:#f8fafc;padding:24px">
            <div style="max-width:680px;margin:auto;background:#fff;border:1px solid #e2e8f0;border-radius:8px;overflow:hidden">
              <div style="background:#126b57;color:#fff;padding:22px 26px"><div style="font-size:12px;font-weight:700">EDUHUB SYSTEM ANALYTICS</div><h1 style="font-size:22px;margin:8px 0 0">Weekly operations digest</h1></div>
              <div style="padding:24px 26px"><p>Xin chào <b>{safeName}</b>,</p><p>Snapshot: <b>{safeSemester}</b></p>
                <table style="width:100%;border-collapse:collapse;margin:18px 0"><tr><td style="padding:12px;background:#ecf7f3">Học sinh active<br><b style="font-size:22px">{data.Overview.ActiveStudents}</b></td><td style="padding:12px;background:#f1f5f9">Điểm trung bình<br><b style="font-size:22px">{average}</b></td><td style="padding:12px;background:#fff7e6">Tỷ lệ đạt<br><b style="font-size:22px">{passRate}</b></td></tr></table>
                <h2 style="font-size:16px">Data Quality · {data.DataQuality.TotalFindings} phát hiện</h2><table style="width:100%;border-collapse:collapse">{issueRows}</table>
                <p style="margin-top:22px"><a href="{reportUrl}" style="display:inline-block;background:#126b57;color:#fff;text-decoration:none;padding:11px 16px;border-radius:6px">Mở DevExpress report</a></p>
              </div>
            </div></body></html>
            """;
        var text = $"EduHub weekly analytics\n{data.Overview.Semester.AcademicYearName} - {data.Overview.Semester.Name}\nActive students: {data.Overview.ActiveStudents}\nAverage score: {average}\nPass rate: {passRate}\nData quality findings: {data.DataQuality.TotalFindings}\nReport: {reportUrl}";
        return new AdminAnalyticsDigestEmail(subject, html, text);
    }

    /// <summary>
    /// Ghi chú: AdminAnalyticsDigestEmail chứa subject, HTML body và text fallback của email SystemAdmin.
    /// </summary>
    private sealed record AdminAnalyticsDigestEmail(string Subject, string HtmlBody, string TextBody);
}
