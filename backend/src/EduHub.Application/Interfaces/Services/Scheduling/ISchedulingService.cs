using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Scheduling;

namespace EduHub.Application.Interfaces.Services.Scheduling;

/// <summary>
/// Ghi chú: ISchedulingService là interface nghiệp vụ chương trình học, phân công và thời khóa biểu tự động.
/// </summary>
public interface ISchedulingService
{
    Task<Result<CurriculumPlanResponse>> CreateCurriculumPlanAsync(CreateCurriculumPlanCommand request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<CurriculumPlanResponse>>> ListCurriculumPlansAsync(ListCurriculumPlansQuery request, CancellationToken cancellationToken);
    Task<Result<TeacherCapabilityResponse>> CreateTeacherCapabilityAsync(CreateTeacherCapabilityCommand request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<TeacherCapabilityResponse>>> ListTeacherCapabilitiesAsync(ListTeacherCapabilitiesQuery request, CancellationToken cancellationToken);
    Task<Result<HomeroomAssignmentResponse>> AssignHomeroomTeacherAsync(AssignHomeroomTeacherCommand request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<HomeroomAssignmentResponse>>> ListHomeroomAssignmentsAsync(ListHomeroomAssignmentsQuery request, CancellationToken cancellationToken);
    Task<Result<GenerateTimetableResponse>> GenerateTimetableAsync(GenerateTimetableCommand request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<TimetableVersionResponse>>> ListTimetableVersionsAsync(ListTimetableVersionsQuery request, CancellationToken cancellationToken);
    Task<Result<TimetableVersionResponse>> GetPublishedTimetableVersionAsync(GetPublishedTimetableVersionQuery request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<TimetableEntryResponse>>> GetTimetableEntriesAsync(GetTimetableEntriesQuery request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<TimetableWeekResponse>>> ListTimetableWeeksAsync(ListTimetableWeeksQuery request, CancellationToken cancellationToken);
    Task<Result<TimetableVersionResponse>> PublishTimetableAsync(PublishTimetableCommand request, CancellationToken cancellationToken);
    Task<Result<TimetableEntryResponse>> MoveTimetableEntryAsync(MoveTimetableEntryCommand request, CancellationToken cancellationToken);
    Task<Result<TimetableEntryResponse>> AssignClassSubjectTeacherAsync(AssignClassSubjectTeacherCommand request, CancellationToken cancellationToken);
    Task<Result<TimetableEntryResponse>> SetTimetableEntryLockAsync(SetTimetableEntryLockCommand request, CancellationToken cancellationToken);
}
