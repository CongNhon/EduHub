using Carter;
using EduHub.Application.Contracts.Scheduling;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Scheduling;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Scheduling;

/// <summary>
/// Ghi chú: SchedulingModule đăng ký API chương trình học, năng lực giáo viên, GVCN và thời khóa biểu theo tuần thực tế.
/// </summary>
public sealed class SchedulingModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes cấu hình route quản trị và route xem thời khóa biểu đã phân quyền theo lớp.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var curriculum = app.MapGroup("/api/v1/curriculum-plans").WithTags("Scheduling");
        curriculum.MapGet("/", ListCurriculumPlansAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("ListCurriculumPlans");
        curriculum.MapPost("/", CreateCurriculumPlanAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("CreateCurriculumPlan");

        var capabilities = app.MapGroup("/api/v1/teacher-capabilities").WithTags("Scheduling");
        capabilities.MapGet("/", ListTeacherCapabilitiesAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("ListTeacherCapabilities");
        capabilities.MapPost("/", CreateTeacherCapabilityAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("CreateTeacherCapability");

        var homerooms = app.MapGroup("/api/v1/homeroom-assignments").WithTags("Scheduling");
        homerooms.MapGet("/", ListHomeroomAssignmentsAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("ListHomeroomAssignments");
        homerooms.MapPost("/classes/{classRoomId:guid}", AssignHomeroomTeacherAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("AssignHomeroomTeacher");

        var timetables = app.MapGroup("/api/v1/timetables").WithTags("Scheduling");
        timetables.MapPost("/generate", GenerateTimetableAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("GenerateTimetable");
        timetables.MapGet("/versions", ListTimetableVersionsAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("ListTimetableVersions");
        timetables.MapGet("/published", GetPublishedTimetableVersionAsync).RequireAuthorization().WithName("GetPublishedTimetableVersion");
        timetables.MapGet("/weeks", ListTimetableWeeksAsync).RequireAuthorization().WithName("ListTimetableWeeks");
        timetables.MapGet("/{versionId:guid}/entries", GetTimetableEntriesAsync).RequireAuthorization().WithName("GetTimetableEntries");
        timetables.MapPost("/{versionId:guid}/publish", PublishTimetableAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("PublishTimetable");
        timetables.MapPut("/entries/{entryId:guid}/slot", MoveTimetableEntryAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("MoveTimetableEntry");
        timetables.MapPut("/entries/{entryId:guid}/subject-teacher", AssignClassSubjectTeacherAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("AssignClassSubjectTeacher");
        timetables.MapPut("/entries/{entryId:guid}/lock", SetTimetableEntryLockAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("SetTimetableEntryLock");
    }

    private static async Task<IResult> CreateCurriculumPlanAsync(CreateCurriculumPlanRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result.ToCreatedHttpResult(response => $"/api/v1/curriculum-plans/{response.Id}", SchedulingMappings.ToDto);
    }

    private static async Task<IResult> ListCurriculumPlansAsync([AsParameters] ListCurriculumPlansRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken)).ToHttpResult(items => items.Select(SchedulingMappings.ToDto).ToList());

    private static async Task<IResult> CreateTeacherCapabilityAsync(CreateTeacherCapabilityRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result.ToCreatedHttpResult(response => $"/api/v1/teacher-capabilities/{response.Id}", SchedulingMappings.ToDto);
    }

    private static async Task<IResult> ListTeacherCapabilitiesAsync([AsParameters] ListTeacherCapabilitiesRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken)).ToHttpResult(items => items.Select(SchedulingMappings.ToDto).ToList());

    private static async Task<IResult> AssignHomeroomTeacherAsync(Guid classRoomId, AssignHomeroomTeacherRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(classRoomId), cancellationToken);
        return result.ToCreatedHttpResult(response => $"/api/v1/homeroom-assignments/{response.Id}", SchedulingMappings.ToDto);
    }

    private static async Task<IResult> ListHomeroomAssignmentsAsync([AsParameters] ListHomeroomAssignmentsRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken)).ToHttpResult(items => items.Select(SchedulingMappings.ToDto).ToList());

    private static async Task<IResult> GenerateTimetableAsync(GenerateTimetableRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(), cancellationToken)).ToHttpResult(SchedulingMappings.ToDto);

    private static async Task<IResult> ListTimetableVersionsAsync(Guid semesterId, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ListTimetableVersionsQuery(semesterId), cancellationToken)).ToHttpResult(items => items.Select(SchedulingMappings.ToDto).ToList());

    private static async Task<IResult> GetPublishedTimetableVersionAsync(Guid semesterId, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublishedTimetableVersionQuery(semesterId), cancellationToken)).ToHttpResult(SchedulingMappings.ToDto);

    private static async Task<IResult> ListTimetableWeeksAsync(Guid semesterId, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ListTimetableWeeksQuery(semesterId), cancellationToken)).ToHttpResult(items => items.Select(SchedulingMappings.ToDto).ToList());

    private static async Task<IResult> GetTimetableEntriesAsync(Guid versionId, [AsParameters] GetTimetableEntriesRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(versionId), cancellationToken)).ToHttpResult(items => items.Select(SchedulingMappings.ToDto).ToList());

    private static async Task<IResult> PublishTimetableAsync(Guid versionId, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new PublishTimetableCommand(versionId), cancellationToken)).ToHttpResult(SchedulingMappings.ToDto);

    private static async Task<IResult> MoveTimetableEntryAsync(Guid entryId, MoveTimetableEntryRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(entryId), cancellationToken)).ToHttpResult(SchedulingMappings.ToDto);

    private static async Task<IResult> AssignClassSubjectTeacherAsync(Guid entryId, AssignClassSubjectTeacherRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(entryId), cancellationToken)).ToHttpResult(SchedulingMappings.ToDto);

    private static async Task<IResult> SetTimetableEntryLockAsync(Guid entryId, SetTimetableEntryLockRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(entryId), cancellationToken)).ToHttpResult(SchedulingMappings.ToDto);
}
