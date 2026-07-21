namespace EduHub.WebApi.Dtos.Notifications;

/// <summary>
/// Ghi chú: ListNotificationsRequest là DTO query string để đọc notification của user hiện tại.
/// </summary>
public sealed record ListNotificationsRequest(bool? IsRead, int? Page, int? PageSize);

/// <summary>
/// Ghi chú: MarkNotificationReadRequest là DTO route để đánh dấu notification đã đọc.
/// </summary>
public sealed record MarkNotificationReadRequest(Guid Id);

/// <summary>
/// Ghi chú: NotificationDto là DTO response API cho một notification của user hiện tại.
/// </summary>
public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Body,
    Guid? StudentId,
    Guid? AssignmentId,
    DateTime OccurredAtUtc,
    DateTime? ReadAtUtc,
    bool IsRead);
