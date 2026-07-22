using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Monitoring;

namespace EduHub.Application.Interfaces.Services.Monitoring;

/// <summary>
/// Ghi chú: ISystemMonitoringService định nghĩa nghiệp vụ ghép cache và job metrics cho SystemAdmin.
/// </summary>
public interface ISystemMonitoringService
{
    /// <summary>
    /// Ghi chú: GetAsync trả snapshot vận hành mới nhất của hệ thống EduHub.
    /// </summary>
    Task<Result<SystemMonitoringResponse>> GetAsync(GetSystemMonitoringQuery request, CancellationToken cancellationToken);
}
