using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Academics;

namespace EduHub.Application.Interfaces.Services.Academics;

/// <summary>
/// Ghi chú: IAcademicService là interface cho nghiệp vụ năm học, học kỳ và môn học.
/// </summary>
public interface IAcademicService
{
    /// <summary>
    /// Ghi chú: CreateAcademicYearAsync xử lý tạo năm học mới.
    /// </summary>
    Task<Result<AcademicYearResponse>> CreateAcademicYearAsync(CreateAcademicYearCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListAcademicYearsAsync xử lý đọc danh sách năm học.
    /// </summary>
    Task<Result<PagedResult<AcademicYearResponse>>> ListAcademicYearsAsync(ListAcademicYearsQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: CreateSemesterAsync xử lý tạo học kỳ trong năm học.
    /// </summary>
    Task<Result<SemesterResponse>> CreateSemesterAsync(CreateSemesterCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListSemestersAsync xử lý đọc danh sách học kỳ.
    /// </summary>
    Task<Result<PagedResult<SemesterResponse>>> ListSemestersAsync(ListSemestersQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: CreateSubjectAsync xử lý tạo môn học mới.
    /// </summary>
    Task<Result<SubjectResponse>> CreateSubjectAsync(CreateSubjectCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: UpdateSubjectAsync xử lý cập nhật môn học hiện có.
    /// </summary>
    Task<Result<SubjectResponse>> UpdateSubjectAsync(UpdateSubjectCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: DisableSubjectAsync xử lý vô hiệu hóa môn học hiện có.
    /// </summary>
    Task<Result<SubjectResponse>> DisableSubjectAsync(DisableSubjectCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListSubjectsAsync xử lý đọc danh sách môn học.
    /// </summary>
    Task<Result<PagedResult<SubjectResponse>>> ListSubjectsAsync(ListSubjectsQuery request, CancellationToken cancellationToken);
}
