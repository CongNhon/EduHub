using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Academics;
using EduHub.Domain.Entities.Academics;

namespace EduHub.Application.Interfaces.Repositories.Academics;

/// <summary>
/// Ghi chú: IAcademicRepository là interface truy cập dữ liệu năm học, học kỳ và môn học.
/// </summary>
public interface IAcademicRepository
{
    /// <summary>
    /// Ghi chú: AcademicYearNameExistsAsync kiểm tra tên năm học đã tồn tại theo tên chuẩn hóa.
    /// </summary>
    Task<bool> AcademicYearNameExistsAsync(string normalizedName, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: AddAcademicYear thêm entity năm học mới vào database context.
    /// </summary>
    void AddAcademicYear(AcademicYear academicYear);

    /// <summary>
    /// Ghi chú: ListAcademicYearsAsync đọc danh sách năm học đã phân trang.
    /// </summary>
    Task<PagedResult<AcademicYearResponse>> ListAcademicYearsAsync(PageRequest pageRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetAcademicYearAsync lấy năm học theo id.
    /// </summary>
    Task<AcademicYear?> GetAcademicYearAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: SemesterNameExistsAsync kiểm tra tên học kỳ đã tồn tại trong năm học.
    /// </summary>
    Task<bool> SemesterNameExistsAsync(Guid academicYearId, string normalizedName, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: SemesterOverlapsAsync kiểm tra học kỳ mới có giao ngày với học kỳ hiện có trong cùng năm học.
    /// </summary>
    Task<bool> SemesterOverlapsAsync(Guid academicYearId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: AddSemester thêm entity học kỳ mới vào database context.
    /// </summary>
    void AddSemester(Semester semester);

    /// <summary>
    /// Ghi chú: ListSemestersAsync đọc danh sách học kỳ đã phân trang.
    /// </summary>
    Task<PagedResult<SemesterResponse>> ListSemestersAsync(Guid? academicYearId, PageRequest pageRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: SubjectCodeExistsAsync kiểm tra mã môn học đã tồn tại theo mã chuẩn hóa.
    /// </summary>
    Task<bool> SubjectCodeExistsAsync(string normalizedCode, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: AddSubject thêm entity môn học mới vào database context.
    /// </summary>
    void AddSubject(Subject subject);

    /// <summary>
    /// Ghi chú: GetSubjectAsync lấy môn học theo id để cập nhật hoặc vô hiệu hóa.
    /// </summary>
    Task<Subject?> GetSubjectAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListSubjectsAsync đọc danh sách môn học đã phân trang.
    /// </summary>
    Task<PagedResult<SubjectResponse>> ListSubjectsAsync(bool? isActive, PageRequest pageRequest, CancellationToken cancellationToken);
}
