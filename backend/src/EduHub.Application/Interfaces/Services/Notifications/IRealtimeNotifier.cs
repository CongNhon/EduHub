using EduHub.Application.Contracts.Notifications;

namespace EduHub.Application.Interfaces.Services.Notifications;

/// <summary>
/// Ghi chú: IRealtimeNotifier push notification tối thiểu tới user đang online qua kênh realtime.
/// </summary>
public interface IRealtimeNotifier
{
    /// <summary>
    /// Ghi chú: SendNotificationAsync gửi payload notification tới đúng recipient user.
    /// </summary>
    Task SendNotificationAsync(
        Guid recipientUserId,
        RealtimeNotificationPayload payload,
        CancellationToken cancellationToken);
}
