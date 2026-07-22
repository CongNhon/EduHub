using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardWeb;
using EduHub.Application.Interfaces.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.WebApi.Controllers.DevExpress;

/// <summary>
/// Ghi chú: EduHubDashboardController phục vụ dashboard thống kê chỉ đọc cho SystemAdmin và chặn chỉnh sửa cấu hình dashboard từ trình duyệt.
/// </summary>
[Authorize(Policy = AuthPolicies.SystemAdmin)]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class EduHubDashboardController(
    DashboardConfigurator configurator,
    IDataProtectionProvider dataProtectionProvider)
    : RestrictedDashboardController(configurator, dataProtectionProvider);
