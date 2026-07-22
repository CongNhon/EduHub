using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Analytics;

namespace EduHub.Application.Interfaces.Services.Analytics;

/// <summary>
/// Ghi chú: IAdminAnalyticsService định nghĩa nghiệp vụ đọc dashboard tổng quan, học lực và chất lượng dữ liệu cho SystemAdmin.
/// </summary>
public interface IAdminAnalyticsService
{
    /// <summary>
    /// Ghi chú: GetOverviewAsync trả KPI tổng quan của trường theo học kỳ mà SystemAdmin chọn.
    /// </summary>
    Task<Result<AdminOverviewResponse>> GetOverviewAsync(GetAdminOverviewQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetAcademicAnalyticsAsync trả thống kê điểm học kỳ đã chuẩn hóa về thang 10.
    /// </summary>
    Task<Result<AdminAcademicAnalyticsResponse>> GetAcademicAnalyticsAsync(GetAdminAcademicAnalyticsQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetDataQualityAsync trả danh sách lỗi dữ liệu cần SystemAdmin giám sát.
    /// </summary>
    Task<Result<AdminDataQualityResponse>> GetDataQualityAsync(GetAdminDataQualityQuery request, CancellationToken cancellationToken);
}
