using EduHub.Application.Common.Models;

namespace EduHub.Application.Features.Students.Common;

/// <summary>
/// Ghi chú: StudentErrors gom các lỗi dùng khi xử lý mã lỗi nghiệp vụ học sinh/phụ huynh.
/// </summary>
public static class StudentErrors
{
    public static readonly Error StudentCodeExists = new(
        "student.code_exists",
        "Student code already exists.",
        ErrorType.Conflict);

    public static readonly Error StudentNotFound = new(
        "student.not_found",
        "Student was not found.",
        ErrorType.NotFound);

    public static readonly Error ParentUserInvalid = new(
        "parent_student.parent_user_invalid",
        "Parent user must be active and have Parent role.",
        ErrorType.Conflict);

    public static readonly Error ParentLinkExists = new(
        "parent_student.link_exists",
        "Parent link already exists.",
        ErrorType.Conflict);

    public static readonly Error ParentLinkNotFound = new(
        "parent_student.link_not_found",
        "Parent link was not found.",
        ErrorType.NotFound);

    public static readonly Error ConcurrencyConflict = new(
        "student.concurrency_conflict",
        "Student was changed by another request.",
        ErrorType.Conflict);

    public static readonly Error StudentUserInvalid = new(
        "student.user_invalid",
        "Student account must be active and have Student role.",
        ErrorType.Conflict);

    public static readonly Error StudentUserAlreadyLinked = new(
        "student.user_already_linked",
        "Student account is already linked to another student.",
        ErrorType.Conflict);

    public static readonly Error ParentRequired = new(
        "student.parent_required",
        "Parent role is required to list linked children.",
        ErrorType.Forbidden);
}
