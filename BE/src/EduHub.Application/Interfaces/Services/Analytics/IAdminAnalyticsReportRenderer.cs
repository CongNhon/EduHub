using EduHub.Application.Contracts.Analytics;

namespace EduHub.Application.Interfaces.Services.Analytics;

/// <summary>
/// Ghi chú: IAdminAnalyticsReportRenderer định nghĩa adapter dựng file báo cáo từ analytics data bằng reporting engine.
/// </summary>
public interface IAdminAnalyticsReportRenderer
{
    /// <summary>
    /// Ghi chú: RenderAsync chọn mẫu report rồi dựng file PDF, XLSX hoặc CSV từ dataset quản trị đã tổng hợp.
    /// </summary>
    Task<RenderedAdminAnalyticsReport> RenderAsync(
        AdminAnalyticsReportData data,
        string reportType,
        string format,
        CancellationToken cancellationToken);
}
