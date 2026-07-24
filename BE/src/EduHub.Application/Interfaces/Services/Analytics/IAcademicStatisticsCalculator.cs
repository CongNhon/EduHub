using EduHub.Application.Contracts.Analytics;

namespace EduHub.Application.Interfaces.Services.Analytics;

/// <summary>
/// Ghi chú: IAcademicStatisticsCalculator định nghĩa các phương thức tính toán thống kê học tập.
/// </summary>
public interface IAcademicStatisticsCalculator
{
    ScoreDistributionMetrics CalculateDistribution(
        IReadOnlyCollection<decimal> scores);

    IReadOnlyList<ScoreBucketMetric> CalculateBuckets(
        IReadOnlyCollection<decimal> scores);

    RateMetric CalculateThresholdRate(
        IReadOnlyCollection<decimal> scores,
        Func<decimal, bool> predicate);

    decimal? CalculatePercentileRank(
        decimal score,
        IReadOnlyCollection<decimal> referenceScores);
}
