using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Analytics;

namespace EduHub.Application.Interfaces.Services.Analytics;

/// <summary>
/// Ghi chú: IAdminAnalyticsReportService định nghĩa nghiệp vụ lấy dataset và xuất báo cáo analytics cho SystemAdmin.
/// </summary>
public interface IAdminAnalyticsReportService
{
    /// <summary>
    /// Ghi chú: GetDataAsync lấy đồng nhất ba dataset report theo cùng một học kỳ.
    /// </summary>
    Task<Result<AdminAnalyticsReportData>> GetDataAsync(Guid? semesterId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ExportAsync xuất mẫu báo cáo analytics và định dạng file mà SystemAdmin yêu cầu.
    /// </summary>
    Task<Result<AdminAnalyticsReportFileResponse>> ExportAsync(ExportAdminAnalyticsReportQuery request, CancellationToken cancellationToken);
}
