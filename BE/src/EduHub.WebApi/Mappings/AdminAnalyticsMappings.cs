using EduHub.Application.Contracts.Analytics;
using EduHub.WebApi.Dtos.Analytics;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: AdminAnalyticsMappings chuyển bộ lọc API sang query và Application response sang DTO dashboard.
/// </summary>
public static class AdminAnalyticsMappings
{
    /// <summary>
    /// Ghi chú: ToOverviewQuery chuyển semesterId trên API thành yêu cầu đọc KPI tổng quan.
    /// </summary>
    public static GetAdminOverviewQuery ToOverviewQuery(this GetAdminAnalyticsRequest request) => new(request.SemesterId);

    /// <summary>
    /// Ghi chú: ToAcademicQuery chuyển semesterId trên API thành yêu cầu đọc thống kê điểm.
    /// </summary>
    public static GetAdminAcademicAnalyticsQuery ToAcademicQuery(this GetAdminAnalyticsRequest request) => new(request.SemesterId);

    /// <summary>
    /// Ghi chú: ToDataQualityQuery chuyển semesterId trên API thành yêu cầu kiểm tra chất lượng dữ liệu.
    /// </summary>
    public static GetAdminDataQualityQuery ToDataQualityQuery(this GetAdminAnalyticsRequest request) => new(request.SemesterId);

    /// <summary>
    /// Ghi chú: ToExportQuery chuyển bộ lọc API thành yêu cầu xuất đúng mẫu DevExpress report, mặc định là Executive Summary.
    /// </summary>
    public static ExportAdminAnalyticsReportQuery ToExportQuery(this ExportAdminAnalyticsReportRequest request) =>
        new(
            request.SemesterId,
            string.IsNullOrWhiteSpace(request.Format) ? "pdf" : request.Format,
            string.IsNullOrWhiteSpace(request.ReportType) ? AdminAnalyticsReportTypes.ExecutiveSummary : request.ReportType);

    /// <summary>
    /// Ghi chú: ToDto chuyển KPI tổng quan Application thành DTO không lộ AcademicYearId nội bộ.
    /// </summary>
    public static AdminOverviewDto ToDto(this AdminOverviewResponse response) =>
        new(
            response.Semester.ToDto(),
            response.AvailableSemesters.Select(ToDto).ToList(),
            response.GeneratedAtUtc,
            response.ActiveStudents,
            response.ActiveTeachers,
            response.ActiveParents,
            response.ActiveClasses,
            response.ActiveSubjects,
            response.PendingProfileChangeRequests,
            response.OpenReportRequests,
            response.PendingOutboxMessages,
            response.FailedExternalSyncs,
            response.UsersByRole.Select(item => new UserRoleCountDto(item.Role, item.Count)).ToList(),
            response.StudentsByGradeLevel.Select(item => new GradeLevelEnrollmentCountDto(item.GradeLevel, item.StudentCount)).ToList());

    /// <summary>
    /// Ghi chú: ToDto chuyển dataset điểm Application thành DTO cho DevExpress charts và grids.
    /// </summary>
    public static AdminAcademicAnalyticsDto ToDto(this AdminAcademicAnalyticsResponse response) =>
        new(
            response.Semester.ToDto(),
            response.GeneratedAtUtc,
            response.AverageNormalizedScore,
            response.PassRatePercentage,
            response.PublishedGradeCount,
            response.TotalGradeCount,
            response.GradeDistribution.Select(item => new GradeDistributionBucketDto(item.Label, item.FromInclusive, item.ToExclusive, item.Count)).ToList(),
            response.SubjectPerformance.Select(item => new SubjectPerformanceDto(item.SubjectCode, item.SubjectName, item.AverageNormalizedScore, item.PassRatePercentage, item.PublishedGradeCount)).ToList(),
            response.ClassPerformance.Select(item => new ClassPerformanceDto(item.ClassCode, item.ClassName, item.GradeLevel, item.AverageNormalizedScore, item.PassRatePercentage, item.PublishedGradeCount)).ToList(),
            response.GradeStatuses.Select(item => new GradeStatusCountDto(item.Status, item.Count)).ToList());

    /// <summary>
    /// Ghi chú: ToDto chuyển danh sách lỗi chất lượng dữ liệu Application thành DTO dashboard.
    /// </summary>
    public static AdminDataQualityDto ToDto(this AdminDataQualityResponse response) =>
        new(
            response.Semester.ToDto(),
            response.GeneratedAtUtc,
            response.TotalFindings,
            response.CriticalFindings,
            response.Issues.Select(item => new DataQualityIssueDto(item.Code, item.Title, item.Severity, item.Count)).ToList());

    /// <summary>
    /// Ghi chú: ToDto chuyển học kỳ analytics thành DTO bộ lọc không chứa AcademicYearId nội bộ.
    /// </summary>
    private static AnalyticsSemesterDto ToDto(this AnalyticsSemesterResponse response) =>
        new(response.Id, response.Name, response.AcademicYearName, response.StartDate, response.EndDate, response.Status);
}
