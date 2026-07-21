using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Notifications;
using EduHub.Application.Interfaces.Services.Notifications;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Notifications.Notifications;

/// <summary>
/// Ghi chú: ListNotificationsQueryValidator kiểm tra filter/phân trang khi user đọc notification.
/// </summary>
public sealed class ListNotificationsQueryValidator : AbstractValidator<ListNotificationsQuery>
{
    /// <summary>
    /// Ghi chú: Constructor khai báo rule page/pageSize cho danh sách notification.
    /// </summary>
    public ListNotificationsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PageRequest.MaxPageSize);
    }
}

/// <summary>
/// Ghi chú: ListNotificationsQueryHandler chuyển query đọc notification sang NotificationService.
/// </summary>
public sealed class ListNotificationsQueryHandler(INotificationService notificationService)
    : IRequestHandler<ListNotificationsQuery, Result<PagedResult<NotificationResponse>>>
{
    /// <summary>
    /// Ghi chú: Handle đọc notification của user hiện tại.
    /// </summary>
    public Task<Result<PagedResult<NotificationResponse>>> Handle(
        ListNotificationsQuery request,
        CancellationToken cancellationToken) =>
        notificationService.ListNotificationsAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: MarkNotificationReadCommandValidator kiểm tra notification id trước khi đánh dấu đã đọc.
/// </summary>
public sealed class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khai báo rule bắt buộc cho notification id.
    /// </summary>
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}

/// <summary>
/// Ghi chú: MarkNotificationReadCommandHandler chuyển command đánh dấu đã đọc sang NotificationService.
/// </summary>
public sealed class MarkNotificationReadCommandHandler(INotificationService notificationService)
    : IRequestHandler<MarkNotificationReadCommand, Result<NotificationResponse>>
{
    /// <summary>
    /// Ghi chú: Handle đánh dấu notification của user hiện tại đã đọc.
    /// </summary>
    public Task<Result<NotificationResponse>> Handle(
        MarkNotificationReadCommand request,
        CancellationToken cancellationToken) =>
        notificationService.MarkReadAsync(request, cancellationToken);
}
