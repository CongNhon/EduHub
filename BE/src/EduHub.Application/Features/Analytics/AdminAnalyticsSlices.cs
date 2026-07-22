using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Services.Analytics;
using MediatR;

namespace EduHub.Application.Features.Analytics;

/// <summary>
/// Ghi chú: GetAdminOverviewQueryHandler chuyển yêu cầu KPI tổng quan sang AdminAnalyticsService.
/// </summary>
public sealed class GetAdminOverviewQueryHandler(IAdminAnalyticsService service)
    : IRequestHandler<GetAdminOverviewQuery, Result<AdminOverviewResponse>>
{
    /// <summary>
    /// Ghi chú: Handle đọc dashboard tổng quan của học kỳ qua service analytics.
    /// </summary>
    public Task<Result<AdminOverviewResponse>> Handle(GetAdminOverviewQuery request, CancellationToken cancellationToken) =>
        service.GetOverviewAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetAdminAcademicAnalyticsQueryHandler chuyển yêu cầu thống kê điểm sang AdminAnalyticsService.
/// </summary>
public sealed class GetAdminAcademicAnalyticsQueryHandler(IAdminAnalyticsService service)
    : IRequestHandler<GetAdminAcademicAnalyticsQuery, Result<AdminAcademicAnalyticsResponse>>
{
    /// <summary>
    /// Ghi chú: Handle đọc thống kê điểm học kỳ qua service analytics.
    /// </summary>
    public Task<Result<AdminAcademicAnalyticsResponse>> Handle(GetAdminAcademicAnalyticsQuery request, CancellationToken cancellationToken) =>
        service.GetAcademicAnalyticsAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetAdminDataQualityQueryHandler chuyển yêu cầu kiểm tra chất lượng dữ liệu sang AdminAnalyticsService.
/// </summary>
public sealed class GetAdminDataQualityQueryHandler(IAdminAnalyticsService service)
    : IRequestHandler<GetAdminDataQualityQuery, Result<AdminDataQualityResponse>>
{
    /// <summary>
    /// Ghi chú: Handle đọc các lỗi chất lượng dữ liệu học vụ qua service analytics.
    /// </summary>
    public Task<Result<AdminDataQualityResponse>> Handle(GetAdminDataQualityQuery request, CancellationToken cancellationToken) =>
        service.GetDataQualityAsync(request, cancellationToken);
}
