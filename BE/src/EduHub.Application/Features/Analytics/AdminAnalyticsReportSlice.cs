using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Services.Analytics;
using MediatR;

namespace EduHub.Application.Features.Analytics;

/// <summary>
/// Ghi chú: ExportAdminAnalyticsReportQueryHandler chuyển yêu cầu export analytics sang report service.
/// </summary>
public sealed class ExportAdminAnalyticsReportQueryHandler(IAdminAnalyticsReportService service)
    : IRequestHandler<ExportAdminAnalyticsReportQuery, Result<AdminAnalyticsReportFileResponse>>
{
    /// <summary>
    /// Ghi chú: Handle xuất file PDF, XLSX hoặc CSV qua AdminAnalyticsReportService.
    /// </summary>
    public Task<Result<AdminAnalyticsReportFileResponse>> Handle(
        ExportAdminAnalyticsReportQuery request,
        CancellationToken cancellationToken) =>
        service.ExportAsync(request, cancellationToken);
}
