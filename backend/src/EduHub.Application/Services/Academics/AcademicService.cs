using EduHub.Application.Common.Models;
using EduHub.Application.Common.Caching;
using EduHub.Application.Contracts.Academics;
using EduHub.Application.Features.Academics.Common;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.Academics;
using EduHub.Application.Interfaces.Services.Academics;
using EduHub.Application.Interfaces.Services.Caching;
using EduHub.Domain.Entities.Academics;

namespace EduHub.Application.Services.Academics;

/// <summary>
/// Ghi chú: AcademicService xử lý nghiệp vụ năm học, học kỳ và môn học.
/// </summary>
public sealed class AcademicService(
    IAcademicRepository academicRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ICacheKeyPolicy cacheKeyPolicy,
    TimeProvider timeProvider)
    : IAcademicService
{
    /// <summary>
    /// Ghi chú: CreateAcademicYearAsync tạo năm học mới nếu tên năm học chưa trùng.
    /// </summary>
    public async Task<Result<AcademicYearResponse>> CreateAcademicYearAsync(
        CreateAcademicYearCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedName = AcademicNormalization.Name(request.Name);
        var exists = await academicRepository.AcademicYearNameExistsAsync(normalizedName, cancellationToken);

        if (exists)
        {
            return Result.Failure<AcademicYearResponse>(AcademicErrors.AcademicYearNameExists);
        }

        var academicYear = new AcademicYear(
            request.Name.Trim(),
            normalizedName,
            request.StartDate,
            request.EndDate);

        academicRepository.AddAcademicYear(academicYear);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(academicYear));
    }

    /// <summary>
    /// Ghi chú: ListAcademicYearsAsync đọc danh sách năm học theo phân trang và tìm kiếm.
    /// </summary>
    public async Task<Result<PagedResult<AcademicYearResponse>>> ListAcademicYearsAsync(
        ListAcademicYearsQuery request,
        CancellationToken cancellationToken)
    {
        var pageRequest = PageRequest.Create(request.Page, request.PageSize, request.Search);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<PagedResult<AcademicYearResponse>>(pageRequest.Error!);
        }

        return Result.Success(await academicRepository.ListAcademicYearsAsync(pageRequest.Value, cancellationToken));
    }

    /// <summary>
    /// Ghi chú: CreateSemesterAsync tạo học kỳ nếu nằm trong năm học và không trùng/giao ngày.
    /// </summary>
    public async Task<Result<SemesterResponse>> CreateSemesterAsync(
        CreateSemesterCommand request,
        CancellationToken cancellationToken)
    {
        var academicYear = await academicRepository.GetAcademicYearAsync(request.AcademicYearId, cancellationToken);
        if (academicYear is null)
        {
            return Result.Failure<SemesterResponse>(AcademicErrors.AcademicYearNotFound);
        }

        if (request.StartDate < academicYear.StartDate || request.EndDate > academicYear.EndDate)
        {
            return Result.Failure<SemesterResponse>(AcademicErrors.SemesterOutsideAcademicYear);
        }

        var normalizedName = AcademicNormalization.Name(request.Name);
        var nameExists = await academicRepository.SemesterNameExistsAsync(
            request.AcademicYearId,
            normalizedName,
            cancellationToken);

        if (nameExists)
        {
            return Result.Failure<SemesterResponse>(AcademicErrors.SemesterNameExists);
        }

        var overlaps = await academicRepository.SemesterOverlapsAsync(
            request.AcademicYearId,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        if (overlaps)
        {
            return Result.Failure<SemesterResponse>(AcademicErrors.SemesterOverlaps);
        }

        var semester = new Semester(
            request.AcademicYearId,
            request.Name.Trim(),
            normalizedName,
            request.StartDate,
            request.EndDate,
            request.GradeEntryFrom,
            request.GradeEntryTo);

        academicRepository.AddSemester(semester);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(semester));
    }

    /// <summary>
    /// Ghi chú: ListSemestersAsync đọc danh sách học kỳ theo năm học, phân trang và tìm kiếm.
    /// </summary>
    public async Task<Result<PagedResult<SemesterResponse>>> ListSemestersAsync(
        ListSemestersQuery request,
        CancellationToken cancellationToken)
    {
        var pageRequest = PageRequest.Create(request.Page, request.PageSize, request.Search);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<PagedResult<SemesterResponse>>(pageRequest.Error!);
        }

        return Result.Success(await academicRepository.ListSemestersAsync(
            request.AcademicYearId,
            pageRequest.Value,
            cancellationToken));
    }

    /// <summary>
    /// Ghi chú: CreateSubjectAsync tạo môn học mới nếu mã môn chưa tồn tại.
    /// </summary>
    public async Task<Result<SubjectResponse>> CreateSubjectAsync(
        CreateSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedCode = AcademicNormalization.Code(request.SubjectCode);
        var exists = await academicRepository.SubjectCodeExistsAsync(normalizedCode, cancellationToken);

        if (exists)
        {
            return Result.Failure<SubjectResponse>(AcademicErrors.SubjectCodeExists);
        }

        var subject = new Subject(
            request.SubjectCode.Trim(),
            normalizedCode,
            request.Name.Trim(),
            request.Credits,
            request.MaxScore ?? 10m);

        academicRepository.AddSubject(subject);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheService.BumpVersionAsync(cacheKeyPolicy.SubjectCatalogScope(), cancellationToken);

        return Result.Success(ToResponse(subject));
    }

    /// <summary>
    /// Ghi chú: UpdateSubjectAsync cập nhật tên, credits và max score của môn học.
    /// </summary>
    public async Task<Result<SubjectResponse>> UpdateSubjectAsync(
        UpdateSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var subject = await academicRepository.GetSubjectAsync(request.Id, cancellationToken);
        if (subject is null)
        {
            return Result.Failure<SubjectResponse>(AcademicErrors.SubjectNotFound);
        }

        subject.Update(request.Name, request.Credits, request.MaxScore, timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheService.BumpVersionAsync(cacheKeyPolicy.SubjectCatalogScope(), cancellationToken);

        return Result.Success(ToResponse(subject));
    }

    /// <summary>
    /// Ghi chú: DisableSubjectAsync vô hiệu hóa môn học nhưng giữ lịch sử dữ liệu.
    /// </summary>
    public async Task<Result<SubjectResponse>> DisableSubjectAsync(
        DisableSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var subject = await academicRepository.GetSubjectAsync(request.Id, cancellationToken);
        if (subject is null)
        {
            return Result.Failure<SubjectResponse>(AcademicErrors.SubjectNotFound);
        }

        subject.Disable(timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheService.BumpVersionAsync(cacheKeyPolicy.SubjectCatalogScope(), cancellationToken);

        return Result.Success(ToResponse(subject));
    }

    /// <summary>
    /// Ghi chú: ListSubjectsAsync đọc danh sách môn học theo active, phân trang và tìm kiếm.
    /// </summary>
    public async Task<Result<PagedResult<SubjectResponse>>> ListSubjectsAsync(
        ListSubjectsQuery request,
        CancellationToken cancellationToken)
    {
        var pageRequest = PageRequest.Create(request.Page, request.PageSize, request.Search);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<PagedResult<SubjectResponse>>(pageRequest.Error!);
        }

        var version = await cacheService.GetVersionAsync(cacheKeyPolicy.SubjectCatalogScope(), cancellationToken);
        var cacheKey = cacheKeyPolicy.SubjectCatalog(
            version,
            request.IsActive,
            pageRequest.Value.Page,
            pageRequest.Value.PageSize,
            pageRequest.Value.Search);

        var result = await cacheService.GetOrCreateAsync(
            cacheKey,
            token => academicRepository.ListSubjectsAsync(request.IsActive, pageRequest.Value, token),
            new CacheEntryOptions(TimeSpan.FromMinutes(10)),
            cancellationToken);

        return Result.Success(result);
    }

    /// <summary>
    /// Ghi chú: ToResponse chuyển entity năm học thành DTO trả về API.
    /// </summary>
    private static AcademicYearResponse ToResponse(AcademicYear year) =>
        new(year.Id, year.Name, year.StartDate, year.EndDate, year.Status.ToString());

    /// <summary>
    /// Ghi chú: ToResponse chuyển entity học kỳ thành DTO trả về API.
    /// </summary>
    private static SemesterResponse ToResponse(Semester semester) =>
        new(
            semester.Id,
            semester.AcademicYearId,
            semester.Name,
            semester.StartDate,
            semester.EndDate,
            semester.GradeEntryFrom,
            semester.GradeEntryTo,
            semester.Status.ToString());

    /// <summary>
    /// Ghi chú: ToResponse chuyển entity môn học thành DTO trả về API.
    /// </summary>
    private static SubjectResponse ToResponse(Subject subject) =>
        new(subject.Id, subject.SubjectCode, subject.Name, subject.Credits, subject.MaxScore, subject.IsActive);
}
