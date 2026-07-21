using EduHub.Domain.Entities.Integration;

namespace EduHub.Application.Interfaces.Repositories.Integrations;

/// <summary>
/// Ghi chú: IExternalSyncRepository truy cập external sync records cho Ministry API.
/// </summary>
public interface IExternalSyncRepository
{
    /// <summary>
    /// Ghi chú: GetAsync lấy sync record theo id.
    /// </summary>
    Task<ExternalSyncRecord?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetLatestForAssignmentAsync lấy sync record mới nhất của assignment.
    /// </summary>
    Task<ExternalSyncRecord?> GetLatestForAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetPendingAsync lấy sync record cần xử lý.
    /// </summary>
    Task<List<ExternalSyncRecord>> GetPendingAsync(int batchSize, DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: Add thêm sync record mới vào DbContext.
    /// </summary>
    void Add(ExternalSyncRecord record);
}
