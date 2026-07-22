using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;

namespace EduHub.Application.Contracts.Analytics;

/// <summary>
/// Ghi chú: GetAdminOverviewQuery đọc số liệu tổng quan của trường theo một học kỳ dành cho SystemAdmin.
/// </summary>
public sealed record GetAdminOverviewQuery(Guid? SemesterId) : IQuery<Result<AdminOverviewResponse>>;

/// <summary>
/// Ghi chú: GetAdminAcademicAnalyticsQuery đọc thống kê điểm đã chuẩn hóa theo môn, lớp và trạng thái điểm của một học kỳ.
/// </summary>
public sealed record GetAdminAcademicAnalyticsQuery(Guid? SemesterId) : IQuery<Result<AdminAcademicAnalyticsResponse>>;

/// <summary>
/// Ghi chú: GetAdminDataQualityQuery đọc các lỗi thiếu hoặc không nhất quán trong dữ liệu học vụ của một học kỳ.
/// </summary>
public sealed record GetAdminDataQualityQuery(Guid? SemesterId) : IQuery<Result<AdminDataQualityResponse>>;

/// <summary>
/// Ghi chú: AnalyticsSemesterResponse mô tả học kỳ được dùng làm phạm vi tính toán dashboard quản trị.
/// </summary>
public sealed record AnalyticsSemesterResponse(
    Guid Id,
    Guid AcademicYearId,
    string Name,
    string AcademicYearName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status);

/// <summary>
/// Ghi chú: AnalyticsSemesterContext chứa học kỳ đang chọn và danh sách học kỳ để SystemAdmin đổi bộ lọc dashboard.
/// </summary>
public sealed record AnalyticsSemesterContext(
    AnalyticsSemesterResponse SelectedSemester,
    IReadOnlyList<AnalyticsSemesterResponse> AvailableSemesters);

/// <summary>
/// Ghi chú: UserRoleCountResponse chứa số tài khoản đang hoạt động của một role cụ thể.
/// </summary>
public sealed record UserRoleCountResponse(string Role, int Count);

/// <summary>
/// Ghi chú: GradeLevelEnrollmentCountResponse chứa số học sinh đang xếp lớp trong một khối của học kỳ.
/// </summary>
public sealed record GradeLevelEnrollmentCountResponse(int GradeLevel, int StudentCount);

/// <summary>
/// Ghi chú: AdminOverviewResponse chứa KPI con người, lớp học và hàng đợi nghiệp vụ của SystemAdmin.
/// </summary>
public sealed record AdminOverviewResponse(
    AnalyticsSemesterResponse Semester,
    IReadOnlyList<AnalyticsSemesterResponse> AvailableSemesters,
    DateTime GeneratedAtUtc,
    int ActiveStudents,
    int ActiveTeachers,
    int ActiveParents,
    int ActiveClasses,
    int ActiveSubjects,
    int PendingProfileChangeRequests,
    int OpenReportRequests,
    int PendingOutboxMessages,
    int FailedExternalSyncs,
    IReadOnlyList<UserRoleCountResponse> UsersByRole,
    IReadOnlyList<GradeLevelEnrollmentCountResponse> StudentsByGradeLevel);

/// <summary>
/// Ghi chú: GradeDistributionBucketResponse chứa số điểm đã công bố thuộc một khoảng điểm chuẩn hóa thang 10.
/// </summary>
public sealed record GradeDistributionBucketResponse(string Label, decimal FromInclusive, decimal? ToExclusive, int Count);

/// <summary>
/// Ghi chú: SubjectPerformanceResponse chứa điểm trung bình và tỷ lệ đạt của một môn trong học kỳ.
/// </summary>
public sealed record SubjectPerformanceResponse(
    string SubjectCode,
    string SubjectName,
    decimal? AverageNormalizedScore,
    decimal? PassRatePercentage,
    int PublishedGradeCount);

/// <summary>
/// Ghi chú: ClassPerformanceResponse chứa điểm trung bình và tỷ lệ đạt của một lớp trong học kỳ.
/// </summary>
public sealed record ClassPerformanceResponse(
    string ClassCode,
    string ClassName,
    int GradeLevel,
    decimal? AverageNormalizedScore,
    decimal? PassRatePercentage,
    int PublishedGradeCount);

/// <summary>
/// Ghi chú: GradeStatusCountResponse chứa số GradeEntry thuộc một trạng thái Draft, Submitted, Published hoặc Locked.
/// </summary>
public sealed record GradeStatusCountResponse(string Status, int Count);

/// <summary>
/// Ghi chú: AdminAcademicAnalyticsResponse chứa dataset điểm học kỳ để DevExpress vẽ biểu đồ học lực toàn trường.
/// </summary>
public sealed record AdminAcademicAnalyticsResponse(
    AnalyticsSemesterResponse Semester,
    DateTime GeneratedAtUtc,
    decimal? AverageNormalizedScore,
    decimal? PassRatePercentage,
    int PublishedGradeCount,
    int TotalGradeCount,
    IReadOnlyList<GradeDistributionBucketResponse> GradeDistribution,
    IReadOnlyList<SubjectPerformanceResponse> SubjectPerformance,
    IReadOnlyList<ClassPerformanceResponse> ClassPerformance,
    IReadOnlyList<GradeStatusCountResponse> GradeStatuses);

/// <summary>
/// Ghi chú: DataQualityIssueResponse mô tả một loại lỗi dữ liệu, mức độ và số bản ghi đang bị ảnh hưởng.
/// </summary>
public sealed record DataQualityIssueResponse(string Code, string Title, string Severity, int Count);

/// <summary>
/// Ghi chú: AdminDataQualityResponse chứa các lỗi dữ liệu học sinh, lớp, giáo viên và thời khóa biểu cần quản trị xử lý.
/// </summary>
public sealed record AdminDataQualityResponse(
    AnalyticsSemesterResponse Semester,
    DateTime GeneratedAtUtc,
    int TotalFindings,
    int CriticalFindings,
    IReadOnlyList<DataQualityIssueResponse> Issues);
