using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Analytics;

namespace EduHub.Application.Interfaces.Services.Analytics;

/// <summary>
/// Ghi chú: IAdminAnalyticsSnapshotService đọc ba dataset cùng học kỳ cho report viewer và Hangfire job nội bộ.
/// </summary>
public interface IAdminAnalyticsSnapshotService
{
    /// <summary>
    /// Ghi chú: ReadAsync lấy Overview, Academic và Data Quality theo một semester context thống nhất.
    /// </summary>
    Task<Result<AdminAnalyticsReportData>> ReadAsync(Guid? semesterId, CancellationToken cancellationToken);
}
