using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer.Native.Services;
using EduHub.Application.Interfaces.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.WebApi.Controllers.DevExpress;

/// <summary>
/// Ghi chú: EduHubWebDocumentViewerController xử lý xem, in và xuất báo cáo DevExpress dành riêng cho SystemAdmin.
/// </summary>
[Authorize(Policy = AuthPolicies.SystemAdmin)]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class EduHubWebDocumentViewerController(
    IWebDocumentViewerMvcControllerService controllerService)
    : WebDocumentViewerController(controllerService);
