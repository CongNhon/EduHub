using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Repositories.Analytics;
using EduHub.Application.Interfaces.Services.Analytics;

namespace EduHub.Application.Services.Analytics;

/// <summary>
/// Ghi chú: DataQualityScoreCalculator thực hiện tính toán điểm chất lượng dữ liệu.
/// </summary>
public sealed class DataQualityScoreCalculator : IDataQualityScoreCalculator
{
    public DataQualityScoreSummary Calculate(DataQualityRawSnapshot snapshot)
    {
        if (snapshot == null || snapshot.Dimensions == null || snapshot.Dimensions.Count == 0)
        {
            return new DataQualityScoreSummary(0, 0, 0, 0, 0, 0, 0);
        }

        var scores = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var dim in snapshot.Dimensions)
        {
            decimal score = dim.EligibleRecordCount > 0 
                ? (decimal)(dim.EligibleRecordCount - dim.AffectedRecordCount) / dim.EligibleRecordCount * 100 
                : 100;
            scores[dim.Code] = score;
        }

        decimal completeness = scores.GetValueOrDefault("COMPLETENESS", 100);
        decimal validity = scores.GetValueOrDefault("VALIDITY", 100);
        decimal consistency = scores.GetValueOrDefault("CONSISTENCY", 100);
        decimal integrity = scores.GetValueOrDefault("INTEGRITY", 100);
        decimal uniqueness = scores.GetValueOrDefault("UNIQUENESS", 100);
        decimal freshness = scores.GetValueOrDefault("FRESHNESS", 100);

        decimal overall = (completeness + validity + consistency + integrity + uniqueness + freshness) / 6;

        return new DataQualityScoreSummary(overall, completeness, validity, consistency, integrity, uniqueness, freshness);
    }
}
