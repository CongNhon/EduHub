using DevExpress.XtraReports.UI;
using EduHub.Application.Contracts.Analytics;

namespace EduHub.Infrastructure.Services.Reports;

/// <summary>
/// Ghi chú: DevExpressAdminReportCatalog ánh xạ mã report của Application và reportUrl của Web Document Viewer sang đúng XtraReport.
/// </summary>
internal static class DevExpressAdminReportCatalog
{
    public const string ExecutiveSummaryViewerName = "admin-system-analytics";
    public const string AcademicByGradeViewerName = "admin-academic-by-grade";
    public const string DataQualityViewerName = "admin-data-quality";
    public const string ScoreDistributionViewerName = "admin-score-distribution";
    public const string AcademicTrendViewerName = "admin-academic-trend";
    public const string StudentRiskViewerName = "admin-student-risk";

    /// <summary>
    /// Ghi chú: Create tạo đúng mẫu báo cáo tổng quan, học lực theo khối hoặc chất lượng dữ liệu theo mức độ.
    /// </summary>
    public static XtraReport Create(string reportType, AdminAnalyticsReportData data) => reportType switch
    {
        AdminAnalyticsReportTypes.ExecutiveSummary => DevExpressAdminAnalyticsReportFactory.Create(data),
        AdminAnalyticsReportTypes.AcademicByGrade => DevExpressGroupedAnalyticsReportFactory.CreateAcademicByGrade(data),
        AdminAnalyticsReportTypes.DataQuality => DevExpressGroupedAnalyticsReportFactory.CreateDataQuality(data),
        AdminAnalyticsReportTypes.ScoreDistribution => DevExpressAdvancedAnalyticsReportFactory.CreateScoreDistribution(data),
        AdminAnalyticsReportTypes.AcademicTrend => DevExpressAdvancedAnalyticsReportFactory.CreateAcademicTrend(data),
        AdminAnalyticsReportTypes.StudentRisk => DevExpressAdvancedAnalyticsReportFactory.CreateStudentRisk(data),
        _ => throw new InvalidOperationException($"Unknown analytics report type '{reportType}'.")
    };

    /// <summary>
    /// Ghi chú: ParseViewerId tách tên mẫu report và semesterId từ reportUrl do Web Document Viewer gửi lên.
    /// </summary>
    public static ParsedAdminReport ParseViewerId(string id)
    {
        var separatorIndex = id.LastIndexOf("--", StringComparison.Ordinal);
        var viewerName = separatorIndex < 0 ? id : id[..separatorIndex];
        Guid? semesterId = null;

        if (separatorIndex >= 0)
        {
            if (!Guid.TryParse(id[(separatorIndex + 2)..], out var parsedSemesterId))
            {
                throw new InvalidOperationException("Invalid semester identifier in DevExpress report name.");
            }

            semesterId = parsedSemesterId;
        }

        var reportType = viewerName switch
        {
            ExecutiveSummaryViewerName => AdminAnalyticsReportTypes.ExecutiveSummary,
            AcademicByGradeViewerName => AdminAnalyticsReportTypes.AcademicByGrade,
            DataQualityViewerName => AdminAnalyticsReportTypes.DataQuality,
            ScoreDistributionViewerName => AdminAnalyticsReportTypes.ScoreDistribution,
            AcademicTrendViewerName => AdminAnalyticsReportTypes.AcademicTrend,
            StudentRiskViewerName => AdminAnalyticsReportTypes.StudentRisk,
            _ => throw new InvalidOperationException("Unknown DevExpress report name.")
        };

        return new ParsedAdminReport(reportType, semesterId);
    }

    /// <summary>
    /// Ghi chú: ParsedAdminReport chứa mẫu report và học kỳ đã phân giải từ URL của Viewer.
    /// </summary>
    internal sealed record ParsedAdminReport(string ReportType, Guid? SemesterId);
}
