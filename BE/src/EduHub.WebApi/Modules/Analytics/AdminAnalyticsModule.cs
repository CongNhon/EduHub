using Carter;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Analytics;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Analytics;

/// <summary>
/// Ghi chú: AdminAnalyticsModule đăng ký API dataset tổng quan, học lực và chất lượng dữ liệu dành riêng cho SystemAdmin.
/// </summary>
public sealed class AdminAnalyticsModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes bảo vệ toàn bộ endpoint analytics bằng policy SystemAdmin.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/analytics")
            .WithTags("Admin Analytics")
            .RequireAuthorization(AuthPolicies.SystemAdmin);

        group.MapGet("/overview", GetOverviewAsync).WithName("GetAdminAnalyticsOverview");
        group.MapGet("/academic", GetAcademicAsync).WithName("GetAdminAcademicAnalytics");
        group.MapGet("/data-quality", GetDataQualityAsync).WithName("GetAdminDataQualityAnalytics");
        group.MapGet("/report/export", ExportReportAsync).WithName("ExportAdminAnalyticsReport");
    }

    /// <summary>
    /// Ghi chú: GetOverviewAsync gửi query KPI tổng quan qua MediatR và trả ApiResponse DTO.
    /// </summary>
    private static async Task<IResult> GetOverviewAsync(
        [AsParameters] GetAdminAnalyticsRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToOverviewQuery(), cancellationToken)).ToHttpResult(response => response.ToDto());

    /// <summary>
    /// Ghi chú: GetAcademicAsync gửi query thống kê điểm qua MediatR và trả ApiResponse DTO.
    /// </summary>
    private static async Task<IResult> GetAcademicAsync(
        [AsParameters] GetAdminAnalyticsRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToAcademicQuery(), cancellationToken)).ToHttpResult(response => response.ToDto());

    /// <summary>
    /// Ghi chú: GetDataQualityAsync gửi query chất lượng dữ liệu qua MediatR và trả ApiResponse DTO.
    /// </summary>
    private static async Task<IResult> GetDataQualityAsync(
        [AsParameters] GetAdminAnalyticsRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToDataQualityQuery(), cancellationToken)).ToHttpResult(response => response.ToDto());

    /// <summary>
    /// Ghi chú: ExportReportAsync xuất mẫu DevExpress report được chọn thành PDF, XLSX hoặc CSV.
    /// </summary>
    private static async Task<IResult> ExportReportAsync(
        [AsParameters] ExportAdminAnalyticsReportRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToExportQuery(), cancellationToken))
        .ToFileHttpResult(file => (file.Content, file.ContentType, file.FileName));
}
