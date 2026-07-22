using Carter;
using EduHub.Application.Contracts.Monitoring;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Monitoring;

/// <summary>
/// Ghi chú: SystemMonitoringModule đăng ký API operational metrics dành riêng cho SystemAdmin.
/// </summary>
public sealed class SystemMonitoringModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký endpoint Redis, Hangfire và queue monitoring có policy SystemAdmin.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/admin/monitoring", GetAsync)
            .WithTags("System Monitoring")
            .WithName("GetSystemMonitoring")
            .RequireAuthorization(AuthPolicies.SystemAdmin);
    }

    /// <summary>
    /// Ghi chú: GetAsync gửi monitoring query qua MediatR và trả ApiResponse DTO.
    /// </summary>
    private static async Task<IResult> GetAsync(ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetSystemMonitoringQuery(), cancellationToken)).ToHttpResult(response => response.ToDto());
}
