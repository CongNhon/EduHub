using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Academics;
using EduHub.Application.Features.Academics.Common;
using EduHub.Application.Interfaces.Repositories.Academics;
using EduHub.Domain.Entities.Academics;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Academics;

/// <summary>
/// Ghi chú: AcademicRepository dùng EF Core để truy cập dữ liệu năm học, học kỳ và môn học.
/// </summary>
public sealed class AcademicRepository(ApplicationDbContext dbContext) : IAcademicRepository
{
    /// <summary>
    /// Ghi chú: AcademicYearNameExistsAsync kiểm tra tên năm học đã tồn tại.
    /// </summary>
    public Task<bool> AcademicYearNameExistsAsync(string normalizedName, CancellationToken cancellationToken) =>
        dbContext.AcademicYears.AnyAsync(year => year.NormalizedName == normalizedName, cancellationToken);

    /// <summary>
    /// Ghi chú: AddAcademicYear thêm năm học mới vào DbContext.
    /// </summary>
    public void AddAcademicYear(AcademicYear academicYear) => dbContext.AcademicYears.Add(academicYear);

    /// <summary>
    /// Ghi chú: ListAcademicYearsAsync đọc danh sách năm học đã phân trang.
    /// </summary>
    public async Task<PagedResult<AcademicYearResponse>> ListAcademicYearsAsync(
        PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AcademicYears.AsNoTracking();
        if (pageRequest.Search is not null)
        {
            var search = AcademicNormalization.Name(pageRequest.Search);
            query = query.Where(year => year.NormalizedName.Contains(search));
        }

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(year => year.StartDate)
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .Select(year => new AcademicYearResponse(
                year.Id,
                year.Name,
                year.StartDate,
                year.EndDate,
                year.Status.ToString()))
            .ToListAsync(cancellationToken);

        return new PagedResult<AcademicYearResponse>(items, pageRequest.Page, pageRequest.PageSize, total);
    }

    /// <summary>
    /// Ghi chú: GetAcademicYearAsync lấy năm học theo id.
    /// </summary>
    public Task<AcademicYear?> GetAcademicYearAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.AcademicYears.AsNoTracking().SingleOrDefaultAsync(year => year.Id == id, cancellationToken);

    /// <summary>
    /// Ghi chú: SemesterNameExistsAsync kiểm tra tên học kỳ trùng trong năm học.
    /// </summary>
    public Task<bool> SemesterNameExistsAsync(
        Guid academicYearId,
        string normalizedName,
        CancellationToken cancellationToken) =>
        dbContext.Semesters.AnyAsync(
            semester => semester.AcademicYearId == academicYearId && semester.NormalizedName == normalizedName,
            cancellationToken);

    /// <summary>
    /// Ghi chú: SemesterOverlapsAsync kiểm tra học kỳ giao ngày trong cùng năm học.
    /// </summary>
    public Task<bool> SemesterOverlapsAsync(
        Guid academicYearId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken) =>
        dbContext.Semesters.AnyAsync(
            semester =>
                semester.AcademicYearId == academicYearId &&
                startDate <= semester.EndDate &&
                endDate >= semester.StartDate,
            cancellationToken);

    /// <summary>
    /// Ghi chú: AddSemester thêm học kỳ mới vào DbContext.
    /// </summary>
    public void AddSemester(Semester semester) => dbContext.Semesters.Add(semester);

    /// <summary>
    /// Ghi chú: ListSemestersAsync đọc danh sách học kỳ đã phân trang.
    /// </summary>
    public async Task<PagedResult<SemesterResponse>> ListSemestersAsync(
        Guid? academicYearId,
        PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Semesters.AsNoTracking();
        if (academicYearId.HasValue)
        {
            query = query.Where(semester => semester.AcademicYearId == academicYearId.Value);
        }

        if (pageRequest.Search is not null)
        {
            var search = AcademicNormalization.Name(pageRequest.Search);
            query = query.Where(semester => semester.NormalizedName.Contains(search));
        }

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderBy(semester => semester.StartDate)
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .Select(semester => new SemesterResponse(
                semester.Id,
                semester.AcademicYearId,
                semester.Name,
                semester.StartDate,
                semester.EndDate,
                semester.GradeEntryFrom,
                semester.GradeEntryTo,
                semester.Status.ToString()))
            .ToListAsync(cancellationToken);

        return new PagedResult<SemesterResponse>(items, pageRequest.Page, pageRequest.PageSize, total);
    }

    /// <summary>
    /// Ghi chú: SubjectCodeExistsAsync kiểm tra mã môn học đã tồn tại.
    /// </summary>
    public Task<bool> SubjectCodeExistsAsync(string normalizedCode, CancellationToken cancellationToken) =>
        dbContext.Subjects.AnyAsync(subject => subject.NormalizedSubjectCode == normalizedCode, cancellationToken);

    /// <summary>
    /// Ghi chú: AddSubject thêm môn học mới vào DbContext.
    /// </summary>
    public void AddSubject(Subject subject) => dbContext.Subjects.Add(subject);

    /// <summary>
    /// Ghi chú: GetSubjectAsync lấy môn học theo id.
    /// </summary>
    public Task<Subject?> GetSubjectAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Subjects.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

    /// <summary>
    /// Ghi chú: ListSubjectsAsync đọc danh sách môn học đã phân trang.
    /// </summary>
    public async Task<PagedResult<SubjectResponse>> ListSubjectsAsync(
        bool? isActive,
        PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Subjects.AsNoTracking();
        if (isActive.HasValue)
        {
            query = query.Where(subject => subject.IsActive == isActive.Value);
        }

        if (pageRequest.Search is not null)
        {
            var search = AcademicNormalization.Code(pageRequest.Search);
            var pattern = $"%{pageRequest.Search.Trim()}%";
            query = query.Where(subject =>
                subject.NormalizedSubjectCode.Contains(search) ||
                EF.Functions.ILike(subject.Name, pattern));
        }

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderBy(subject => subject.SubjectCode)
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .Select(subject => new SubjectResponse(
                subject.Id,
                subject.SubjectCode,
                subject.Name,
                subject.Credits,
                subject.MaxScore,
                subject.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResult<SubjectResponse>(items, pageRequest.Page, pageRequest.PageSize, total);
    }
}
