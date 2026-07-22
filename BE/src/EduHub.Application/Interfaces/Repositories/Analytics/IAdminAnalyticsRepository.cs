using EduHub.Application.Contracts.Analytics;

namespace EduHub.Application.Interfaces.Repositories.Analytics;

/// <summary>
/// Ghi chú: IAdminAnalyticsRepository định nghĩa các truy vấn PostgreSQL phục vụ dashboard SystemAdmin.
/// </summary>
public interface IAdminAnalyticsRepository
{
    /// <summary>
    /// Ghi chú: ResolveSemesterContextAsync chọn học kỳ yêu cầu hoặc học kỳ active/gần nhất và trả danh sách bộ lọc học kỳ.
    /// </summary>
    Task<AnalyticsSemesterContext?> ResolveSemesterContextAsync(Guid? semesterId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetOverviewAsync tổng hợp số học sinh, người dùng, lớp và hàng đợi nghiệp vụ của học kỳ đã chọn.
    /// </summary>
    Task<AdminOverviewResponse> GetOverviewAsync(AnalyticsSemesterContext context, DateTime generatedAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetAcademicAnalyticsAsync tổng hợp điểm chuẩn hóa theo môn, lớp, khoảng điểm và trạng thái điểm.
    /// </summary>
    Task<AdminAcademicAnalyticsResponse> GetAcademicAnalyticsAsync(AnalyticsSemesterResponse semester, DateTime generatedAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetDataQualityAsync đếm các bản ghi học vụ thiếu liên kết hoặc vi phạm tính đầy đủ dữ liệu.
    /// </summary>
    Task<AdminDataQualityResponse> GetDataQualityAsync(AnalyticsSemesterResponse semester, DateTime generatedAtUtc, CancellationToken cancellationToken);
}
