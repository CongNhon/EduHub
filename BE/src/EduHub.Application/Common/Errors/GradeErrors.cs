using EduHub.Application.Common.Models;

namespace EduHub.Application.Common.Errors;

/// <summary>
/// Ghi chú: GradeErrors chứa mã lỗi nghiệp vụ cho cấu hình điểm và GPA.
/// </summary>
public static class GradeErrors
{
    public static readonly Error SubjectNotFound = new(
        "Grade.SubjectNotFound",
        "Subject was not found.",
        ErrorType.NotFound);

    public static readonly Error SemesterNotFound = new(
        "Grade.SemesterNotFound",
        "Semester was not found.",
        ErrorType.NotFound);

    public static readonly Error InvalidComponentWeights = new(
        "Grade.InvalidComponentWeights",
        "Grade component weights must total exactly 1.00.",
        ErrorType.Conflict);

    public static readonly Error DuplicateComponentName = new(
        "Grade.DuplicateComponentName",
        "Grade component names must be unique in a configuration version.",
        ErrorType.Conflict);

    public static readonly Error DuplicateComponentOrder = new(
        "Grade.DuplicateComponentOrder",
        "Grade component display orders must be unique in a configuration version.",
        ErrorType.Conflict);

    public static readonly Error AssignmentNotFound = new(
        "Grade.AssignmentNotFound",
        "Teaching assignment was not found.",
        ErrorType.NotFound);

    public static readonly Error ComponentInvalid = new(
        "Grade.ComponentInvalid",
        "Grade component does not belong to the assignment subject and semester.",
        ErrorType.Conflict);

    public static readonly Error StudentNotEnrolled = new(
        "Grade.StudentNotEnrolled",
        "Student is not actively enrolled in the assignment class and semester.",
        ErrorType.Conflict);

    public static readonly Error TeacherForbidden = new(
        "Grade.TeacherForbidden",
        "Teacher can only update or submit assigned gradebooks.",
        ErrorType.Forbidden);

    public static readonly Error ScoreOutOfRange = new(
        "Grade.ScoreOutOfRange",
        "Score must be within the grade component max score.",
        ErrorType.Validation);

    public static readonly Error ScorePrecisionInvalid = new(
        "Grade.ScorePrecisionInvalid",
        "Score precision exceeds the configured maximum.",
        ErrorType.Validation);

    public static readonly Error VersionRequired = new(
        "Grade.VersionRequired",
        "Version is required when updating an existing grade.",
        ErrorType.Validation);

    public static readonly Error StaleVersion = new(
        "Grade.StaleVersion",
        "Grade version is stale.",
        ErrorType.Conflict);

    public static readonly Error InvalidGradeState = new(
        "Grade.InvalidGradeState",
        "Grade state does not allow this operation.",
        ErrorType.Conflict);

    public static readonly Error MissingRequiredGrades = new(
        "Grade.MissingRequiredGrades",
        "All required grade components must have scores before submit.",
        ErrorType.Conflict);

    public static readonly Error NoGrades = new(
        "Grade.NoGrades",
        "Gradebook has no grade entries.",
        ErrorType.Conflict);

    public static readonly Error ReasonRequired = new(
        "Grade.ReasonRequired",
        "Reason is required for reopen.",
        ErrorType.Validation);

    public static readonly Error ParentForbidden = new(
        "Grade.ParentForbidden",
        "Parent can only read published grades of linked students.",
        ErrorType.Forbidden);

    public static readonly Error EntryWindowClosed = new(
        "Grade.EntryWindowClosed",
        "Grade entry is outside the configured semester window.",
        ErrorType.Conflict);
}
