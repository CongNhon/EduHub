using EduHub.Application.Contracts.Scheduling;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Enums;

namespace EduHub.Application.Interfaces.Repositories.Scheduling;

/// <summary>
/// Ghi chú: ISchedulingRepository là interface truy cập chương trình học, năng lực giáo viên và thời khóa biểu trong PostgreSQL.
/// </summary>
public interface ISchedulingRepository
{
    Task<AcademicYear?> GetAcademicYearAsync(Guid id, CancellationToken cancellationToken);
    Task<Semester?> GetSemesterAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> CurriculumPlanExistsAsync(Guid academicYearId, int gradeLevel, CancellationToken cancellationToken);
    Task<IReadOnlyList<Subject>> GetSubjectsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    void AddCurriculumPlan(CurriculumPlan plan);
    Task<IReadOnlyList<CurriculumPlanResponse>> ListCurriculumPlansAsync(Guid? academicYearId, int? gradeLevel, CancellationToken cancellationToken);
    Task<IReadOnlyList<CurriculumPlan>> GetCurriculumPlansForYearAsync(Guid academicYearId, CancellationToken cancellationToken);
    Task<User?> GetActiveTeacherAsync(Guid teacherId, CancellationToken cancellationToken);
    Task<Subject?> GetActiveSubjectAsync(Guid subjectId, CancellationToken cancellationToken);
    Task<bool> TeacherCapabilityExistsAsync(Guid teacherId, Guid subjectId, CancellationToken cancellationToken);
    Task<int> CountCapabilitiesAsync(Guid teacherId, TeacherSubjectPriority priority, CancellationToken cancellationToken);
    void AddTeacherCapability(TeacherSubjectCapability capability);
    Task<IReadOnlyList<TeacherCapabilityResponse>> ListTeacherCapabilitiesAsync(Guid? teacherId, Guid? subjectId, CancellationToken cancellationToken);
    Task<ClassRoom?> GetClassRoomAsync(Guid classRoomId, CancellationToken cancellationToken);
    /// <summary>
    /// Ghi chú: GetActiveHomeroomForClassAsync lấy phân công GVCN active có tracking để đổi giáo viên và giữ lịch sử.
    /// </summary>
    Task<HomeroomAssignment?> GetActiveHomeroomForClassAsync(Guid classRoomId, CancellationToken cancellationToken);
    Task<bool> HasActiveHomeroomForClassAsync(Guid classRoomId, CancellationToken cancellationToken);
    Task<bool> HasActiveHomeroomForTeacherAsync(Guid teacherId, CancellationToken cancellationToken);
    Task<bool> HasActiveTeachingAssignmentAsync(Guid classRoomId, Guid teacherId, CancellationToken cancellationToken);
    void AddHomeroomAssignment(HomeroomAssignment assignment);
    Task<IReadOnlyList<HomeroomAssignmentResponse>> ListHomeroomAssignmentsAsync(Guid? academicYearId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClassRoom>> ListClassRoomsAsync(Guid academicYearId, CancellationToken cancellationToken);
    Task<IReadOnlyList<User>> ListActiveTeachersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TeacherSubjectCapability>> ListActiveCapabilitiesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TeachingAssignment>> ListTeachingAssignmentsAsync(Guid semesterId, CancellationToken cancellationToken);
    Task<IReadOnlyList<HomeroomAssignment>> ListActiveHomeroomAssignmentsAsync(Guid academicYearId, CancellationToken cancellationToken);
    void AddTeachingAssignment(TeachingAssignment assignment);
    void AddTimetableVersion(TimetableVersion version);
    Task<IReadOnlyList<TimetableVersionResponse>> ListTimetableVersionsAsync(Guid semesterId, CancellationToken cancellationToken);
    Task<TimetableVersionResponse?> GetPublishedTimetableVersionAsync(Guid semesterId, CancellationToken cancellationToken);
    Task<TimetableVersion?> GetTimetableVersionAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TimetableEntryResponse>> ListTimetableEntriesAsync(Guid timetableVersionId, Guid? classRoomId, int? weekNumber, CancellationToken cancellationToken);
    Task<TimetableEntry?> GetTimetableEntryAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// Ghi chú: ListTimetableEntriesForClassSubjectAsync lấy toàn bộ tiết của một lớp-môn trong bản lịch để đổi giáo viên đồng bộ.
    /// </summary>
    Task<IReadOnlyList<TimetableEntry>> ListTimetableEntriesForClassSubjectAsync(Guid timetableVersionId, Guid classRoomId, Guid subjectId, CancellationToken cancellationToken);
    /// <summary>
    /// Ghi chú: ListTeacherTimetableEntriesAsync lấy lịch hiện có của giáo viên để kiểm tra xung đột và tải dạy.
    /// </summary>
    Task<IReadOnlyList<TimetableEntry>> ListTeacherTimetableEntriesAsync(Guid timetableVersionId, Guid teacherId, CancellationToken cancellationToken);
    /// <summary>
    /// Ghi chú: ListActiveTeachingAssignmentsForScopeAsync lấy assignment active của đúng lớp-môn-học kỳ để giữ sổ điểm khi đổi giáo viên.
    /// </summary>
    Task<IReadOnlyList<TeachingAssignment>> ListActiveTeachingAssignmentsForScopeAsync(Guid classRoomId, Guid subjectId, Guid semesterId, CancellationToken cancellationToken);
    Task<TimetableEntry?> GetClassSlotEntryAsync(Guid timetableVersionId, Guid classRoomId, int weekNumber, int dayOfWeek, TimetableSession session, int periodNumber, CancellationToken cancellationToken);
    Task<CurriculumSubjectQuota?> GetQuotaForClassSubjectAsync(Guid classRoomId, Guid subjectId, CancellationToken cancellationToken);
    Task<TeacherSubjectCapability?> GetActiveTeacherCapabilityAsync(Guid teacherId, Guid subjectId, CancellationToken cancellationToken);
    Task<bool> TeacherSlotConflictAsync(Guid timetableVersionId, Guid teacherId, int weekNumber, int dayOfWeek, TimetableSession session, int periodNumber, IReadOnlyCollection<Guid> excludedEntryIds, CancellationToken cancellationToken);
    Task<int> CountTeacherPeriodsAsync(Guid timetableVersionId, Guid teacherId, int weekNumber, CancellationToken cancellationToken);
    Task<bool> IsHomeroomTeacherForClassAsync(Guid teacherId, Guid classRoomId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TimetableVersion>> ListPublishedTimetableVersionsAsync(Guid semesterId, Guid exceptId, CancellationToken cancellationToken);
    Task<bool> CanViewClassTimetableAsync(Guid userId, UserRole role, Guid classRoomId, Guid semesterId, CancellationToken cancellationToken);
}
