using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Analytics;

namespace EduHub.Application.Interfaces.Services.Analytics;

/// <summary>
/// Ghi chú: IAdminAdvancedAnalyticsService định nghĩa các nghiệp vụ phân tích nâng cao cho admin.
/// </summary>
public interface IAdminAdvancedAnalyticsService
{
    Task<Result<AdminAdvancedSummaryResponse>> ReadSummaryAsync(
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken);

    Task<Result<AcademicDistributionResponse>> ReadDistributionAsync(
        AdminAdvancedAnalyticsFilter filter,
        string groupBy,
        CancellationToken cancellationToken);

    Task<Result<AcademicTrendResponse>> ReadTrendsAsync(
        AdminAdvancedAnalyticsFilter filter,
        int maxSemesters,
        CancellationToken cancellationToken);

    Task<Result<StudentRiskResponse>> ReadStudentRiskAsync(
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken);
}
