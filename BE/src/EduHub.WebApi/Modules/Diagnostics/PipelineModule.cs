using Carter;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Common;
using EduHub.WebApi.Dtos.Diagnostics;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Diagnostics;

/// <summary>
/// Ghi chú: PipelineModule đăng ký các endpoint API cho kiểm tra MediatR pipeline.
/// </summary>
public sealed class PipelineModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký route HTTP cho endpoint kiểm tra MediatR pipeline.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/pipeline")
            .WithTags("Diagnostics")
            .RequireAuthorization(AuthPolicies.SystemAdmin);

        group.MapPost("/command", RunCommandAsync)
            .WithName("RunPipelineCommand")
            .WithSummary("Runs the command pipeline probe.")
            .Produces<ApiResponse<PipelineProbeDto>>()
            .ProducesValidationProblem();

        group.MapGet("/query", RunQueryAsync)
            .WithName("RunPipelineQuery")
            .WithSummary("Runs the query pipeline probe.")
            .Produces<ApiResponse<PipelineProbeDto>>();

        group.MapGet("/cache", GetCacheMetricsAsync)
            .WithName("GetCacheMetrics")
            .WithSummary("Returns Redis cache hit/miss/failure counters.")
            .Produces<ApiResponse<CacheMetricsDto>>();
    }

    /// <summary>
    /// Ghi chú: RunCommandAsync nhận PipelineProbeRequest DTO, map sang command và trả PipelineProbeDto.
    /// </summary>
    private static async Task<IResult> RunCommandAsync(
        PipelineProbeRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result.ToHttpResult(DiagnosticsMappings.ToDto);
    }

    /// <summary>
    /// Ghi chú: RunQueryAsync nhận query string name, gửi query và trả PipelineProbeDto.
    /// </summary>
    private static async Task<IResult> RunQueryAsync(
        [AsParameters] PipelineProbeQueryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(request.ToQuery(), cancellationToken);
        return Results.Ok(ApiResponse.Ok(response.ToDto()));
    }

    private static async Task<IResult> GetCacheMetricsAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(DiagnosticsMappings.ToCacheMetricsQuery(), cancellationToken);
        return Results.Ok(ApiResponse.Ok(response.ToDto()));
    }
}
