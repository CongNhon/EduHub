using DevExpress.XtraReports.Services;
using DevExpress.XtraReports.UI;
using EduHub.Application.Interfaces.Services.Analytics;

namespace EduHub.Infrastructure.Services.Reports;

/// <summary>
/// Ghi chú: DevExpressAdminReportProvider phân giải reportUrl của Web Document Viewer thành một trong ba XtraReport quản trị.
/// </summary>
public sealed class DevExpressAdminReportProvider(IAdminAnalyticsReportService reportService) : IReportProviderAsync
{
    /// <summary>
    /// Ghi chú: GetReportAsync đọc reportType và semesterId trong reportUrl, lấy dataset đã authorize và tạo đúng XtraReport.
    /// </summary>
    public async Task<XtraReport> GetReportAsync(string id, ReportProviderContext context)
    {
        var selection = DevExpressAdminReportCatalog.ParseViewerId(id);
        var data = await reportService.GetDataAsync(selection.SemesterId, CancellationToken.None);
        if (data.IsFailure)
        {
            throw new InvalidOperationException(data.Error?.Message ?? "Analytics report data is unavailable.");
        }

        return DevExpressAdminReportCatalog.Create(selection.ReportType, data.Value);
    }
}
