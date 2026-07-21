using EduHub.Application.Common.Models;

namespace EduHub.Application.Features.Classes.Common;

/// <summary>
/// Ghi chú: ClassErrors gom các lỗi dùng khi xử lý lớp học, phân công giáo viên và ghi danh.
/// </summary>
public static class ClassErrors
{
    public static readonly Error ClassCodeExists = new("class.code_exists", "Class code already exists in the academic year.", ErrorType.Conflict);

    public static readonly Error ClassNotFound = new("class.not_found", "Class was not found.", ErrorType.NotFound);

    public static readonly Error AcademicYearNotFound = new("class.academic_year_not_found", "Academic year was not found.", ErrorType.NotFound);

    public static readonly Error SemesterNotFound = new("assignment.semester_not_found", "Semester was not found.", ErrorType.NotFound);

    public static readonly Error SubjectInvalid = new("assignment.subject_invalid", "Subject must be active.", ErrorType.Conflict);

    public static readonly Error TeacherInvalid = new("assignment.teacher_invalid", "Teacher must be active and have Teacher role.", ErrorType.Conflict);

    public static readonly Error TeacherCapabilityRequired = new("assignment.teacher_capability_required", "Teacher must have an active capability for the selected subject.", ErrorType.Conflict);

    public static readonly Error HomeroomTeachingConflict = new("assignment.homeroom_conflict", "The homeroom teacher cannot teach an academic subject in the same class.", ErrorType.Conflict);

    public static readonly Error AssignmentInvalidScope = new("assignment.invalid_scope", "Class and semester must belong to the same academic year.", ErrorType.Conflict);

    public static readonly Error AssignmentExists = new("assignment.exists", "Active teaching assignment already exists.", ErrorType.Conflict);

    public static readonly Error StudentInvalid = new("enrollment.student_invalid", "Student must be active.", ErrorType.Conflict);

    public static readonly Error EnrollmentExists = new("enrollment.exists", "Student already has an active enrollment in this semester.", ErrorType.Conflict);

    public static readonly Error ClassCapacityExceeded = new("class.capacity_exceeded", "Class capacity was exceeded.", ErrorType.Conflict);

    public static readonly Error EnrollmentNotFound = new("enrollment.not_found", "Active enrollment was not found.", ErrorType.NotFound);

    public static readonly Error DuplicateBulkStudent = new("enrollment.bulk_duplicate_student", "Student appears more than once in the bulk payload.", ErrorType.Conflict);
}
