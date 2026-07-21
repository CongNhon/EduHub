using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using EduHub.Application.Contracts.Grades;
using EduHub.Application.Interfaces.Services.Grades;
using EduHub.Application.Interfaces.Services.Reports;
using EduHub.Application.Services.Grades;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Options;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EduHub.Infrastructure.Services.Reports;

/// <summary>
/// Ghi chú: SimplePdfReportGenerator sinh PDF bảng điểm từ snapshot grade Published/Locked.
/// </summary>
public sealed partial class SimplePdfReportGenerator(
    ApplicationDbContext dbContext,
    IReportFileStorage reportFileStorage,
    IGpaCalculator gpaCalculator,
    IOptions<SchoolProfileOptions> schoolOptions,
    ILogger<SimplePdfReportGenerator> logger,
    TimeProvider timeProvider)
{
    private const string PolicyVersion = "report-card-v3";

    /// <summary>
    /// Ghi chú: GenerateReportCardAsync sinh file PDF cho một report job idempotently.
    /// </summary>
    public async Task GenerateReportCardAsync(Guid reportJobId, CancellationToken cancellationToken = default)
    {
        var reportJob = await dbContext.ReportJobs.SingleOrDefaultAsync(job => job.Id == reportJobId, cancellationToken);
        if (reportJob is null || reportJob.Status == ReportJobStatus.Completed)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        reportJob.MarkProcessing(now);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var student = await dbContext.Students.AsNoTracking().SingleAsync(
                candidate => candidate.Id == reportJob.StudentId,
                cancellationToken);
            var semester = await dbContext.Semesters.AsNoTracking().SingleAsync(
                candidate => candidate.Id == reportJob.SemesterId,
                cancellationToken);
            var enrollment = await dbContext.Enrollments.AsNoTracking()
                .Where(candidate => candidate.StudentId == reportJob.StudentId && candidate.SemesterId == reportJob.SemesterId)
                .OrderByDescending(candidate => candidate.EnrolledAtUtc)
                .Select(candidate => new { candidate.ClassRoom.ClassCode, ClassName = candidate.ClassRoom.Name })
                .FirstOrDefaultAsync(cancellationToken);
            var grades = await dbContext.GradeEntries
                .AsNoTracking()
                .Where(entry =>
                    entry.StudentId == reportJob.StudentId &&
                    entry.Assignment.SemesterId == reportJob.SemesterId &&
                    (entry.Status == GradeStatus.Published || entry.Status == GradeStatus.Locked))
                .OrderBy(entry => entry.Assignment.Subject.Name)
                .ThenBy(entry => entry.Component.DisplayOrder)
                .Select(entry => new ReportGradeLine(
                    entry.Assignment.Subject.Name,
                    entry.Assignment.Subject.Credits,
                    entry.Component.Name,
                    entry.Component.Weight,
                    entry.Component.MaxScore,
                    entry.Component.IsRequired,
                    entry.Component.IncludeInGpa,
                    entry.Score))
                .ToListAsync(cancellationToken);

            var subjectSummaries = grades
                .GroupBy(grade => new { grade.SubjectName, grade.Credits })
                .Select(group =>
                {
                    var average = gpaCalculator.CalculateSubjectAverage(group
                        .Select(grade => new GradeComponentScoreInput(grade.Score, grade.Weight, grade.IsRequired, grade.IncludeInGpa))
                        .ToList());
                    return new ReportSubjectSummary(group.Key.SubjectName, group.Key.Credits, average.Average, average.IsAvailable);
                })
                .OrderBy(subject => subject.SubjectName)
                .ToList();
            var semesterGpa = gpaCalculator.CalculateSemesterGpa(subjectSummaries
                .Select(subject => new SubjectGradeForGpaInput(subject.Average, subject.Credits))
                .ToList());
            var classification = semesterGpa.IsAvailable
                ? gpaCalculator.Classify(semesterGpa.Gpa!.Value, DefaultClassificationPolicy.Current)
                : null;

            var pdf = CreatePdf(
                student.FullName,
                student.StudentCode,
                enrollment?.ClassName ?? "Chưa xác định",
                enrollment?.ClassCode ?? "-",
                semester.Name,
                grades,
                subjectSummaries,
                semesterGpa,
                classification,
                now,
                schoolOptions.Value);

            var storageKey = await reportFileStorage.SaveAsync(reportJob.Id, pdf, cancellationToken);
            var checksum = Convert.ToHexString(SHA256.HashData(pdf));
            reportJob.MarkCompleted(storageKey, checksum, PolicyVersion, now, now.AddDays(7));
            var reportRequest = await dbContext.ReportRequests.SingleOrDefaultAsync(request => request.ReportJobId == reportJob.Id, cancellationToken);
            if (reportRequest is not null)
            {
                reportRequest.Complete(now);
                dbContext.OutboxMessages.Add(new OutboxMessage("ReportCompleted", JsonSerializer.Serialize(new { reportRequestId = reportRequest.Id, recipientUserId = reportRequest.RequesterUserId, studentId = reportRequest.StudentId }), now));
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            const string safeFailureReason = "Report generation failed. Please contact the school administrator.";
            LogReportGenerationFailed(logger, ex, reportJob.Id);
            reportJob.MarkFailed(safeFailureReason, timeProvider.GetUtcNow().UtcDateTime);
            var reportRequest = await dbContext.ReportRequests.SingleOrDefaultAsync(request => request.ReportJobId == reportJob.Id, cancellationToken);
            if (reportRequest is not null)
            {
                var failedAt = timeProvider.GetUtcNow().UtcDateTime;
                reportRequest.Fail(safeFailureReason, failedAt);
                dbContext.OutboxMessages.Add(new OutboxMessage("ReportFailed", JsonSerializer.Serialize(new { reportRequestId = reportRequest.Id, recipientUserId = reportRequest.RequesterUserId, studentId = reportRequest.StudentId }), failedAt));
            }
            await dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Ghi chú: CreatePdf tạo PDF tiếng Việt có nhận diện trường, học sinh, lớp, học kỳ và bảng điểm đã công bố.
    /// </summary>
    private static byte[] CreatePdf(
        string studentName,
        string studentCode,
        string className,
        string classCode,
        string semesterName,
        IReadOnlyList<ReportGradeLine> grades,
        IReadOnlyList<ReportSubjectSummary> subjectSummaries,
        SemesterGpaResult semesterGpa,
        ClassificationResult? classification,
        DateTime generatedAtUtc,
        SchoolProfileOptions school)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(32);
            page.DefaultTextStyle(style => style.FontSize(10).FontFamily("Lato"));
            page.Header().Column(column =>
            {
                column.Item().Text(school.Name).Bold().FontSize(15).FontColor(Colors.Indigo.Darken2);
                column.Item().Text($"{school.Address} | {school.Email} | {school.PhoneNumber}").FontSize(8).FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(14).AlignCenter().Text("BÁO CÁO KẾT QUẢ HỌC TẬP").Bold().FontSize(18);
            });
            page.Content().PaddingVertical(18).Column(column =>
            {
                column.Spacing(12);
                column.Item().Background(Colors.Grey.Lighten4).Padding(12).Row(row =>
                {
                    row.RelativeItem().Column(info => { info.Item().Text($"Học sinh: {studentName}").SemiBold(); info.Item().Text($"Mã học sinh: {studentCode}"); });
                    row.RelativeItem().Column(info => { info.Item().Text($"Lớp: {className} ({classCode})").SemiBold(); info.Item().Text($"Học kỳ: {semesterName}"); });
                });
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(semesterGpa.IsAvailable ? $"GPA học kỳ: {semesterGpa.Gpa:0.00}" : "GPA học kỳ: Chưa đủ dữ liệu").Bold();
                    row.RelativeItem().AlignRight().Text(classification is null ? "Xếp loại: Chưa xác định" : $"Xếp loại: {classification.Name}").Bold();
                });
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns => { columns.RelativeColumn(); columns.ConstantColumn(70); columns.ConstantColumn(90); });
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Môn học");
                        header.Cell().Element(HeaderCell).AlignRight().Text("Tín chỉ");
                        header.Cell().Element(HeaderCell).AlignRight().Text("Trung bình");
                    });
                    foreach (var subject in subjectSummaries)
                    {
                        table.Cell().Element(BodyCell).Text(subject.SubjectName);
                        table.Cell().Element(BodyCell).AlignRight().Text(subject.Credits.ToString(CultureInfo.InvariantCulture));
                        table.Cell().Element(BodyCell).AlignRight().Text(subject.IsAvailable ? $"{subject.Average:0.00}" : "N/A");
                    }
                });
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns => { columns.RelativeColumn(1.5f); columns.RelativeColumn(1.5f); columns.ConstantColumn(65); columns.ConstantColumn(65); });
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Môn học");
                        header.Cell().Element(HeaderCell).Text("Thành phần");
                        header.Cell().Element(HeaderCell).AlignRight().Text("Trọng số");
                        header.Cell().Element(HeaderCell).AlignRight().Text("Điểm");
                    });
                    foreach (var grade in grades)
                    {
                        table.Cell().Element(BodyCell).Text(grade.SubjectName);
                        table.Cell().Element(BodyCell).Text(grade.ComponentName);
                        table.Cell().Element(BodyCell).AlignRight().Text($"{grade.Weight:P0}");
                        table.Cell().Element(BodyCell).AlignRight().Text($"{grade.Score:0.##}/{grade.MaxScore:0.##}").SemiBold();
                    }
                });
                if (grades.Count == 0) column.Item().Text("Chưa có điểm được công bố trong học kỳ này.").Italic().FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(10).Text($"Báo cáo chỉ gồm dữ liệu Published/Locked tại thời điểm tạo. Classification policy: {DefaultClassificationPolicy.Current.Version}.").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
            page.Footer().Row(row =>
            {
                row.RelativeItem().Text($"Tạo lúc {generatedAtUtc.ToLocalTime():dd/MM/yyyy HH:mm} | {PolicyVersion}").FontSize(8).FontColor(Colors.Grey.Darken1);
                row.ConstantItem(80).AlignRight().Text(text => { text.Span("Trang "); text.CurrentPageNumber(); });
            });
        })).GeneratePdf();
    }

    private static IContainer HeaderCell(IContainer container) => container.Background(Colors.Indigo.Darken2).Padding(7).DefaultTextStyle(style => style.FontColor(Colors.White).SemiBold());
    private static IContainer BodyCell(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(7);
    /// <summary>
    /// Ghi chu: ReportGradeLine chua mot thanh phan diem Published/Locked de ve bang chi tiet PDF.
    /// </summary>
    private sealed record ReportGradeLine(
        string SubjectName,
        int Credits,
        string ComponentName,
        decimal Weight,
        decimal MaxScore,
        bool IsRequired,
        bool IncludeInGpa,
        decimal Score);

    /// <summary>
    /// Ghi chu: ReportSubjectSummary chua trung binh mon va tin chi de tinh GPA hoc ky trong PDF.
    /// </summary>
    private sealed record ReportSubjectSummary(string SubjectName, int Credits, decimal? Average, bool IsAvailable);

    [LoggerMessage(
        EventId = 40,
        Level = LogLevel.Error,
        Message = "Report generation failed. ReportJobId: {ReportJobId}")]
    /// <summary>
    /// Ghi chu: LogReportGenerationFailed ghi exception noi bo theo report job id ma khong tra chi tiet loi cho client.
    /// </summary>
    private static partial void LogReportGenerationFailed(ILogger logger, Exception exception, Guid reportJobId);
}
