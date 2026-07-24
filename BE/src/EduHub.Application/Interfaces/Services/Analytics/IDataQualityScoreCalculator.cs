using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Repositories.Analytics;

namespace EduHub.Application.Interfaces.Services.Analytics;

/// <summary>
/// Ghi chú: IDataQualityScoreCalculator định nghĩa phương thức tính toán điểm chất lượng dữ liệu.
/// </summary>
public interface IDataQualityScoreCalculator
{
    DataQualityScoreSummary Calculate(DataQualityRawSnapshot snapshot);
}
