using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Grades;
using EduHub.Domain.Entities.Academics;

namespace EduHub.Application.Interfaces.Repositories.Grades;

/// <summary>
/// Ghi chú: IGradeConfigurationRepository là interface truy cập dữ liệu cấu hình thành phần điểm.
/// </summary>
public interface IGradeConfigurationRepository
{
    /// <summary>
    /// Ghi chú: SubjectExistsAsync kiểm tra môn học tồn tại.
    /// </summary>
    Task<bool> SubjectExistsAsync(Guid subjectId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: SemesterExistsAsync kiểm tra học kỳ tồn tại.
    /// </summary>
    Task<bool> SemesterExistsAsync(Guid semesterId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetNextVersionAsync lấy version cấu hình tiếp theo cho subject-semester.
    /// </summary>
    Task<int> GetNextVersionAsync(Guid subjectId, Guid semesterId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: DeactivateActiveComponentsAsync tắt version cấu hình active cũ của subject-semester.
    /// </summary>
    Task DeactivateActiveComponentsAsync(Guid subjectId, Guid semesterId, DateTime updatedAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: AddRange thêm danh sách GradeComponent mới vào DbContext.
    /// </summary>
    void AddRange(IReadOnlyList<GradeComponent> components);

    /// <summary>
    /// Ghi chú: ListConfigurationsAsync đọc danh sách version cấu hình điểm đã phân trang.
    /// </summary>
    Task<PagedResult<GradeConfigurationResponse>> ListConfigurationsAsync(
        Guid? subjectId,
        Guid? semesterId,
        bool? isActive,
        PageRequest pageRequest,
        CancellationToken cancellationToken);
}
