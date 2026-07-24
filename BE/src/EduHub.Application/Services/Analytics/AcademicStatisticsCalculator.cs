using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Services.Analytics;

namespace EduHub.Application.Services.Analytics;

/// <summary>
/// Ghi chú: AcademicStatisticsCalculator thực hiện các phép tính thống kê học tập.
/// </summary>
public sealed class AcademicStatisticsCalculator : IAcademicStatisticsCalculator
{
    public ScoreDistributionMetrics CalculateDistribution(IReadOnlyCollection<decimal> scores)
    {
        if (scores == null || scores.Count == 0)
        {
            return new ScoreDistributionMetrics(0, null, null, null, null, null, null, null, null, null, null, null);
        }

        var sortedScores = scores.OrderBy(x => x).ToList();
        int count = sortedScores.Count;

        decimal mean = Math.Round(scores.Average(), 2);
        decimal min = Math.Round(sortedScores[0], 2);
        decimal max = Math.Round(sortedScores[count - 1], 2);
        decimal median = Math.Round(GetPercentile(sortedScores, 0.5m), 2);
        
        decimal sumOfSquares = scores.Sum(x => (x - mean) * (x - mean));
        decimal variance = count > 1 ? sumOfSquares / (count - 1) : 0;
        decimal stdDev = (decimal)Math.Sqrt((double)variance);

        decimal p10 = Math.Round(GetPercentile(sortedScores, 0.1m), 2);
        decimal q1 = Math.Round(GetPercentile(sortedScores, 0.25m), 2);
        decimal q3 = Math.Round(GetPercentile(sortedScores, 0.75m), 2);
        decimal p90 = Math.Round(GetPercentile(sortedScores, 0.9m), 2);
        decimal iqr = Math.Round(q3 - q1, 2);

        return new ScoreDistributionMetrics(
            count, mean, median, min, max, Math.Round(stdDev, 2), Math.Round(variance, 2), p10, q1, q3, p90, iqr);
    }

    public IReadOnlyList<ScoreBucketMetric> CalculateBuckets(IReadOnlyCollection<decimal> scores)
    {
        var buckets = new List<(string Code, string Name, decimal Min, decimal Max)>
        {
            ("VERY_POOR", "Kém (0-3)", 0, 3),
            ("POOR", "Yếu (3-5)", 3, 5),
            ("AVERAGE", "Trung bình (5-6.5)", 5, 6.5m),
            ("GOOD", "Khá (6.5-8)", 6.5m, 8),
            ("EXCELLENT", "Giỏi (8-9)", 8, 9),
            ("OUTSTANDING", "Xuất sắc (9-10)", 9, 10.0001m)
        };

        var result = new List<ScoreBucketMetric>();
        int totalCount = scores?.Count ?? 0;

        foreach (var b in buckets)
        {
            int count = scores?.Count(x => x >= b.Min && x < b.Max) ?? 0;
            decimal percentage = totalCount > 0 ? (decimal)count / totalCount * 100 : 0;
            result.Add(new ScoreBucketMetric(b.Code, b.Name, count, Math.Round(percentage, 1)));
        }

        return result;
    }

    public RateMetric CalculateThresholdRate(IReadOnlyCollection<decimal> scores, Func<decimal, bool> predicate)
    {
        if (scores == null || scores.Count == 0) return new RateMetric(0, null);
        int count = scores.Count(predicate);
        return new RateMetric(count, Math.Round((decimal)count / scores.Count * 100, 1));
    }

    public decimal? CalculatePercentileRank(decimal score, IReadOnlyCollection<decimal> referenceScores)
    {
        if (referenceScores == null || referenceScores.Count == 0) return null;
        int countBelow = referenceScores.Count(x => x < score);
        int countEqual = referenceScores.Count(x => x == score);
        return (decimal)(countBelow + 0.5m * countEqual) / referenceScores.Count * 100;
    }

    private static decimal GetPercentile(List<decimal> sortedScores, decimal percentile)
    {
        int n = sortedScores.Count;
        if (n == 0) return 0;
        if (n == 1) return sortedScores[0];

        decimal realIndex = percentile * (n - 1);
        int index = (int)realIndex;
        decimal fraction = realIndex - index;

        if (index >= n - 1) return sortedScores[n - 1];
        return sortedScores[index] + fraction * (sortedScores[index + 1] - sortedScores[index]);
    }
}
