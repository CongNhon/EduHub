using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;

namespace EduHub.Application.Contracts.Academics;

/// <summary>
/// Ghi chú: AcademicYearResponse là dữ liệu trả về cho năm học.
/// </summary>
public sealed record AcademicYearResponse(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status);

/// <summary>
/// Ghi chú: CreateAcademicYearCommand là command để tạo năm học mới.
/// </summary>
public sealed record CreateAcademicYearCommand(
    string Name,
    DateOnly StartDate,
    DateOnly EndDate) : ICommand<Result<AcademicYearResponse>>;

/// <summary>
/// Ghi chú: ListAcademicYearsQuery là query để đọc danh sách năm học.
/// </summary>
public sealed record ListAcademicYearsQuery(
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize,
    string? Search = null) : IQuery<Result<PagedResult<AcademicYearResponse>>>;

/// <summary>
/// Ghi chú: SemesterResponse là dữ liệu trả về cho học kỳ.
/// </summary>
public sealed record SemesterResponse(
    Guid Id,
    Guid AcademicYearId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    DateOnly GradeEntryFrom,
    DateOnly GradeEntryTo,
    string Status);

/// <summary>
/// Ghi chú: CreateSemesterCommand là command để tạo học kỳ mới.
/// </summary>
public sealed record CreateSemesterCommand(
    Guid AcademicYearId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    DateOnly GradeEntryFrom,
    DateOnly GradeEntryTo) : ICommand<Result<SemesterResponse>>;

/// <summary>
/// Ghi chú: ListSemestersQuery là query để đọc danh sách học kỳ.
/// </summary>
public sealed record ListSemestersQuery(
    Guid? AcademicYearId = null,
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize,
    string? Search = null) : IQuery<Result<PagedResult<SemesterResponse>>>;

/// <summary>
/// Ghi chú: SubjectResponse là dữ liệu trả về cho môn học.
/// </summary>
public sealed record SubjectResponse(
    Guid Id,
    string SubjectCode,
    string Name,
    int Credits,
    decimal MaxScore,
    bool IsActive);

/// <summary>
/// Ghi chú: CreateSubjectCommand là command để tạo môn học mới.
/// </summary>
public sealed record CreateSubjectCommand(
    string SubjectCode,
    string Name,
    int Credits,
    decimal? MaxScore) : ICommand<Result<SubjectResponse>>;

/// <summary>
/// Ghi chú: UpdateSubjectCommand là command để cập nhật môn học hiện có.
/// </summary>
public sealed record UpdateSubjectCommand(
    Guid Id,
    string Name,
    int Credits,
    decimal MaxScore) : ICommand<Result<SubjectResponse>>;

/// <summary>
/// Ghi chú: DisableSubjectCommand là command để vô hiệu hóa môn học.
/// </summary>
public sealed record DisableSubjectCommand(Guid Id) : ICommand<Result<SubjectResponse>>;

/// <summary>
/// Ghi chú: ListSubjectsQuery là query để đọc danh sách môn học.
/// </summary>
public sealed record ListSubjectsQuery(
    bool? IsActive = null,
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize,
    string? Search = null) : IQuery<Result<PagedResult<SubjectResponse>>>;
