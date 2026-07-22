using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;

namespace EduHub.Application.Contracts.Analytics;

/// <summary>
/// Ghi chú: ExportAdminAnalyticsReportQuery yêu cầu xuất một mẫu báo cáo quản trị theo học kỳ và định dạng PDF, XLSX hoặc CSV.
/// </summary>
public sealed record ExportAdminAnalyticsReportQuery(Guid? SemesterId, string Format, string ReportType)
    : IQuery<Result<AdminAnalyticsReportFileResponse>>;

/// <summary>
/// Ghi chú: AdminAnalyticsReportTypes định danh ba mẫu báo cáo quản trị được phép xem và xuất file.
/// </summary>
public static class AdminAnalyticsReportTypes
{
    public const string ExecutiveSummary = "executive-summary";
    public const string AcademicByGrade = "academic-by-grade";
    public const string DataQuality = "data-quality";

    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        ExecutiveSummary,
        AcademicByGrade,
        DataQuality
    };

    /// <summary>
    /// Ghi chú: IsSupported kiểm tra mã mẫu báo cáo có thuộc catalog quản trị được công bố hay không.
    /// </summary>
    public static bool IsSupported(string reportType) => Supported.Contains(reportType);
}

/// <summary>
/// Ghi chú: AdminAnalyticsReportData chứa ba dataset Overview, Academic và Data Quality để dựng XtraReport.
/// </summary>
public sealed record AdminAnalyticsReportData(
    AdminOverviewResponse Overview,
    AdminAcademicAnalyticsResponse Academic,
    AdminDataQualityResponse DataQuality);

/// <summary>
/// Ghi chú: RenderedAdminAnalyticsReport chứa bytes và metadata do DevExpress report renderer tạo ra.
/// </summary>
public sealed record RenderedAdminAnalyticsReport(byte[] Content, string ContentType, string FileExtension);

/// <summary>
/// Ghi chú: AdminAnalyticsReportFileResponse chứa file báo cáo đã authorize để API trả về cho SystemAdmin.
/// </summary>
public sealed record AdminAnalyticsReportFileResponse(string FileName, string ContentType, byte[] Content);
