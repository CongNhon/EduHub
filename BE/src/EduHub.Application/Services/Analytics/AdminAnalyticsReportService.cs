using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Services.Analytics;
using EduHub.Domain.Enums;

namespace EduHub.Application.Services.Analytics;

/// <summary>
/// Ghi chú: AdminAnalyticsReportService lấy ba dataset cùng học kỳ và chuyển cho DevExpress report renderer.
/// </summary>
public sealed class AdminAnalyticsReportService(
    IAdminAnalyticsSnapshotService snapshotService,
    ICurrentUser currentUser,
    IAdminAnalyticsReportRenderer renderer) : IAdminAnalyticsReportService
{
    private static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase) { "pdf", "xlsx", "csv" };

    /// <summary>
    /// Ghi chú: GetDataAsync lấy Overview trước để khóa học kỳ, sau đó lấy Academic và Data Quality cùng semesterId.
    /// </summary>
    public async Task<Result<AdminAnalyticsReportData>> GetDataAsync(Guid? semesterId, CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.SystemAdmin)
        {
            return Result.Failure<AdminAnalyticsReportData>(new Error(
                "AnalyticsReport.SystemAdminRequired",
                "System administrator role is required.",
                ErrorType.Forbidden));
        }

        return await snapshotService.ReadAsync(semesterId, cancellationToken);
    }

    /// <summary>
    /// Ghi chú: ExportAsync kiểm tra format và reportType rồi dựng file analytics có tên theo mẫu và học kỳ.
    /// </summary>
    public async Task<Result<AdminAnalyticsReportFileResponse>> ExportAsync(
        ExportAdminAnalyticsReportQuery request,
        CancellationToken cancellationToken)
    {
        var format = request.Format.Trim().ToLowerInvariant();
        if (!SupportedFormats.Contains(format))
        {
            return Result.Failure<AdminAnalyticsReportFileResponse>(new Error(
                "AnalyticsReport.UnsupportedFormat",
                "Supported formats are pdf, xlsx and csv.",
                ErrorType.Validation));
        }

        var reportType = request.ReportType.Trim().ToLowerInvariant();
        if (!AdminAnalyticsReportTypes.IsSupported(reportType))
        {
            return Result.Failure<AdminAnalyticsReportFileResponse>(new Error(
                "AnalyticsReport.UnsupportedReportType",
                "Mẫu báo cáo không được hỗ trợ hoặc không tồn tại.",
                ErrorType.Validation));
        }

        var data = await GetDataAsync(request.SemesterId, cancellationToken);
        if (data.IsFailure)
        {
            return Result.Failure<AdminAnalyticsReportFileResponse>(data.Error!);
        }

        var rendered = await renderer.RenderAsync(data.Value, reportType, format, cancellationToken);
        var semesterSlug = data.Value.Overview.Semester.Name.Replace(' ', '-').ToLowerInvariant();
        return Result.Success(new AdminAnalyticsReportFileResponse(
            $"eduhub-{reportType}-{semesterSlug}.{rendered.FileExtension}",
            rendered.ContentType,
            rendered.Content));
    }
}
