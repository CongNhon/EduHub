using EduHub.Domain.Entities.Integration;

namespace EduHub.Application.Interfaces.Services.Integrations;

/// <summary>
/// Ghi chú: MinistrySyncResult là kết quả Ministry API trả về sau khi nhận sổ điểm.
/// </summary>
public sealed record MinistrySyncResult(string ExternalId, string ExternalVersion);

/// <summary>
/// Ghi chú: IEducationMinistryGateway định nghĩa cổng gọi Ministry API để sync sổ điểm đã publish/lock.
/// </summary>
public interface IEducationMinistryGateway
{
    /// <summary>
    /// Ghi chú: SyncGradebookAsync gửi payload của ExternalSyncRecord sổ điểm sang Ministry API.
    /// </summary>
    Task<MinistrySyncResult> SyncGradebookAsync(ExternalSyncRecord record, CancellationToken cancellationToken);
}
