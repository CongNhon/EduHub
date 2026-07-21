using EduHub.Application.Interfaces.Repositories.Integrations;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Integrations;

/// <summary>
/// Ghi chú: ExternalSyncRepository dùng EF Core để truy cập external sync records.
/// </summary>
public sealed class ExternalSyncRepository(ApplicationDbContext dbContext) : IExternalSyncRepository
{
    /// <summary>
    /// Ghi chú: GetAsync đọc một ExternalSyncRecord theo id để admin xem trạng thái hoặc job xử lý.
    /// </summary>
    public Task<ExternalSyncRecord?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.ExternalSyncRecords.SingleOrDefaultAsync(record => record.Id == id, cancellationToken);

    /// <summary>
    /// Ghi chú: GetLatestForAssignmentAsync đọc sync record mới nhất của assignment để manual retry.
    /// </summary>
    public Task<ExternalSyncRecord?> GetLatestForAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken) =>
        dbContext.ExternalSyncRecords
            .Where(record => record.AggregateType == "Gradebook" && record.AggregateId == assignmentId)
            .OrderByDescending(record => record.Version)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Ghi chú: GetPendingAsync đọc batch sync record đang chờ hoặc đã đến lịch retry.
    /// </summary>
    public Task<List<ExternalSyncRecord>> GetPendingAsync(int batchSize, DateTime utcNow, CancellationToken cancellationToken) =>
        dbContext.ExternalSyncRecords
            .Where(record =>
                record.Status == ExternalSyncStatus.Pending ||
                record.Status == ExternalSyncStatus.RetryScheduled && record.NextRetryAtUtc <= utcNow)
            .OrderBy(record => record.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Ghi chú: Add thêm ExternalSyncRecord mới sau khi sổ điểm đã publish/lock.
    /// </summary>
    public void Add(ExternalSyncRecord record) => dbContext.ExternalSyncRecords.Add(record);
}
