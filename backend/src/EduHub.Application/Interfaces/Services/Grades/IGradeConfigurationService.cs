using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Grades;

namespace EduHub.Application.Interfaces.Services.Grades;

/// <summary>
/// Ghi chú: IGradeConfigurationService là interface nghiệp vụ cấu hình thành phần điểm.
/// </summary>
public interface IGradeConfigurationService
{
    /// <summary>
    /// Ghi chú: CreateGradeConfigurationAsync tạo version cấu hình điểm mới cho subject-semester.
    /// </summary>
    Task<Result<GradeConfigurationResponse>> CreateGradeConfigurationAsync(
        CreateGradeConfigurationCommand request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListGradeConfigurationsAsync đọc danh sách version cấu hình điểm.
    /// </summary>
    Task<Result<PagedResult<GradeConfigurationResponse>>> ListGradeConfigurationsAsync(
        ListGradeConfigurationsQuery request,
        CancellationToken cancellationToken);
}

