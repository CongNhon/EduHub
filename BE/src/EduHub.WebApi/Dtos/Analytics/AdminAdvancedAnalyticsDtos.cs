namespace EduHub.WebApi.Dtos.Analytics;

/// <summary>
/// Ghi chú: AdminAdvancedAnalyticsRequest chứa các tham số query từ WebApi.
/// </summary>
public sealed record AdminAdvancedAnalyticsRequest(
    Guid? SemesterId,
    Guid? PreviousSemesterId,
    int[]? GradeLevels,
    Guid[]? ClassIds,
    Guid[]? SubjectIds,
    Guid[]? TeacherIds,
    string? RiskLevel = null,
    int Skip = 0,
    int Take = 20);

/// <summary>
/// Ghi chú: AdminAdvancedSummaryDto chứa dữ liệu tóm tắt cho WebApi response.
/// </summary>
public sealed record AdminAdvancedSummaryDto(
    AdvancedMetricMetadataDto Metadata,
    CommonDecimalMetricDto AverageScore,
    CommonDecimalMetricDto PassRate,
    CommonDecimalMetricDto ExcellentRate,
    CommonDecimalMetricDto MissingGradeRate,
    GrowthSummaryDto Growth,
    DataQualityScoreSummaryDto DataQuality);

/// <summary>
/// Ghi chú: AdvancedMetricMetadataDto chứa thông tin version cho WebApi response.
/// </summary>
public sealed record AdvancedMetricMetadataDto(
    string MetricVersion,
    string RiskModelVersion,
    string QualityModelVersion,
    DateTime GeneratedAt);

/// <summary>
/// Ghi chú: CommonDecimalMetricDto chứa giá trị metric và so sánh cho WebApi response.
/// </summary>
public sealed record CommonDecimalMetricDto(
    decimal? Value,
    decimal? PreviousValue,
    decimal? AbsoluteChange,
    decimal? PercentageChange,
    string Trend);

/// <summary>
/// Ghi chú: GrowthSummaryDto chứa tóm tắt tăng trưởng cho WebApi response.
/// </summary>
public sealed record GrowthSummaryDto(
    int TotalCount,
    int ImprovedCount,
    int StableCount,
    int DeclinedCount,
    decimal? MeanGrowth,
    decimal? MedianGrowth);

/// <summary>
/// Ghi chú: DataQualityScoreSummaryDto chứa các chỉ số chất lượng dữ liệu cho WebApi response.
/// </summary>
public sealed record DataQualityScoreSummaryDto(
    decimal OverallScore,
    decimal Completeness,
    decimal Validity,
    decimal Consistency,
    decimal Integrity,
    decimal Uniqueness,
    decimal Freshness);

/// <summary>
/// Ghi chú: AcademicDistributionDto chứa dữ liệu phân bổ điểm số cho WebApi response.
/// </summary>
public sealed record AcademicDistributionDto(
    AdvancedMetricMetadataDto Metadata,
    ScoreDistributionMetricsDto Overall,
    IReadOnlyList<ScoreBucketMetricDto> Buckets,
    IReadOnlyList<GroupedDistributionItemDto> Grouped);

/// <summary>
/// Ghi chú: ScoreDistributionMetricsDto chứa chỉ số thống kê cho WebApi response.
/// </summary>
public sealed record ScoreDistributionMetricsDto(
    int SampleSize,
    decimal? Mean,
    decimal? Median,
    decimal? Min,
    decimal? Max,
    decimal? StandardDeviation,
    decimal? Variance,
    decimal? P10,
    decimal? Q1,
    decimal? Q3,
    decimal? P90,
    decimal? InterquartileRange);

/// <summary>
/// Ghi chú: ScoreBucketMetricDto chứa thông tin nhóm điểm cho WebApi response.
/// </summary>
public sealed record ScoreBucketMetricDto(
    string Code,
    string Name,
    int Count,
    decimal Percentage);

/// <summary>
/// Ghi chú: GroupedDistributionItemDto chứa dữ liệu phân bổ theo nhóm cho WebApi response.
/// </summary>
public sealed record GroupedDistributionItemDto(
    string GroupKey,
    string GroupName,
    ScoreDistributionMetricsDto Metrics);

/// <summary>
/// Ghi chú: AcademicTrendDto chứa dữ liệu xu hướng cho WebApi response.
/// </summary>
public sealed record AcademicTrendDto(
    AdvancedMetricMetadataDto Metadata,
    IReadOnlyList<AcademicTrendPointDto> Points);

/// <summary>
/// Ghi chú: AcademicTrendPointDto chứa dữ liệu điểm xu hướng cho WebApi response.
/// </summary>
public sealed record AcademicTrendPointDto(
    Guid SemesterId,
    string SemesterName,
    int AcademicYearStart,
    int AcademicYearEnd,
    decimal? Mean,
    decimal? Median,
    decimal? StandardDeviation,
    decimal? PassRate,
    decimal? FailureRate,
    decimal? MissingRate,
    int ValidScoreCount,
    int StudentCount);

/// <summary>
/// Ghi chú: StudentRiskDto chứa dữ liệu rủi ro học sinh cho WebApi response.
/// </summary>
public sealed record StudentRiskDto(
    AdvancedMetricMetadataDto Metadata,
    StudentRiskSummaryDto Summary,
    IReadOnlyList<StudentRiskItemDto> Items);

/// <summary>
/// Ghi chú: StudentRiskSummaryDto chứa tóm tắt số lượng rủi ro cho WebApi response.
/// </summary>
public sealed record StudentRiskSummaryDto(
    int Total,
    int Low,
    int Medium,
    int High,
    int Critical);

/// <summary>
/// Ghi chú: StudentRiskItemDto chứa chi tiết rủi ro học sinh cho WebApi response.
/// </summary>
public sealed record StudentRiskItemDto(
    Guid StudentId,
    string StudentCode,
    string StudentName,
    Guid ClassId,
    string ClassCode,
    string ClassName,
    int GradeLevel,
    decimal RiskScore,
    string RiskLevel,
    decimal? CurrentAverage,
    decimal? PreviousAverage,
    decimal? Growth,
    int FailedSubjectCount,
    int TotalSubjectCount,
    decimal? MissingGradeRate,
    decimal? CurrentPercentileInGrade,
    IReadOnlyList<StudentRiskReasonDto> Reasons);

/// <summary>
/// Ghi chú: StudentRiskReasonDto chứa lý do rủi ro cho WebApi response.
/// </summary>
public sealed record StudentRiskReasonDto(
    string Code,
    string Message);
