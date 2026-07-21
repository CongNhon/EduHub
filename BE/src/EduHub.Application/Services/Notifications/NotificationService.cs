using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Notifications;
using EduHub.Application.Features.Notifications.Common;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.Notifications;
using EduHub.Application.Interfaces.Services.Notifications;
using EduHub.Domain.Entities.Notifications;

namespace EduHub.Application.Services.Notifications;

/// <summary>
/// Ghi chú: NotificationService xử lý đọc notification và đánh dấu đã đọc cho user hiện tại.
/// </summary>
public sealed class NotificationService(
    INotificationRepository notificationRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : INotificationService
{
    /// <summary>
    /// Ghi chú: ListNotificationsAsync đọc danh sách notification của user đang đăng nhập.
    /// </summary>
    public async Task<Result<PagedResult<NotificationResponse>>> ListNotificationsAsync(
        ListNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<PagedResult<NotificationResponse>>(NotificationErrors.Unauthorized);
        }

        var pageRequest = PageRequest.Create(request.Page, request.PageSize);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<PagedResult<NotificationResponse>>(pageRequest.Error!);
        }

        return Result.Success(await notificationRepository.ListForRecipientAsync(
            currentUser.UserId.Value,
            request.IsRead,
            pageRequest.Value,
            cancellationToken));
    }

    /// <summary>
    /// Ghi chú: MarkReadAsync đánh dấu notification thuộc user hiện tại là đã đọc.
    /// </summary>
    public async Task<Result<NotificationResponse>> MarkReadAsync(
        MarkNotificationReadCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<NotificationResponse>(NotificationErrors.Unauthorized);
        }

        var notification = await notificationRepository.GetForRecipientAsync(
            request.Id,
            currentUser.UserId.Value,
            cancellationToken);
        if (notification is null)
        {
            return Result.Failure<NotificationResponse>(NotificationErrors.NotFound);
        }

        notification.MarkRead(timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToResponse(notification));
    }

    /// <summary>
    /// Ghi chú: ToResponse chuyển Notification entity thành NotificationResponse trả về API.
    /// </summary>
    private static NotificationResponse ToResponse(Notification notification) =>
        new(
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Body,
            notification.StudentId,
            notification.AssignmentId,
            notification.OccurredAtUtc,
            notification.ReadAtUtc,
            notification.IsRead);
}
