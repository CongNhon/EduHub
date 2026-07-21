using EduHub.Application.Interfaces.Authentication;
using Hangfire.Dashboard;

namespace EduHub.WebApi.Security;

/// <summary>
/// Ghi chú: HangfireDashboardAuthorizationFilter chỉ cho SystemAdmin đã đăng nhập mở dashboard Hangfire.
/// </summary>
public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    /// Ghi chú: Authorize kiểm tra user hiện tại có role SystemAdmin không.
    /// </summary>
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true &&
            httpContext.User.IsInRole(AuthPolicies.SystemAdmin);
    }
}
