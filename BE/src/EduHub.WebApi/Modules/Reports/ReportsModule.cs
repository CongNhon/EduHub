using Carter;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Common;
using EduHub.WebApi.Dtos.Reports;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Reports;

/// <summary>
/// Ghi chú: ReportsModule đăng ký endpoint API tạo, xem trạng thái và tải PDF report.
/// </summary>
public sealed class ReportsModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký route HTTP cho report jobs.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/reports").WithTags("Reports");

        group.MapPost("/report-cards", RequestReportCardAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("RequestReportCard");

        group.MapPost("/requests", CreateReportRequestAsync)
            .RequireAuthorization(AuthPolicies.Parent)
            .WithName("CreateReportRequest");

        group.MapGet("/requests", ListReportRequestsAsync)
            .WithName("ListReportRequests");

        group.MapPut("/requests/{id:guid}/review", ReviewReportRequestAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("ReviewReportRequest");

        group.MapGet("/jobs/{id:guid}", GetReportJobAsync)
            .WithName("GetReportJob");

        group.MapGet("/jobs/{id:guid}/download", DownloadReportAsync)
            .WithName("DownloadReport");
    }

    /// <summary>
    /// Ghi chú: RequestReportCardAsync nhận request tạo PDF và trả 202 Accepted với report job id.
    /// </summary>
    private static async Task<IResult> RequestReportCardAsync(
        RequestReportCardRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result.IsSuccess
            ? Results.Accepted($"/api/v1/reports/jobs/{result.Value.Id}", ApiResponse.Ok(result.Value.ToDto()))
            : result.ToHttpResult(ReportMappings.ToDto);
    }

    private static async Task<IResult> CreateReportRequestAsync(CreateReportRequestRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result.ToCreatedHttpResult(response => $"/api/v1/reports/requests/{response.Id}", ReportMappings.ToDto);
    }

    private static async Task<IResult> ListReportRequestsAsync([AsParameters] ListReportRequestsRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken)).ToHttpResult(result => result.ToPagedResponse(ReportMappings.ToDto));

    private static async Task<IResult> ReviewReportRequestAsync(Guid id, ReviewReportRequestRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(id), cancellationToken)).ToHttpResult(ReportMappings.ToDto);

    /// <summary>
    /// Ghi chú: GetReportJobAsync đọc trạng thái report job nếu user có quyền.
    /// </summary>
    private static async Task<IResult> GetReportJobAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(ReportMappings.ToStatusQuery(id), cancellationToken)).ToHttpResult(ReportMappings.ToDto);

    /// <summary>
    /// Ghi chú: DownloadReportAsync tải PDF nếu report completed, chưa expired và user có quyền.
    /// </summary>
    private static async Task<IResult> DownloadReportAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(ReportMappings.ToDownloadQuery(id), cancellationToken);
        return result.IsSuccess
            ? Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName)
            : result.ToHttpResult(value => value);
    }
}
