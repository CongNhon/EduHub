using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EduHub.WebApi.Hubs;

/// <summary>
/// Ghi chú: NotificationsHub là SignalR hub xác thực JWT để user nhận notification realtime.
/// </summary>
[Authorize]
public sealed class NotificationsHub : Hub
{
    /// <summary>
    /// Ghi chú: OnConnectedAsync thêm connection vào group user do server xác lập từ JWT.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Ghi chú: UserGroup tạo tên group SignalR nội bộ cho một user id.
    /// </summary>
    public static string UserGroup(string userId) => $"user:{userId}";
}
