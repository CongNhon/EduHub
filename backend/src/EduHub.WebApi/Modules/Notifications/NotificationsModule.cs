using Carter;
using EduHub.WebApi.Dtos.Notifications;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Notifications;

/// <summary>
/// Ghi chú: NotificationsModule đăng ký endpoint API đọc và đánh dấu notification của user hiện tại.
/// </summary>
public sealed class NotificationsModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký route HTTP cho notification persistence.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications").WithTags("Notifications");

        group.MapGet("/", ListAsync)
            .WithName("ListNotifications");

        group.MapPut("/{id:guid}/read", MarkReadAsync)
            .WithName("MarkNotificationRead");
    }

    /// <summary>
    /// Ghi chú: ListAsync đọc notification của user hiện tại từ query string.
    /// </summary>
    private static async Task<IResult> ListAsync(
        [AsParameters] ListNotificationsRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken))
        .ToHttpResult(result => result.ToPagedResponse(NotificationMappings.ToDto));

    /// <summary>
    /// Ghi chú: MarkReadAsync đánh dấu một notification của user hiện tại đã đọc.
    /// </summary>
    private static async Task<IResult> MarkReadAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new MarkNotificationReadRequest(id).ToCommand(), cancellationToken))
        .ToHttpResult(NotificationMappings.ToDto);
}
