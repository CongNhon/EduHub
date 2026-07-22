using EduHub.Application.Contracts.Monitoring;

namespace EduHub.Application.Interfaces.Repositories.Monitoring;

/// <summary>
/// Ghi chú: ISystemMonitoringRepository định nghĩa truy vấn hàng đợi PostgreSQL và Hangfire cho SystemAdmin.
/// </summary>
public interface ISystemMonitoringRepository
{
    /// <summary>
    /// Ghi chú: GetSnapshotAsync đọc backlog Outbox, job, integration, email và notification tại thời điểm yêu cầu.
    /// </summary>
    Task<OperationalMonitoringSnapshot> GetSnapshotAsync(DateTime nowUtc, CancellationToken cancellationToken);
}
