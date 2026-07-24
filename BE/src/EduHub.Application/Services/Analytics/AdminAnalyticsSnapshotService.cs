using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Repositories.Analytics;
using EduHub.Application.Interfaces.Services.Analytics;

namespace EduHub.Application.Services.Analytics;

/// <summary>
/// Ghi chú: AdminAnalyticsSnapshotService đọc trực tiếp read-model repository cho report và background job nội bộ.
/// </summary>
public sealed class AdminAnalyticsSnapshotService(
    IAdminAnalyticsRepository repository,
    IAdminAdvancedAnalyticsService advancedAnalyticsService,
    TimeProvider timeProvider) : IAdminAnalyticsSnapshotService
{
    /// <summary>
    /// Ghi chú: ReadAsync khóa học kỳ một lần rồi tổng hợp các dataset với cùng thời điểm tạo.
    /// </summary>
    public async Task<Result<AdminAnalyticsReportData>> ReadAsync(Guid? semesterId, CancellationToken cancellationToken)
    {
        var context = await repository.ResolveSemesterContextAsync(semesterId, cancellationToken);
        if (context is null)
        {
            return Result.Failure<AdminAnalyticsReportData>(new Error(
                "Analytics.SemesterNotFound",
                "The requested semester was not found and no fallback semester is available.",
                ErrorType.NotFound));
        }

        var generatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var overview = await repository.GetOverviewAsync(context, generatedAtUtc, cancellationToken);
        var academic = await repository.GetAcademicAnalyticsAsync(context.SelectedSemester, generatedAtUtc, cancellationToken);
        var dataQuality = await repository.GetDataQualityAsync(context.SelectedSemester, generatedAtUtc, cancellationToken);

        // Fetch advanced analytics data with default settings
        var filter = new AdminAdvancedAnalyticsFilter(SemesterId: context.SelectedSemester.Id);

        var distributionTask = advancedAnalyticsService.ReadDistributionAsync(filter, "class", cancellationToken);
        var trendTask = advancedAnalyticsService.ReadTrendsAsync(filter, 4, cancellationToken);
        var riskTask = advancedAnalyticsService.ReadStudentRiskAsync(filter, cancellationToken);

        await Task.WhenAll(distributionTask, trendTask, riskTask);

        var distribution = (await distributionTask).Value;
        var trends = (await trendTask).Value;
        var risk = (await riskTask).Value;

        return Result.Success(new AdminAnalyticsReportData(
            overview,
            academic,
            dataQuality,
            distribution,
            trends,
            risk));
    }
}
