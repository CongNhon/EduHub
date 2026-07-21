using Carter;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Integrations;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Integrations;

/// <summary>
/// Ghi chú: IntegrationSyncModule đăng ký endpoint admin quản lý Ministry sync records.
/// </summary>
public sealed class IntegrationSyncModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký route HTTP để retry và đọc trạng thái Ministry sync.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/sync").WithTags("Integration Sync");

        group.MapPost("/grades/{assignmentId:guid}/retry", RetryGradeSyncAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("RetryGradeSync");

        group.MapGet("/records/{id:guid}", GetSyncRecordAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("GetExternalSyncRecord");
    }

    /// <summary>
    /// Ghi chú: RetryGradeSyncAsync nhận assignment id và reason để admin retry Ministry sync.
    /// </summary>
    private static async Task<IResult> RetryGradeSyncAsync(
        Guid assignmentId,
        RetryGradeSyncRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(assignmentId), cancellationToken)).ToHttpResult(IntegrationMappings.ToDto);

    /// <summary>
    /// Ghi chú: GetSyncRecordAsync đọc trạng thái một ExternalSyncRecord cho admin.
    /// </summary>
    private static async Task<IResult> GetSyncRecordAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(IntegrationMappings.ToStatusQuery(id), cancellationToken)).ToHttpResult(IntegrationMappings.ToDto);
}
