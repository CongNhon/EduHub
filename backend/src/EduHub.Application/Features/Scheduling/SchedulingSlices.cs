using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Scheduling;
using EduHub.Application.Interfaces.Services.Scheduling;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Scheduling;

/// <summary>
/// Ghi chú: CreateCurriculumPlanCommandValidator kiểm tra tuần học và quota từng môn trước khi tạo chương trình khối.
/// </summary>
public sealed class CreateCurriculumPlanCommandValidator : AbstractValidator<CreateCurriculumPlanCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khai báo rule số tuần, khối 10-12 và tổng quota học kỳ của chương trình.
    /// </summary>
    public CreateCurriculumPlanCommandValidator()
    {
        RuleFor(command => command.AcademicYearId).NotEmpty();
        RuleFor(command => command.GradeLevel).InclusiveBetween(10, 12);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(160);
        RuleFor(command => command.TotalWeeks).Equal(35);
        RuleFor(command => command.Semester1Weeks).Equal(18);
        RuleFor(command => command.Semester2Weeks).Equal(17);
        RuleFor(command => command).Must(command => command.Semester1Weeks + command.Semester2Weeks == command.TotalWeeks)
            .WithMessage("Semester weeks must equal total weeks.");
        RuleFor(command => command.SubjectQuotas).NotEmpty();
        RuleFor(command => command).Must(command => HasValidWeeklyLoad(
                command.SubjectQuotas.Sum(quota => quota.Semester1Periods),
                command.Semester1Weeks))
            .WithMessage("Semester 1 must contain 29 morning periods plus zero or more complete 5-period afternoon sessions per week.");
        RuleFor(command => command).Must(command => HasValidWeeklyLoad(
                command.SubjectQuotas.Sum(quota => quota.Semester2Periods),
                command.Semester2Weeks))
            .WithMessage("Semester 2 must contain 29 morning periods plus zero or more complete 5-period afternoon sessions per week.");
        RuleFor(command => command.SubjectQuotas.Count(quota => quota.IncludesHomeroom)).Equal(1)
            .WithMessage("A curriculum must contain exactly one homeroom activity.");
        RuleForEach(command => command.SubjectQuotas).ChildRules(quota =>
        {
            quota.RuleFor(item => item.SubjectId).NotEmpty();
            quota.RuleFor(item => item.Kind).IsInEnum();
            quota.RuleFor(item => item.PreferredSession).IsInEnum().When(item => item.PreferredSession.HasValue);
            quota.RuleFor(item => item.AnnualPeriods).GreaterThan(0);
            quota.RuleFor(item => item).Must(item => item.Semester1Periods + item.Semester2Periods == item.AnnualPeriods)
                .WithMessage("Semester periods must equal annual periods.");
            quota.RuleFor(item => item.MaxPeriodsPerDay).InclusiveBetween(1, 2);
            quota.RuleFor(item => item).Must(item => !item.IncludesHomeroom ||
                    item.AnnualPeriods == 35 && item.Semester1Periods == 18 && item.Semester2Periods == 17 &&
                    item.MaxPeriodsPerDay == 1)
                .WithMessage("Homeroom activity requires 35 annual periods, an 18/17 split and one period per teaching week.");
        });
    }

    /// <summary>
    /// Ghi chú: HasValidWeeklyLoad kiểm tra tổng tiết học kỳ tạo được 29 tiết sáng và các buổi chiều đủ 5 tiết cho mỗi tuần.
    /// </summary>
    private static bool HasValidWeeklyLoad(int semesterPeriods, int semesterWeeks)
    {
        if (semesterWeeks <= 0 || semesterPeriods % semesterWeeks != 0) return false;
        var periodsPerWeek = semesterPeriods / semesterWeeks;
        return periodsPerWeek is >= 29 and <= 54 && (periodsPerWeek - 29) % 5 == 0;
    }
}

/// <summary>
/// Ghi chú: CreateCurriculumPlanCommandHandler chuyển yêu cầu tạo chương trình sang SchedulingService.
/// </summary>
public sealed class CreateCurriculumPlanCommandHandler(ISchedulingService service)
    : IRequestHandler<CreateCurriculumPlanCommand, Result<CurriculumPlanResponse>>
{
    public Task<Result<CurriculumPlanResponse>> Handle(CreateCurriculumPlanCommand request, CancellationToken cancellationToken) =>
        service.CreateCurriculumPlanAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListCurriculumPlansQueryHandler đọc chương trình học qua SchedulingService.
/// </summary>
public sealed class ListCurriculumPlansQueryHandler(ISchedulingService service)
    : IRequestHandler<ListCurriculumPlansQuery, Result<IReadOnlyList<CurriculumPlanResponse>>>
{
    public Task<Result<IReadOnlyList<CurriculumPlanResponse>>> Handle(ListCurriculumPlansQuery request, CancellationToken cancellationToken) =>
        service.ListCurriculumPlansAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: CreateTeacherCapabilityCommandValidator kiểm tra giáo viên, môn và tải tiết tối đa mỗi tuần.
/// </summary>
public sealed class CreateTeacherCapabilityCommandValidator : AbstractValidator<CreateTeacherCapabilityCommand>
{
    public CreateTeacherCapabilityCommandValidator()
    {
        RuleFor(command => command.TeacherId).NotEmpty();
        RuleFor(command => command.SubjectId).NotEmpty();
        RuleFor(command => command.Priority).IsInEnum();
        RuleFor(command => command.MaxPeriodsPerWeek).InclusiveBetween(1, 35);
    }
}

/// <summary>
/// Ghi chú: CreateTeacherCapabilityCommandHandler chuyển khai báo năng lực giáo viên sang SchedulingService.
/// </summary>
public sealed class CreateTeacherCapabilityCommandHandler(ISchedulingService service)
    : IRequestHandler<CreateTeacherCapabilityCommand, Result<TeacherCapabilityResponse>>
{
    public Task<Result<TeacherCapabilityResponse>> Handle(CreateTeacherCapabilityCommand request, CancellationToken cancellationToken) =>
        service.CreateTeacherCapabilityAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListTeacherCapabilitiesQueryHandler đọc môn chính/phụ của giáo viên qua SchedulingService.
/// </summary>
public sealed class ListTeacherCapabilitiesQueryHandler(ISchedulingService service)
    : IRequestHandler<ListTeacherCapabilitiesQuery, Result<IReadOnlyList<TeacherCapabilityResponse>>>
{
    public Task<Result<IReadOnlyList<TeacherCapabilityResponse>>> Handle(ListTeacherCapabilitiesQuery request, CancellationToken cancellationToken) =>
        service.ListTeacherCapabilitiesAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: AssignHomeroomTeacherCommandValidator kiểm tra id lớp và giáo viên trước khi phân công GVCN.
/// </summary>
public sealed class AssignHomeroomTeacherCommandValidator : AbstractValidator<AssignHomeroomTeacherCommand>
{
    public AssignHomeroomTeacherCommandValidator()
    {
        RuleFor(command => command.ClassRoomId).NotEmpty();
        RuleFor(command => command.TeacherId).NotEmpty();
    }
}

/// <summary>
/// Ghi chú: AssignHomeroomTeacherCommandHandler chuyển phân công GVCN sang SchedulingService.
/// </summary>
public sealed class AssignHomeroomTeacherCommandHandler(ISchedulingService service)
    : IRequestHandler<AssignHomeroomTeacherCommand, Result<HomeroomAssignmentResponse>>
{
    public Task<Result<HomeroomAssignmentResponse>> Handle(AssignHomeroomTeacherCommand request, CancellationToken cancellationToken) =>
        service.AssignHomeroomTeacherAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListHomeroomAssignmentsQueryHandler đọc danh sách GVCN của năm học qua SchedulingService.
/// </summary>
public sealed class ListHomeroomAssignmentsQueryHandler(ISchedulingService service)
    : IRequestHandler<ListHomeroomAssignmentsQuery, Result<IReadOnlyList<HomeroomAssignmentResponse>>>
{
    public Task<Result<IReadOnlyList<HomeroomAssignmentResponse>>> Handle(ListHomeroomAssignmentsQuery request, CancellationToken cancellationToken) =>
        service.ListHomeroomAssignmentsAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GenerateTimetableCommandValidator kiểm tra học kỳ và tên phiên bản trước khi chạy constraint solver.
/// </summary>
public sealed class GenerateTimetableCommandValidator : AbstractValidator<GenerateTimetableCommand>
{
    public GenerateTimetableCommandValidator()
    {
        RuleFor(command => command.SemesterId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(160);
    }
}

/// <summary>
/// Ghi chú: GenerateTimetableCommandHandler gọi SchedulingService để tự phân giáo viên và sinh lịch cho toàn học kỳ.
/// </summary>
public sealed class GenerateTimetableCommandHandler(ISchedulingService service)
    : IRequestHandler<GenerateTimetableCommand, Result<GenerateTimetableResponse>>
{
    public Task<Result<GenerateTimetableResponse>> Handle(GenerateTimetableCommand request, CancellationToken cancellationToken) =>
        service.GenerateTimetableAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListTimetableVersionsQueryHandler đọc lịch sử phiên bản thời khóa biểu của học kỳ.
/// </summary>
public sealed class ListTimetableVersionsQueryHandler(ISchedulingService service)
    : IRequestHandler<ListTimetableVersionsQuery, Result<IReadOnlyList<TimetableVersionResponse>>>
{
    public Task<Result<IReadOnlyList<TimetableVersionResponse>>> Handle(ListTimetableVersionsQuery request, CancellationToken cancellationToken) =>
        service.ListTimetableVersionsAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetPublishedTimetableVersionQueryHandler lấy phiên bản lịch đang công bố qua SchedulingService.
/// </summary>
public sealed class GetPublishedTimetableVersionQueryHandler(ISchedulingService service)
    : IRequestHandler<GetPublishedTimetableVersionQuery, Result<TimetableVersionResponse>>
{
    public Task<Result<TimetableVersionResponse>> Handle(GetPublishedTimetableVersionQuery request, CancellationToken cancellationToken) =>
        service.GetPublishedTimetableVersionAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetTimetableEntriesQueryHandler đọc lưới tiết của một phiên bản thời khóa biểu.
/// </summary>
public sealed class GetTimetableEntriesQueryHandler(ISchedulingService service)
    : IRequestHandler<GetTimetableEntriesQuery, Result<IReadOnlyList<TimetableEntryResponse>>>
{
    public Task<Result<IReadOnlyList<TimetableEntryResponse>>> Handle(GetTimetableEntriesQuery request, CancellationToken cancellationToken) =>
        service.GetTimetableEntriesAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListTimetableWeeksQueryHandler trả các tuần có ngày cụ thể và tuần hiện tại của học kỳ.
/// </summary>
public sealed class ListTimetableWeeksQueryHandler(ISchedulingService service)
    : IRequestHandler<ListTimetableWeeksQuery, Result<IReadOnlyList<TimetableWeekResponse>>>
{
    public Task<Result<IReadOnlyList<TimetableWeekResponse>>> Handle(ListTimetableWeeksQuery request, CancellationToken cancellationToken) =>
        service.ListTimetableWeeksAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: PublishTimetableCommandHandler công bố bản nháp thời khóa biểu qua SchedulingService.
/// </summary>
public sealed class PublishTimetableCommandHandler(ISchedulingService service)
    : IRequestHandler<PublishTimetableCommand, Result<TimetableVersionResponse>>
{
    public Task<Result<TimetableVersionResponse>> Handle(PublishTimetableCommand request, CancellationToken cancellationToken) =>
        service.PublishTimetableAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: MoveTimetableEntryCommandValidator kiểm tra slot thủ công trước khi service kiểm tra xung đột.
/// </summary>
public sealed class MoveTimetableEntryCommandValidator : AbstractValidator<MoveTimetableEntryCommand>
{
    public MoveTimetableEntryCommandValidator()
    {
        RuleFor(command => command.TimetableEntryId).NotEmpty();
        RuleFor(command => command.WeekNumber).InclusiveBetween(1, 40);
        RuleFor(command => command.DayOfWeek).InclusiveBetween(1, 6);
        RuleFor(command => command.Session).IsInEnum();
        RuleFor(command => command.PeriodNumber).InclusiveBetween(1, 5);
    }
}

/// <summary>
/// Ghi chú: AssignClassSubjectTeacherCommandValidator kiểm tra entry và giáo viên trước khi đổi toàn bộ assignment lớp-môn.
/// </summary>
public sealed class AssignClassSubjectTeacherCommandValidator : AbstractValidator<AssignClassSubjectTeacherCommand>
{
    public AssignClassSubjectTeacherCommandValidator()
    {
        RuleFor(command => command.TimetableEntryId).NotEmpty();
        RuleFor(command => command.TeacherId).NotEmpty();
    }
}

/// <summary>
/// Ghi chú: AssignClassSubjectTeacherCommandHandler chuyển lựa chọn giáo viên lớp-môn sang SchedulingService.
/// </summary>
public sealed class AssignClassSubjectTeacherCommandHandler(ISchedulingService service)
    : IRequestHandler<AssignClassSubjectTeacherCommand, Result<TimetableEntryResponse>>
{
    public Task<Result<TimetableEntryResponse>> Handle(AssignClassSubjectTeacherCommand request, CancellationToken cancellationToken) =>
        service.AssignClassSubjectTeacherAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: MoveTimetableEntryCommandHandler chuyển thao tác kéo-thả tiết học sang SchedulingService.
/// </summary>
public sealed class MoveTimetableEntryCommandHandler(ISchedulingService service)
    : IRequestHandler<MoveTimetableEntryCommand, Result<TimetableEntryResponse>>
{
    public Task<Result<TimetableEntryResponse>> Handle(MoveTimetableEntryCommand request, CancellationToken cancellationToken) =>
        service.MoveTimetableEntryAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: SetTimetableEntryLockCommandHandler khóa hoặc mở khóa một tiết trong bản nháp.
/// </summary>
public sealed class SetTimetableEntryLockCommandHandler(ISchedulingService service)
    : IRequestHandler<SetTimetableEntryLockCommand, Result<TimetableEntryResponse>>
{
    public Task<Result<TimetableEntryResponse>> Handle(SetTimetableEntryLockCommand request, CancellationToken cancellationToken) =>
        service.SetTimetableEntryLockAsync(request, cancellationToken);
}
