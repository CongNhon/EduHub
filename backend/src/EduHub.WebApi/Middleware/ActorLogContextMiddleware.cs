using System.Security.Claims;
using Serilog.Context;

namespace EduHub.WebApi.Middleware;

/// <summary>
/// Ghi chú: ActorLogContextMiddleware đẩy user id/role đã xác thực vào Serilog LogContext.
/// </summary>
public sealed class ActorLogContextMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Ghi chú: InvokeAsync thêm ActorUserId và ActorRole cho log trong request hiện tại.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var role = context.User.FindFirstValue(ClaimTypes.Role) ?? "anonymous";

        using (LogContext.PushProperty("ActorUserId", userId))
        using (LogContext.PushProperty("ActorRole", role))
        {
            await next(context);
        }
    }
}
