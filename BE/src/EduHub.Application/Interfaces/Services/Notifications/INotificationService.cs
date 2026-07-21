using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Notifications;

namespace EduHub.Application.Interfaces.Services.Notifications;

/// <summary>
/// Ghi chú: INotificationService xử lý nghiệp vụ đọc và đánh dấu notification của user hiện tại.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Ghi chú: ListNotificationsAsync đọc notification của user hiện tại.
    /// </summary>
    Task<Result<PagedResult<NotificationResponse>>> ListNotificationsAsync(
        ListNotificationsQuery request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: MarkReadAsync đánh dấu một notification của user hiện tại đã đọc.
    /// </summary>
    Task<Result<NotificationResponse>> MarkReadAsync(MarkNotificationReadCommand request, CancellationToken cancellationToken);
}
