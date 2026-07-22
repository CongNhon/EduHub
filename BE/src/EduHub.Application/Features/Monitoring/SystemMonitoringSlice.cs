using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Monitoring;
using EduHub.Application.Interfaces.Services.Monitoring;
using MediatR;

namespace EduHub.Application.Features.Monitoring;

/// <summary>
/// Ghi chú: GetSystemMonitoringQueryHandler chuyển yêu cầu giám sát hệ thống sang SystemMonitoringService.
/// </summary>
public sealed class GetSystemMonitoringQueryHandler(ISystemMonitoringService service)
    : IRequestHandler<GetSystemMonitoringQuery, Result<SystemMonitoringResponse>>
{
    /// <summary>
    /// Ghi chú: Handle đọc snapshot Redis, Hangfire và operational queues qua service monitoring.
    /// </summary>
    public Task<Result<SystemMonitoringResponse>> Handle(GetSystemMonitoringQuery request, CancellationToken cancellationToken) =>
        service.GetAsync(request, cancellationToken);
}
