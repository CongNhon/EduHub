using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Integrations;

namespace EduHub.Application.Interfaces.Services.Integrations;

/// <summary>
/// Ghi chú: IExternalSyncService xử lý admin xem/retry external sync records.
/// </summary>
public interface IExternalSyncService
{
    /// <summary>
    /// Ghi chú: RetryGradeSyncAsync retry sync Ministry cho assignment đã publish bằng idempotency key cũ.
    /// </summary>
    Task<Result<ExternalSyncRecordResponse>> RetryGradeSyncAsync(
        RetryGradeSyncCommand request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetSyncRecordAsync đọc trạng thái sync record theo id.
    /// </summary>
    Task<Result<ExternalSyncRecordResponse>> GetSyncRecordAsync(
        GetExternalSyncRecordQuery request,
        CancellationToken cancellationToken);
}
