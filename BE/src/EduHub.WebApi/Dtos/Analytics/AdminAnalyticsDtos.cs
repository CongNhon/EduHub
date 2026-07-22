namespace EduHub.WebApi.Dtos.Analytics;

/// <summary>
/// Ghi chú: GetAdminAnalyticsRequest chứa học kỳ tùy chọn mà SystemAdmin dùng để lọc dashboard.
/// </summary>
public sealed record GetAdminAnalyticsRequest(Guid? SemesterId);

/// <summary>
/// Ghi chú: ExportAdminAnalyticsReportRequest chứa học kỳ, format file và mẫu báo cáo quản trị cần xuất.
/// </summary>
public sealed record ExportAdminAnalyticsReportRequest(Guid? SemesterId, string? Format, string? ReportType);

/// <summary>
/// Ghi chú: AnalyticsSemesterDto trả tên, niên khóa và thời gian của học kỳ đang thống kê.
/// </summary>
public sealed record AnalyticsSemesterDto(
    Guid Id,
    string Name,
    string AcademicYearName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status);

/// <summary>
/// Ghi chú: UserRoleCountDto trả số tài khoản active theo từng role cho biểu đồ quản trị.
/// </summary>
public sealed record UserRoleCountDto(string Role, int Count);

/// <summary>
/// Ghi chú: GradeLevelEnrollmentCountDto trả số học sinh đang học theo từng khối.
/// </summary>
public sealed record GradeLevelEnrollmentCountDto(int GradeLevel, int StudentCount);

/// <summary>
/// Ghi chú: AdminOverviewDto trả KPI tổng quan và hàng đợi nghiệp vụ của trường.
/// </summary>
public sealed record AdminOverviewDto(
    AnalyticsSemesterDto Semester,
    IReadOnlyList<AnalyticsSemesterDto> AvailableSemesters,
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
    IReadOnlyList<UserRoleCountDto> UsersByRole,
    IReadOnlyList<GradeLevelEnrollmentCountDto> StudentsByGradeLevel);

/// <summary>
/// Ghi chú: GradeDistributionBucketDto trả số lượng điểm thuộc một khoảng điểm chuẩn hóa thang 10.
/// </summary>
public sealed record GradeDistributionBucketDto(string Label, decimal FromInclusive, decimal? ToExclusive, int Count);

/// <summary>
/// Ghi chú: SubjectPerformanceDto trả điểm trung bình, tỷ lệ đạt và số điểm công bố của một môn.
/// </summary>
public sealed record SubjectPerformanceDto(
    string SubjectCode,
    string SubjectName,
    decimal? AverageNormalizedScore,
    decimal? PassRatePercentage,
    int PublishedGradeCount);

/// <summary>
/// Ghi chú: ClassPerformanceDto trả điểm trung bình, tỷ lệ đạt và số điểm công bố của một lớp.
/// </summary>
public sealed record ClassPerformanceDto(
    string ClassCode,
    string ClassName,
    int GradeLevel,
    decimal? AverageNormalizedScore,
    decimal? PassRatePercentage,
    int PublishedGradeCount);

/// <summary>
/// Ghi chú: GradeStatusCountDto trả số GradeEntry theo trạng thái xử lý điểm.
/// </summary>
public sealed record GradeStatusCountDto(string Status, int Count);

/// <summary>
/// Ghi chú: AdminAcademicAnalyticsDto trả dataset học lực toàn trường theo học kỳ.
/// </summary>
public sealed record AdminAcademicAnalyticsDto(
    AnalyticsSemesterDto Semester,
    DateTime GeneratedAtUtc,
    decimal? AverageNormalizedScore,
    decimal? PassRatePercentage,
    int PublishedGradeCount,
    int TotalGradeCount,
    IReadOnlyList<GradeDistributionBucketDto> GradeDistribution,
    IReadOnlyList<SubjectPerformanceDto> SubjectPerformance,
    IReadOnlyList<ClassPerformanceDto> ClassPerformance,
    IReadOnlyList<GradeStatusCountDto> GradeStatuses);

/// <summary>
/// Ghi chú: DataQualityIssueDto trả mã lỗi, mô tả, mức độ và số bản ghi dữ liệu cần xử lý.
/// </summary>
public sealed record DataQualityIssueDto(string Code, string Title, string Severity, int Count);

/// <summary>
/// Ghi chú: AdminDataQualityDto trả tổng số phát hiện và chi tiết chất lượng dữ liệu học vụ.
/// </summary>
public sealed record AdminDataQualityDto(
    AnalyticsSemesterDto Semester,
    DateTime GeneratedAtUtc,
    int TotalFindings,
    int CriticalFindings,
    IReadOnlyList<DataQualityIssueDto> Issues);
