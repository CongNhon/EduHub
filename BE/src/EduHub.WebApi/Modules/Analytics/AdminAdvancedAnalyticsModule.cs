using Carter;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Services.Analytics;
using EduHub.WebApi.Dtos.Analytics;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.WebApi.Modules.Analytics;

/// <summary>
/// Ghi chú: AdminAdvancedAnalyticsModule đăng ký các API phân tích nâng cao dành cho SystemAdmin.
/// </summary>
public sealed class AdminAdvancedAnalyticsModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes bảo vệ toàn bộ endpoint phân tích nâng cao bằng policy SystemAdmin.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/analytics/advanced")
            .WithTags("Admin Advanced Analytics")
            .RequireAuthorization(AuthPolicies.SystemAdmin);

        group.MapGet("/summary", GetSummaryAsync).WithName("GetAdvancedSummary");
        group.MapGet("/distribution", GetDistributionAsync).WithName("GetAcademicDistribution");
        group.MapGet("/trends", GetTrendsAsync).WithName("GetAcademicTrends");
        group.MapGet("/student-risk", GetStudentRiskAsync).WithName("GetStudentRisk");
    }

    /// <summary>
    /// Ghi chú: GetSummaryAsync lấy dữ liệu tóm tắt phân tích nâng cao.
    /// </summary>
    private static async Task<IResult> GetSummaryAsync(
        [AsParameters] AdminAdvancedAnalyticsRequest request,
        IAdminAdvancedAnalyticsService service,
        CancellationToken cancellationToken) =>
        (await service.ReadSummaryAsync(request.ToFilter(), cancellationToken))
        .ToHttpResult(response => response.ToDto());

    /// <summary>
    /// Ghi chú: GetDistributionAsync lấy dữ liệu phân bổ điểm số.
    /// </summary>
    private static async Task<IResult> GetDistributionAsync(
        [AsParameters] AdminAdvancedAnalyticsRequest request,
        IAdminAdvancedAnalyticsService service,
        CancellationToken cancellationToken,
        [FromQuery] string groupBy = "class") =>
        (await service.ReadDistributionAsync(request.ToFilter(), groupBy, cancellationToken))
        .ToHttpResult(response => response.ToDto());

    /// <summary>
    /// Ghi chú: GetTrendsAsync lấy dữ liệu xu hướng học tập qua nhiều học kỳ.
    /// </summary>
    private static async Task<IResult> GetTrendsAsync(
        [AsParameters] AdminAdvancedAnalyticsRequest request,
        IAdminAdvancedAnalyticsService service,
        CancellationToken cancellationToken,
        [FromQuery] int maxSemesters = 4) =>
        (await service.ReadTrendsAsync(request.ToFilter(), maxSemesters, cancellationToken))
        .ToHttpResult(response => response.ToDto());

    /// <summary>
    /// Ghi chú: GetStudentRiskAsync lấy danh sách rủi ro học sinh.
    /// </summary>
    private static async Task<IResult> GetStudentRiskAsync(
        [AsParameters] AdminAdvancedAnalyticsRequest request,
        IAdminAdvancedAnalyticsService service,
        CancellationToken cancellationToken) =>
        (await service.ReadStudentRiskAsync(request.ToFilter(), cancellationToken))
        .ToHttpResult(response => response.ToDto());
}
