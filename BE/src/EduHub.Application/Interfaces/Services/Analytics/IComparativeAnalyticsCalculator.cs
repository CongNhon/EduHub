using EduHub.Application.Contracts.Analytics;

namespace EduHub.Application.Interfaces.Services.Analytics;

/// <summary>
/// Ghi chú: IComparativeAnalyticsCalculator định nghĩa các phương thức so sánh giữa các kỳ hoặc các nhóm.
/// </summary>
public interface IComparativeAnalyticsCalculator
{
    ComparativeDecimalMetric Compare(decimal? current, decimal? previous);
    GrowthSummary CalculateGrowth(IReadOnlyCollection<decimal> growthValues);
}
