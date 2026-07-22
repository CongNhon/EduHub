using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Repositories.Analytics;
using EduHub.Application.Interfaces.Services.Analytics;
using EduHub.Domain.Enums;

namespace EduHub.Application.Services.Analytics;

/// <summary>
/// Ghi chú: AdminAnalyticsService kiểm tra quyền SystemAdmin, chọn học kỳ và điều phối truy vấn dashboard.
/// </summary>
public sealed class AdminAnalyticsService(
    IAdminAnalyticsRepository repository,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IAdminAnalyticsService
{
    private static readonly Error ForbiddenError = new(
        "Analytics.SystemAdminRequired",
        "System administrator role is required.",
        ErrorType.Forbidden);

    private static readonly Error SemesterNotFoundError = new(
        "Analytics.SemesterNotFound",
        "The requested semester was not found and no fallback semester is available.",
        ErrorType.NotFound);

    /// <summary>
    /// Ghi chú: GetOverviewAsync trả KPI người dùng, học sinh, lớp và hàng đợi của học kỳ đã chọn.
    /// </summary>
    public async Task<Result<AdminOverviewResponse>> GetOverviewAsync(GetAdminOverviewQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.SystemAdmin)
        {
            return Result.Failure<AdminOverviewResponse>(ForbiddenError);
        }

        var context = await repository.ResolveSemesterContextAsync(request.SemesterId, cancellationToken);
        return context is null
            ? Result.Failure<AdminOverviewResponse>(SemesterNotFoundError)
            : Result.Success(await repository.GetOverviewAsync(context, UtcNow(), cancellationToken));
    }

    /// <summary>
    /// Ghi chú: GetAcademicAnalyticsAsync trả dataset điểm chuẩn hóa của học kỳ đã chọn.
    /// </summary>
    public async Task<Result<AdminAcademicAnalyticsResponse>> GetAcademicAnalyticsAsync(GetAdminAcademicAnalyticsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.SystemAdmin)
        {
            return Result.Failure<AdminAcademicAnalyticsResponse>(ForbiddenError);
        }

        var context = await repository.ResolveSemesterContextAsync(request.SemesterId, cancellationToken);
        return context is null
            ? Result.Failure<AdminAcademicAnalyticsResponse>(SemesterNotFoundError)
            : Result.Success(await repository.GetAcademicAnalyticsAsync(context.SelectedSemester, UtcNow(), cancellationToken));
    }

    /// <summary>
    /// Ghi chú: GetDataQualityAsync trả các lỗi liên kết dữ liệu học vụ của học kỳ đã chọn.
    /// </summary>
    public async Task<Result<AdminDataQualityResponse>> GetDataQualityAsync(GetAdminDataQualityQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.SystemAdmin)
        {
            return Result.Failure<AdminDataQualityResponse>(ForbiddenError);
        }

        var context = await repository.ResolveSemesterContextAsync(request.SemesterId, cancellationToken);
        return context is null
            ? Result.Failure<AdminDataQualityResponse>(SemesterNotFoundError)
            : Result.Success(await repository.GetDataQualityAsync(context.SelectedSemester, UtcNow(), cancellationToken));
    }

    /// <summary>
    /// Ghi chú: UtcNow lấy thời điểm UTC thống nhất để gắn mốc tạo cho mỗi dataset analytics.
    /// </summary>
    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
