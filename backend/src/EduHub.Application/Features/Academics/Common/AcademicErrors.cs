using EduHub.Application.Common.Models;

namespace EduHub.Application.Features.Academics.Common;

/// <summary>
/// Ghi chú: AcademicErrors gom các lỗi dùng khi xử lý mã lỗi nghiệp vụ năm học/học kỳ/môn học.
/// </summary>
public static class AcademicErrors
{
    public static readonly Error AcademicYearNameExists = new(
        "academic_year.name_exists",
        "Academic year name already exists.",
        ErrorType.Conflict);

    public static readonly Error AcademicYearNotFound = new(
        "academic_year.not_found",
        "Academic year was not found.",
        ErrorType.NotFound);

    public static readonly Error SemesterNameExists = new(
        "semester.name_exists",
        "Semester name already exists for the academic year.",
        ErrorType.Conflict);

    public static readonly Error SemesterOutsideAcademicYear = new(
        "semester.outside_academic_year",
        "Semester dates must stay within the academic year.",
        ErrorType.Validation);

    public static readonly Error SemesterOverlaps = new(
        "semester.overlaps",
        "Semester overlaps another semester in the same academic year.",
        ErrorType.Conflict);

    public static readonly Error SubjectCodeExists = new(
        "subject.code_exists",
        "Subject code already exists.",
        ErrorType.Conflict);

    public static readonly Error SubjectNotFound = new(
        "subject.not_found",
        "Subject was not found.",
        ErrorType.NotFound);
}
