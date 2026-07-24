using EduHub.Application.Common.Options;
using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Services.Analytics;
using Microsoft.Extensions.Options;

namespace EduHub.Application.Services.Analytics;

/// <summary>
/// Ghi chú: ComparativeAnalyticsCalculator thực hiện các phép tính so sánh chỉ số với làm tròn và ngưỡng cấu hình.
/// </summary>
public sealed class ComparativeAnalyticsCalculator : IComparativeAnalyticsCalculator
{
    private readonly AdvancedAnalyticsOptions _options;

    public ComparativeAnalyticsCalculator(IOptions<AdvancedAnalyticsOptions> options)
    {
        _options = options.Value;
    }

    public ComparativeDecimalMetric Compare(decimal? current, decimal? previous)
    {
        if (current == null || previous == null || previous == 0)
        {
            return new ComparativeDecimalMetric(null, null, "STABLE");
        }

        decimal absoluteChange = Math.Round(current.Value - previous.Value, 2);
        decimal percentageChange = Math.Round(absoluteChange / Math.Abs(previous.Value) * 100, 1);
        
        string trend = absoluteChange >= _options.StableGrowthAbsoluteThreshold ? "UP" : 
                       absoluteChange <= -_options.StableGrowthAbsoluteThreshold ? "DOWN" : "STABLE";

        return new ComparativeDecimalMetric(absoluteChange, percentageChange, trend);
    }

    public GrowthSummary CalculateGrowth(IReadOnlyCollection<decimal> growthValues)
    {
        if (growthValues == null || growthValues.Count == 0)
        {
            return new GrowthSummary(0, 0, 0, 0, null, null);
        }

        int totalCount = growthValues.Count;
        decimal threshold = _options.StableGrowthAbsoluteThreshold;

        int improvedCount = growthValues.Count(x => x >= threshold);
        int declinedCount = growthValues.Count(x => x <= -threshold);
        int stableCount = totalCount - improvedCount - declinedCount;

        decimal meanGrowth = Math.Round(growthValues.Average(), 2);
        var sortedGrowth = growthValues.OrderBy(x => x).ToList();
        decimal medianGrowth = Math.Round(GetMedian(sortedGrowth), 2);

        return new GrowthSummary(totalCount, improvedCount, stableCount, declinedCount, meanGrowth, medianGrowth);
    }

    private static decimal GetMedian(List<decimal> sortedList)
    {
        int count = sortedList.Count;
        if (count == 0) return 0;
        if (count % 2 == 0)
            return (sortedList[count / 2 - 1] + sortedList[count / 2]) / 2;
        return sortedList[count / 2];
    }
}
