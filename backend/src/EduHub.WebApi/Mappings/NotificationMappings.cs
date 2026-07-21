using EduHub.Application.Contracts.Notifications;
using EduHub.WebApi.Dtos.Notifications;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: NotificationMappings map giữa Notification DTO API và contract Application.
/// </summary>
public static class NotificationMappings
{
    /// <summary>
    /// Ghi chú: ToQuery chuyển ListNotificationsRequest API thành ListNotificationsQuery application.
    /// </summary>
    public static ListNotificationsQuery ToQuery(this ListNotificationsRequest request) =>
        new(request.IsRead, request.Page ?? 1, request.PageSize ?? 20);

    /// <summary>
    /// Ghi chú: ToCommand chuyển MarkNotificationReadRequest API thành MarkNotificationReadCommand application.
    /// </summary>
    public static MarkNotificationReadCommand ToCommand(this MarkNotificationReadRequest request) => new(request.Id);

    /// <summary>
    /// Ghi chú: ToDto chuyển NotificationResponse application thành NotificationDto API.
    /// </summary>
    public static NotificationDto ToDto(this NotificationResponse response) =>
        new(
            response.Id,
            response.Type,
            response.Title,
            response.Body,
            response.StudentId,
            response.AssignmentId,
            response.OccurredAtUtc,
            response.ReadAtUtc,
            response.IsRead);
}
