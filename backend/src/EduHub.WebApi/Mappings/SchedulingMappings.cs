using EduHub.Application.Contracts.Scheduling;
using EduHub.WebApi.Dtos.Scheduling;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: SchedulingMappings chuyển đổi DTO API với command, query và response của module thời khóa biểu.
/// </summary>
public static class SchedulingMappings
{
    public static CreateCurriculumPlanCommand ToCommand(this CreateCurriculumPlanRequest request) =>
        new(
            request.AcademicYearId,
            request.GradeLevel,
            request.Name,
            request.TotalWeeks,
            request.Semester1Weeks,
            request.Semester2Weeks,
            request.SubjectQuotas.Select(quota => new CurriculumSubjectQuotaInput(
                quota.SubjectId,
                quota.Kind,
                quota.AnnualPeriods,
                quota.Semester1Periods,
                quota.Semester2Periods,
                quota.CanDoublePeriod,
                quota.MaxPeriodsPerDay,
                quota.IncludesHomeroom,
                quota.PreferredSession)).ToList());

    public static ListCurriculumPlansQuery ToQuery(this ListCurriculumPlansRequest request) => new(request.AcademicYearId, request.GradeLevel);
    public static CreateTeacherCapabilityCommand ToCommand(this CreateTeacherCapabilityRequest request) => new(request.TeacherId, request.SubjectId, request.Priority, request.MaxPeriodsPerWeek);
    public static ListTeacherCapabilitiesQuery ToQuery(this ListTeacherCapabilitiesRequest request) => new(request.TeacherId, request.SubjectId);
    public static AssignHomeroomTeacherCommand ToCommand(this AssignHomeroomTeacherRequest request, Guid classRoomId) => new(classRoomId, request.TeacherId);
    public static ListHomeroomAssignmentsQuery ToQuery(this ListHomeroomAssignmentsRequest request) => new(request.AcademicYearId);
    public static GenerateTimetableCommand ToCommand(this GenerateTimetableRequest request) => new(request.SemesterId, request.Name);
    public static GetTimetableEntriesQuery ToQuery(this GetTimetableEntriesRequest request, Guid versionId) => new(versionId, request.ClassRoomId, request.WeekNumber);
    public static MoveTimetableEntryCommand ToCommand(this MoveTimetableEntryRequest request, Guid entryId) => new(entryId, request.WeekNumber, request.DayOfWeek, request.Session, request.PeriodNumber);
    public static AssignClassSubjectTeacherCommand ToCommand(this AssignClassSubjectTeacherRequest request, Guid entryId) => new(entryId, request.TeacherId);
    public static SetTimetableEntryLockCommand ToCommand(this SetTimetableEntryLockRequest request, Guid entryId) => new(entryId, request.IsLocked);

    public static CurriculumPlanDto ToDto(CurriculumPlanResponse response) =>
        new(
            response.Id,
            response.AcademicYearId,
            response.GradeLevel,
            response.Name,
            response.TotalWeeks,
            response.Semester1Weeks,
            response.Semester2Weeks,
            response.AnnualPeriodTotal,
            response.IsActive,
            response.SubjectQuotas.Select(quota => new CurriculumSubjectQuotaDto(
                quota.Id,
                quota.SubjectId,
                quota.SubjectCode,
                quota.SubjectName,
                quota.Kind,
                quota.AnnualPeriods,
                quota.Semester1Periods,
                quota.Semester2Periods,
                quota.CanDoublePeriod,
                quota.MaxPeriodsPerDay,
                quota.IncludesHomeroom,
                quota.PreferredSession)).ToList());

    public static TeacherCapabilityDto ToDto(TeacherCapabilityResponse response) => new(response.Id, response.TeacherId, response.TeacherName, response.SubjectId, response.SubjectCode, response.SubjectName, response.Priority, response.MaxPeriodsPerWeek, response.IsActive);
    public static HomeroomAssignmentDto ToDto(HomeroomAssignmentResponse response) => new(response.Id, response.ClassRoomId, response.ClassCode, response.ClassName, response.TeacherId, response.TeacherName, response.IsActive);
    public static TimetableVersionDto ToDto(TimetableVersionResponse response) => new(response.Id, response.SemesterId, response.SemesterName, response.Name, response.Status, response.GeneratedAtUtc, response.PublishedAtUtc, response.EntryCount);
    public static GenerateTimetableDto ToDto(GenerateTimetableResponse response) => new(ToDto(response.Version), response.AutoCreatedTeachingAssignments, response.AutoCreatedHomeroomAssignments, response.EntryCount);
    public static TimetableWeekDto ToDto(TimetableWeekResponse response) => new(response.WeekNumber, response.StartDate, response.EndDate, response.IsCurrent);
    public static TimetableEntryDto ToDto(TimetableEntryResponse response) => new(response.Id, response.TimetableVersionId, response.ClassRoomId, response.ClassCode, response.ClassName, response.SubjectId, response.SubjectCode, response.SubjectName, response.TeacherId, response.TeacherName, response.WeekNumber, response.WeekStartDate, response.WeekEndDate, response.DayOfWeek, response.Session, response.PeriodNumber, response.StartTime, response.EndTime, response.Kind, response.CountsTowardQuota, response.IsLocked, response.Note);
}
