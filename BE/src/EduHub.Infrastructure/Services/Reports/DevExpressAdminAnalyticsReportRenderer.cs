using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Services.Analytics;
using DevExpress.XtraPrinting;

namespace EduHub.Infrastructure.Services.Reports;

/// <summary>
/// Ghi chú: DevExpressAdminAnalyticsReportRenderer xuất mẫu XtraReport quản trị được chọn thành PDF, XLSX hoặc CSV.
/// </summary>
public sealed class DevExpressAdminAnalyticsReportRenderer : IAdminAnalyticsReportRenderer
{
    /// <summary>
    /// Ghi chú: RenderAsync chọn report factory, dựng document và xuất bytes theo format đã được Application Service kiểm tra.
    /// </summary>
    public Task<RenderedAdminAnalyticsReport> RenderAsync(
        AdminAnalyticsReportData data,
        string reportType,
        string format,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var report = DevExpressAdminReportCatalog.Create(reportType, data);
        using var stream = new MemoryStream();
        report.CreateDocument();
        cancellationToken.ThrowIfCancellationRequested();

        var metadata = format switch
        {
            "pdf" => ExportPdf(report, stream),
            "xlsx" => ExportXlsx(report, stream),
            "csv" => ExportCsv(report, stream),
            _ => throw new InvalidOperationException($"Unsupported report format '{format}'.")
        };

        return Task.FromResult(new RenderedAdminAnalyticsReport(stream.ToArray(), metadata.ContentType, metadata.Extension));
    }

    /// <summary>
    /// Ghi chú: ExportPdf xuất XtraReport thành PDF để xem hoặc in.
    /// </summary>
    private static ExportMetadata ExportPdf(DevExpress.XtraReports.UI.XtraReport report, Stream stream)
    {
        report.ExportToPdf(stream, new PdfExportOptions());
        return new ExportMetadata("application/pdf", "pdf");
    }

    /// <summary>
    /// Ghi chú: ExportXlsx xuất XtraReport thành workbook Excel để phân tích thêm.
    /// </summary>
    private static ExportMetadata ExportXlsx(DevExpress.XtraReports.UI.XtraReport report, Stream stream)
    {
        report.ExportToXlsx(stream, new XlsxExportOptions());
        return new ExportMetadata("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx");
    }

    /// <summary>
    /// Ghi chú: ExportCsv xuất XtraReport thành CSV cho công cụ dữ liệu bên ngoài.
    /// </summary>
    private static ExportMetadata ExportCsv(DevExpress.XtraReports.UI.XtraReport report, Stream stream)
    {
        report.ExportToCsv(stream, new CsvExportOptions());
        return new ExportMetadata("text/csv; charset=utf-8", "csv");
    }

    /// <summary>
    /// Ghi chú: ExportMetadata chứa MIME type và extension của file DevExpress vừa xuất.
    /// </summary>
    private sealed record ExportMetadata(string ContentType, string Extension);
}
