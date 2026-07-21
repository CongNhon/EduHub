using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;

namespace EduHub.Application.Contracts.Notifications;

/// <summary>
/// Ghi chú: NotificationResponse là dữ liệu thông báo trả về cho user hiện tại.
/// </summary>
public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string Title,
    string Body,
    Guid? StudentId,
    Guid? AssignmentId,
    DateTime OccurredAtUtc,
    DateTime? ReadAtUtc,
    bool IsRead);

/// <summary>
/// Ghi chú: ListNotificationsQuery là query đọc danh sách notification của user hiện tại.
/// </summary>
public sealed record ListNotificationsQuery(
    bool? IsRead = null,
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize) : IQuery<Result<PagedResult<NotificationResponse>>>;

/// <summary>
/// Ghi chú: MarkNotificationReadCommand là command đánh dấu một notification của user hiện tại đã đọc.
/// </summary>
public sealed record MarkNotificationReadCommand(Guid Id) : ICommand<Result<NotificationResponse>>;

/// <summary>
/// Ghi chú: RealtimeNotificationPayload là payload tối thiểu được push qua SignalR cho recipient.
/// </summary>
public sealed record RealtimeNotificationPayload(
    Guid NotificationId,
    string Type,
    Guid? StudentId,
    DateTime OccurredAtUtc);
