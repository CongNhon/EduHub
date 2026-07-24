namespace EduHub.Application.Contracts.Analytics;

/// <summary>
/// Ghi chú: AdminAdvancedAnalyticsFilter chứa các điều kiện lọc cho báo cáo phân tích nâng cao.
/// </summary>
public sealed record AdminAdvancedAnalyticsFilter(
    Guid? SemesterId = null,
    Guid? PreviousSemesterId = null,
    IReadOnlyList<int>? GradeLevels = null,
    IReadOnlyList<Guid>? ClassIds = null,
    IReadOnlyList<Guid>? SubjectIds = null,
    IReadOnlyList<Guid>? TeacherIds = null,
    string? RiskLevel = null,
    int Skip = 0,
    int Take = 20);

/// <summary>
/// Ghi chú: AdvancedMetricMetadata chứa thông tin version của metric.
/// </summary>
public sealed record AdvancedMetricMetadata(
    string MetricVersion,
    string RiskModelVersion,
    string QualityModelVersion,
    DateTime GeneratedAt);

/// <summary>
/// Ghi chú: CommonDecimalMetric chứa giá trị metric và so sánh với kỳ trước.
/// </summary>
public sealed record CommonDecimalMetric(
    decimal? Value,
    decimal? PreviousValue,
    decimal? AbsoluteChange,
    decimal? PercentageChange,
    string Trend);

/// <summary>
/// Ghi chú: ComparativeDecimalMetric chứa kết quả so sánh giữa 2 giá trị decimal.
/// </summary>
public sealed record ComparativeDecimalMetric(
    decimal? AbsoluteChange,
    decimal? PercentageChange,
    string Trend);

/// <summary>
/// Ghi chú: RateMetric chứa giá trị tỉ lệ phần trăm.
/// </summary>
public sealed record RateMetric(
    int Count,
    decimal? Percentage);

/// <summary>
/// Ghi chú: ScoreDistributionMetrics chứa các chỉ số thống kê mô tả cho tập điểm.
/// </summary>
public sealed record ScoreDistributionMetrics(
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
/// Ghi chú: ScoreBucketMetric chứa thông tin một nhóm điểm.
/// </summary>
public sealed record ScoreBucketMetric(
    string Code,
    string Name,
    int Count,
    decimal Percentage);

/// <summary>
/// Ghi chú: GrowthSummary chứa tóm tắt về sự tăng trưởng của một nhóm học sinh.
/// </summary>
public sealed record GrowthSummary(
    int TotalCount,
    int ImprovedCount,
    int StableCount,
    int DeclinedCount,
    decimal? MeanGrowth,
    decimal? MedianGrowth);

/// <summary>
/// Ghi chú: AdminAdvancedSummaryResponse chứa dữ liệu tóm tắt cho dashboard admin.
/// </summary>
public sealed record AdminAdvancedSummaryResponse(
    AdvancedMetricMetadata Metadata,
    CommonDecimalMetric AverageScore,
    CommonDecimalMetric PassRate,
    CommonDecimalMetric ExcellentRate,
    CommonDecimalMetric MissingGradeRate,
    GrowthSummary Growth,
    DataQualityScoreSummary DataQuality);

/// <summary>
/// Ghi chú: AcademicDistributionResponse chứa dữ liệu phân bổ điểm số.
/// </summary>
public sealed record AcademicDistributionResponse(
    AdvancedMetricMetadata Metadata,
    ScoreDistributionMetrics Overall,
    IReadOnlyList<ScoreBucketMetric> Buckets,
    IReadOnlyList<GroupedDistributionItem> Grouped);

/// <summary>
/// Ghi chú: GroupedDistributionItem chứa dữ liệu phân bổ cho một nhóm (lớp, môn...).
/// </summary>
public sealed record GroupedDistributionItem(
    string GroupKey,
    string GroupName,
    ScoreDistributionMetrics Metrics);

/// <summary>
/// Ghi chú: AcademicTrendResponse chứa dữ liệu xu hướng qua các học kỳ.
/// </summary>
public sealed record AcademicTrendResponse(
    AdvancedMetricMetadata Metadata,
    IReadOnlyList<AcademicTrendPoint> Points);

/// <summary>
/// Ghi chú: AcademicTrendPoint chứa dữ liệu tại một thời điểm (học kỳ) trong xu hướng.
/// </summary>
public sealed record AcademicTrendPoint(
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
/// Ghi chú: StudentRiskSummary chứa tóm tắt số lượng học sinh theo mức độ rủi ro.
/// </summary>
public sealed record StudentRiskSummary(
    int Total,
    int Low,
    int Medium,
    int High,
    int Critical);

/// <summary>
/// Ghi chú: StudentRiskReason chứa mã và thông báo lý do rủi ro.
/// </summary>
public sealed record StudentRiskReason(
    string Code,
    string Message);

/// <summary>
/// Ghi chú: StudentRiskItem chứa thông tin rủi ro chi tiết của một học sinh.
/// </summary>
public sealed record StudentRiskItem(
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
    IReadOnlyList<StudentRiskReason> Reasons);

/// <summary>
/// Ghi chú: StudentRiskResponse chứa danh sách học sinh có rủi ro và tóm tắt.
/// </summary>
public sealed record StudentRiskResponse(
    AdvancedMetricMetadata Metadata,
    StudentRiskSummary Summary,
    IReadOnlyList<StudentRiskItem> Items);

/// <summary>
/// Ghi chú: DataQualityScoreSummary chứa các chỉ số chất lượng dữ liệu.
/// </summary>
public sealed record DataQualityScoreSummary(
    decimal OverallScore,
    decimal Completeness,
    decimal Validity,
    decimal Consistency,
    decimal Integrity,
    decimal Uniqueness,
    decimal Freshness);

/// <summary>
/// Ghi chú: StudentRiskInput chứa dữ liệu đầu vào để tính toán rủi ro cho một học sinh.
/// </summary>
public sealed record StudentRiskInput(
    Guid StudentId,
    string StudentCode,
    string StudentName,
    Guid ClassId,
    string ClassCode,
    string ClassName,
    int GradeLevel,
    decimal? CurrentAverage,
    decimal? PreviousAverage,
    decimal? MissingGradeRate,
    int FailedSubjectCount,
    int TotalSubjectCount,
    decimal? CurrentPercentileInGrade);
