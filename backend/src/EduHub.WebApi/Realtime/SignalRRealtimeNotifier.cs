using EduHub.Application.Contracts.Notifications;
using EduHub.Application.Interfaces.Services.Notifications;
using EduHub.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EduHub.WebApi.Realtime;

/// <summary>
/// Ghi chú: SignalRRealtimeNotifier gửi notification payload tới đúng user group trên NotificationsHub.
/// </summary>
public sealed class SignalRRealtimeNotifier(IHubContext<NotificationsHub> hubContext) : IRealtimeNotifier
{
    /// <summary>
    /// Ghi chú: SendNotificationAsync push NotificationReceived tới recipient user đang online.
    /// </summary>
    public Task SendNotificationAsync(
        Guid recipientUserId,
        RealtimeNotificationPayload payload,
        CancellationToken cancellationToken) =>
        hubContext.Clients
            .Group(NotificationsHub.UserGroup(recipientUserId.ToString()))
            .SendAsync("NotificationReceived", payload, cancellationToken);
}
