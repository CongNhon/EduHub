using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Notifications;
using EduHub.Domain.Entities.Notifications;

namespace EduHub.Application.Interfaces.Repositories.Notifications;

/// <summary>
/// Ghi chú: INotificationRepository truy cập notification theo recipient user.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Ghi chú: ListForRecipientAsync đọc notification của một user có phân trang.
    /// </summary>
    Task<PagedResult<NotificationResponse>> ListForRecipientAsync(
        Guid recipientUserId,
        bool? isRead,
        PageRequest pageRequest,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetForRecipientAsync lấy một notification thuộc user hiện tại.
    /// </summary>
    Task<Notification?> GetForRecipientAsync(Guid id, Guid recipientUserId, CancellationToken cancellationToken);
}
