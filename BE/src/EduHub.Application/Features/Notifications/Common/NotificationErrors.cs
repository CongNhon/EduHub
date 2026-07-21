using EduHub.Application.Common.Models;

namespace EduHub.Application.Features.Notifications.Common;

/// <summary>
/// Ghi chú: NotificationErrors chứa mã lỗi nghiệp vụ khi user đọc hoặc cập nhật notification.
/// </summary>
public static class NotificationErrors
{
    public static readonly Error Unauthorized = new(
        "Notification.Unauthorized",
        "Authentication is required to read notifications.",
        ErrorType.Unauthorized);

    public static readonly Error NotFound = new(
        "Notification.NotFound",
        "Notification was not found.",
        ErrorType.NotFound);
}
