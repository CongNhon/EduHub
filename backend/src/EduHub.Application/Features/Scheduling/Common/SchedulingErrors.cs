using EduHub.Application.Common.Models;

namespace EduHub.Application.Features.Scheduling.Common;

/// <summary>
/// Ghi chú: SchedulingErrors tập trung lỗi nghiệp vụ chương trình học, giáo viên chủ nhiệm và thời khóa biểu.
/// </summary>
public static class SchedulingErrors
{
    public static readonly Error AcademicYearNotFound = new("Scheduling.AcademicYearNotFound", "Academic year was not found.", ErrorType.NotFound);
    public static readonly Error SemesterNotFound = new("Scheduling.SemesterNotFound", "Semester was not found.", ErrorType.NotFound);
    public static readonly Error CurriculumPlanExists = new("Scheduling.CurriculumPlanExists", "A curriculum plan already exists for this grade and academic year.", ErrorType.Conflict);
    public static readonly Error CurriculumSubjectInvalid = new("Scheduling.CurriculumSubjectInvalid", "One or more curriculum subjects are invalid or inactive.", ErrorType.Validation);
    public static readonly Error CurriculumTotalInvalid = new("Scheduling.CurriculumTotalInvalid", "Curriculum quotas do not match the configured semester totals.", ErrorType.Validation);
    public static readonly Error TeacherNotFound = new("Scheduling.TeacherNotFound", "An active teacher account was not found.", ErrorType.NotFound);
    public static readonly Error TeacherCapabilityRequired = new("Scheduling.TeacherCapabilityRequired", "The selected teacher is not qualified for this subject.", ErrorType.Conflict);
    public static readonly Error TeacherLoadExceeded = new("Scheduling.TeacherLoadExceeded", "The selected teacher would exceed the configured weekly teaching load.", ErrorType.Conflict);
    public static readonly Error SubjectNotFound = new("Scheduling.SubjectNotFound", "An active subject was not found.", ErrorType.NotFound);
    public static readonly Error CapabilityExists = new("Scheduling.CapabilityExists", "The teacher already has this subject capability.", ErrorType.Conflict);
    public static readonly Error CapabilityLimit = new("Scheduling.CapabilityLimit", "A teacher can have one primary subject and at most two secondary subjects.", ErrorType.Conflict);
    public static readonly Error ClassNotFound = new("Scheduling.ClassNotFound", "Class room was not found.", ErrorType.NotFound);
    public static readonly Error HomeroomExists = new("Scheduling.HomeroomExists", "The class or teacher already has an active homeroom assignment.", ErrorType.Conflict);
    public static readonly Error HomeroomTeachingConflict = new("Scheduling.HomeroomTeachingConflict", "The homeroom teacher cannot teach an academic subject in the same class.", ErrorType.Conflict);
    public static readonly Error HomeroomTeacherManagedSeparately = new("Scheduling.HomeroomTeacherManagedSeparately", "Use the homeroom assignment workflow to change the homeroom teacher.", ErrorType.Conflict);
    public static readonly Error GenerationDataIncomplete = new("Scheduling.GenerationDataIncomplete", "Curriculum, teacher capability, class or semester data is incomplete.", ErrorType.Conflict);
    public static readonly Error TeachingAssignmentInvalid = new("Scheduling.TeachingAssignmentInvalid", "The class and subject must have exactly one active teaching assignment.", ErrorType.Conflict);
    public static readonly Error TimetableNotFound = new("Scheduling.TimetableNotFound", "Timetable version was not found.", ErrorType.NotFound);
    public static readonly Error TimetableEntryNotFound = new("Scheduling.TimetableEntryNotFound", "Timetable entry was not found.", ErrorType.NotFound);
    public static readonly Error TimetableNotDraft = new("Scheduling.TimetableNotDraft", "Only a draft timetable can be modified or published.", ErrorType.Conflict);
    public static readonly Error TimetableSlotConflict = new("Scheduling.TimetableSlotConflict", "The target slot conflicts with the class or teacher timetable.", ErrorType.Conflict);
}
