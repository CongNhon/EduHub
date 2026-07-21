using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Notifications;
using EduHub.Application.Interfaces.Repositories.Notifications;
using EduHub.Domain.Entities.Notifications;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Notifications;

/// <summary>
/// Ghi chú: NotificationRepository dùng EF Core để đọc notification của recipient user.
/// </summary>
public sealed class NotificationRepository(ApplicationDbContext dbContext) : INotificationRepository
{
    /// <summary>
    /// Ghi chú: ListForRecipientAsync đọc danh sách notification của user có phân trang và filter read/unread.
    /// </summary>
    public async Task<PagedResult<NotificationResponse>> ListForRecipientAsync(
        Guid recipientUserId,
        bool? isRead,
        PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.RecipientUserId == recipientUserId);

        if (isRead.HasValue)
        {
            query = isRead.Value
                ? query.Where(notification => notification.ReadAtUtc != null)
                : query.Where(notification => notification.ReadAtUtc == null);
        }

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(notification => notification.OccurredAtUtc)
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .Select(notification => new NotificationResponse(
                notification.Id,
                notification.Type,
                notification.Title,
                notification.Body,
                notification.StudentId,
                notification.AssignmentId,
                notification.OccurredAtUtc,
                notification.ReadAtUtc,
                notification.ReadAtUtc != null))
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationResponse>(items, pageRequest.Page, pageRequest.PageSize, total);
    }

    /// <summary>
    /// Ghi chú: GetForRecipientAsync lấy notification theo id và recipient để user không đọc notification người khác.
    /// </summary>
    public Task<Notification?> GetForRecipientAsync(Guid id, Guid recipientUserId, CancellationToken cancellationToken) =>
        dbContext.Notifications.SingleOrDefaultAsync(
            notification => notification.Id == id && notification.RecipientUserId == recipientUserId,
            cancellationToken);
}
