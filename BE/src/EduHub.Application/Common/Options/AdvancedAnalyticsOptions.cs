namespace EduHub.Application.Common.Options;

/// <summary>
/// Ghi chú: AdvancedAnalyticsOptions chứa cấu hình cho tính năng phân tích nâng cao.
/// </summary>
public sealed class AdvancedAnalyticsOptions
{
    public decimal PassingScore { get; init; } = 5.0m;
    public decimal GoodScore { get; init; } = 6.5m;
    public decimal ExcellentScore { get; init; } = 8.0m;
    public decimal StableGrowthAbsoluteThreshold { get; init; } = 0.2m;
    public int MinimumBenchmarkSampleSize { get; init; } = 5;
    public RiskWeightsOptions RiskWeights { get; init; } = new();
}

/// <summary>
/// Ghi chú: RiskWeightsOptions chứa trọng số cho mô hình tính toán rủi ro học sinh.
/// </summary>
public sealed class RiskWeightsOptions
{
    public decimal LowPerformance { get; init; } = 0.35m;
    public decimal Decline { get; init; } = 0.25m;
    public decimal MissingGrades { get; init; } = 0.20m;
    public decimal FailedSubjects { get; init; } = 0.20m;
}
