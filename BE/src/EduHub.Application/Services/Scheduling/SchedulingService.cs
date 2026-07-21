using EduHub.Application.Common.Models;
using EduHub.Application.Common.Scheduling;
using EduHub.Application.Contracts.Scheduling;
using EduHub.Application.Features.Scheduling.Common;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.Scheduling;
using EduHub.Application.Interfaces.Services.Scheduling;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Enums;

namespace EduHub.Application.Services.Scheduling;

/// <summary>
/// Ghi chú: SchedulingService xử lý chương trình học, năng lực giáo viên, GVCN và thời khóa biểu theo từng tuần thực tế.
/// </summary>
public sealed class 
    SchedulingService(
    ISchedulingRepository repository,
    ITimetableGenerator timetableGenerator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ISchedulingService
{
    private static readonly Error AdminRequired = new(
        "Scheduling.AdminRequired",
        "Academic administrator role is required.",
        ErrorType.Forbidden);

    /// <summary>
    /// Ghi chú: CreateCurriculumPlanAsync tạo chương trình khối và toàn bộ quota môn sau khi kiểm tra năm học, môn và tổng tiết.
    /// </summary>
    public async Task<Result<CurriculumPlanResponse>> CreateCurriculumPlanAsync(CreateCurriculumPlanCommand request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator()) return Result.Failure<CurriculumPlanResponse>(AdminRequired);
        if (await repository.GetAcademicYearAsync(request.AcademicYearId, cancellationToken) is null)
        {
            return Result.Failure<CurriculumPlanResponse>(SchedulingErrors.AcademicYearNotFound);
        }

        if (await repository.CurriculumPlanExistsAsync(request.AcademicYearId, request.GradeLevel, cancellationToken))
        {
            return Result.Failure<CurriculumPlanResponse>(SchedulingErrors.CurriculumPlanExists);
        }

        var subjectIds = request.SubjectQuotas.Select(quota => quota.SubjectId).ToList();
        if (subjectIds.Count == 0 || subjectIds.Distinct().Count() != subjectIds.Count)
        {
            return Result.Failure<CurriculumPlanResponse>(SchedulingErrors.CurriculumSubjectInvalid);
        }

        var subjects = await repository.GetSubjectsAsync(subjectIds, cancellationToken);
        if (subjects.Count != subjectIds.Count)
        {
            return Result.Failure<CurriculumPlanResponse>(SchedulingErrors.CurriculumSubjectInvalid);
        }

        var homeroomQuota = request.SubjectQuotas.SingleOrDefault(quota => quota.IncludesHomeroom);
        if (homeroomQuota is null || subjects.Single(subject => subject.Id == homeroomQuota.SubjectId).NormalizedSubjectCode != "HOMEROOM")
        {
            return Result.Failure<CurriculumPlanResponse>(SchedulingErrors.CurriculumSubjectInvalid);
        }

        var plan = new CurriculumPlan(
            request.AcademicYearId,
            request.GradeLevel,
            request.Name,
            request.TotalWeeks,
            request.Semester1Weeks,
            request.Semester2Weeks);

        foreach (var input in request.SubjectQuotas)
        {
            plan.SubjectQuotas.Add(new CurriculumSubjectQuota(
                plan.Id,
                input.SubjectId,
                input.Kind,
                input.AnnualPeriods,
                input.Semester1Periods,
                input.Semester2Periods,
                input.CanDoublePeriod,
                input.MaxPeriodsPerDay,
                input.IncludesHomeroom,
                input.PreferredSession));
        }

        repository.AddCurriculumPlan(plan);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToCurriculumResponse(plan, subjects));
    }

    /// <summary>
    /// Ghi chú: ListCurriculumPlansAsync đọc chương trình và quota theo bộ lọc năm học hoặc khối.
    /// </summary>
    public async Task<Result<IReadOnlyList<CurriculumPlanResponse>>> ListCurriculumPlansAsync(ListCurriculumPlansQuery request, CancellationToken cancellationToken) =>
        Result.Success(await repository.ListCurriculumPlansAsync(request.AcademicYearId, request.GradeLevel, cancellationToken));

    /// <summary>
    /// Ghi chú: CreateTeacherCapabilityAsync khai báo môn chính/phụ và chặn giáo viên có quá một môn chính hoặc hai môn phụ.
    /// </summary>
    public async Task<Result<TeacherCapabilityResponse>> CreateTeacherCapabilityAsync(CreateTeacherCapabilityCommand request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator()) return Result.Failure<TeacherCapabilityResponse>(AdminRequired);
        var teacher = await repository.GetActiveTeacherAsync(request.TeacherId, cancellationToken);
        if (teacher is null) return Result.Failure<TeacherCapabilityResponse>(SchedulingErrors.TeacherNotFound);
        var subject = await repository.GetActiveSubjectAsync(request.SubjectId, cancellationToken);
        if (subject is null) return Result.Failure<TeacherCapabilityResponse>(SchedulingErrors.SubjectNotFound);
        if (await repository.TeacherCapabilityExistsAsync(request.TeacherId, request.SubjectId, cancellationToken))
        {
            return Result.Failure<TeacherCapabilityResponse>(SchedulingErrors.CapabilityExists);
        }

        var limit = request.Priority == TeacherSubjectPriority.Primary ? 1 : 2;
        if (await repository.CountCapabilitiesAsync(request.TeacherId, request.Priority, cancellationToken) >= limit)
        {
            return Result.Failure<TeacherCapabilityResponse>(SchedulingErrors.CapabilityLimit);
        }

        var capability = new TeacherSubjectCapability(request.TeacherId, request.SubjectId, request.Priority, request.MaxPeriodsPerWeek);
        repository.AddTeacherCapability(capability);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new TeacherCapabilityResponse(
            capability.Id,
            teacher.Id,
            teacher.FullName,
            subject.Id,
            subject.SubjectCode,
            subject.Name,
            capability.Priority.ToString(),
            capability.MaxPeriodsPerWeek,
            capability.IsActive));
    }

    /// <summary>
    /// Ghi chú: ListTeacherCapabilitiesAsync đọc danh sách môn giáo viên có thể dạy để kiểm soát auto assignment.
    /// </summary>
    public async Task<Result<IReadOnlyList<TeacherCapabilityResponse>>> ListTeacherCapabilitiesAsync(ListTeacherCapabilitiesQuery request, CancellationToken cancellationToken) =>
        Result.Success(await repository.ListTeacherCapabilitiesAsync(request.TeacherId, request.SubjectId, cancellationToken));

    /// <summary>
    /// Ghi chú: AssignHomeroomTeacherAsync gắn GVCN và ngăn giáo viên đang dạy môn học trong chính lớp đó.
    /// </summary>
    public async Task<Result<HomeroomAssignmentResponse>> AssignHomeroomTeacherAsync(AssignHomeroomTeacherCommand request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator()) return Result.Failure<HomeroomAssignmentResponse>(AdminRequired);
        var classRoom = await repository.GetClassRoomAsync(request.ClassRoomId, cancellationToken);
        if (classRoom is null) return Result.Failure<HomeroomAssignmentResponse>(SchedulingErrors.ClassNotFound);
        var teacher = await repository.GetActiveTeacherAsync(request.TeacherId, cancellationToken);
        if (teacher is null) return Result.Failure<HomeroomAssignmentResponse>(SchedulingErrors.TeacherNotFound);
        var currentAssignment = await repository.GetActiveHomeroomForClassAsync(request.ClassRoomId, cancellationToken);
        if (currentAssignment?.TeacherId == request.TeacherId)
        {
            return Result.Success(new HomeroomAssignmentResponse(
                currentAssignment.Id,
                currentAssignment.ClassRoomId,
                currentAssignment.ClassRoom.ClassCode,
                currentAssignment.ClassRoom.Name,
                currentAssignment.TeacherId,
                currentAssignment.Teacher.FullName,
                currentAssignment.IsActive));
        }

        if (await repository.HasActiveHomeroomForTeacherAsync(request.TeacherId, cancellationToken))
        {
            return Result.Failure<HomeroomAssignmentResponse>(SchedulingErrors.HomeroomExists);
        }

        if (await repository.HasActiveTeachingAssignmentAsync(request.ClassRoomId, request.TeacherId, cancellationToken))
        {
            return Result.Failure<HomeroomAssignmentResponse>(SchedulingErrors.HomeroomTeachingConflict);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (currentAssignment is not null)
        {
            currentAssignment.Deactivate(now);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var assignment = new HomeroomAssignment(classRoom.Id, teacher.Id, now);
        repository.AddHomeroomAssignment(assignment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new HomeroomAssignmentResponse(
            assignment.Id,
            classRoom.Id,
            classRoom.ClassCode,
            classRoom.Name,
            teacher.Id,
            teacher.FullName,
            assignment.IsActive));
    }

    /// <summary>
    /// Ghi chú: ListHomeroomAssignmentsAsync đọc giáo viên chủ nhiệm của các lớp trong năm học.
    /// </summary>
    public async Task<Result<IReadOnlyList<HomeroomAssignmentResponse>>> ListHomeroomAssignmentsAsync(ListHomeroomAssignmentsQuery request, CancellationToken cancellationToken) =>
        Result.Success(await repository.ListHomeroomAssignmentsAsync(request.AcademicYearId, cancellationToken));

    /// <summary>
    /// Ghi chú: GenerateTimetableAsync bổ sung GVCN/phân công còn thiếu, gọi constraint solver và lưu lịch của toàn bộ tuần trong học kỳ.
    /// </summary>
    public async Task<Result<GenerateTimetableResponse>> GenerateTimetableAsync(GenerateTimetableCommand request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator() || currentUser.UserId is null) return Result.Failure<GenerateTimetableResponse>(AdminRequired);
        var semester = await repository.GetSemesterAsync(request.SemesterId, cancellationToken);
        if (semester is null) return Result.Failure<GenerateTimetableResponse>(SchedulingErrors.SemesterNotFound);

        var classes = (await repository.ListClassRoomsAsync(semester.AcademicYearId, cancellationToken)).ToList();
        var plans = (await repository.GetCurriculumPlansForYearAsync(semester.AcademicYearId, cancellationToken)).ToList();
        var teachers = (await repository.ListActiveTeachersAsync(cancellationToken)).ToList();
        var capabilities = (await repository.ListActiveCapabilitiesAsync(cancellationToken)).ToList();
        var assignments = (await repository.ListTeachingAssignmentsAsync(semester.Id, cancellationToken)).ToList();
        var homerooms = (await repository.ListActiveHomeroomAssignmentsAsync(semester.AcademicYearId, cancellationToken)).ToList();

        if (classes.Count == 0 || plans.Count == 0 || teachers.Count == 0 || capabilities.Count == 0 ||
            classes.Any(classRoom => plans.All(plan => plan.GradeLevel != classRoom.GradeLevel)))
        {
            return Result.Failure<GenerateTimetableResponse>(SchedulingErrors.GenerationDataIncomplete);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var weekCount = GetSemesterWeekCount(semester, plans);
        var loadByTeacher = BuildTeacherLoads(assignments, classes, plans, semester, weekCount);
        var autoHomerooms = AutoAssignHomerooms(classes, teachers, assignments, homerooms, loadByTeacher, now);
        if (autoHomerooms is null)
        {
            return Result.Failure<GenerateTimetableResponse>(SchedulingErrors.GenerationDataIncomplete);
        }

        foreach (var assignment in autoHomerooms) repository.AddHomeroomAssignment(assignment);
        homerooms.AddRange(autoHomerooms);

        var autoTeachingAssignments = AutoAssignTeachers(
            semester.Id,
            classes,
            plans,
            capabilities,
            assignments,
            homerooms,
            loadByTeacher,
            semester,
            weekCount,
            now);
        if (autoTeachingAssignments is null)
        {
            return Result.Failure<GenerateTimetableResponse>(SchedulingErrors.GenerationDataIncomplete);
        }

        foreach (var assignment in autoTeachingAssignments) repository.AddTeachingAssignment(assignment);
        assignments.AddRange(autoTeachingAssignments);

        var requirements = BuildRequirements(classes, plans, assignments, homerooms, semester, weekCount);
        if (requirements is null)
        {
            return Result.Failure<GenerateTimetableResponse>(SchedulingErrors.CurriculumTotalInvalid);
        }
        var generated = timetableGenerator.Generate(requirements);
        if (!generated.Success)
        {
            return Result.Failure<GenerateTimetableResponse>(new Error(
                "Scheduling.NoFeasibleTimetable",
                generated.FailureReason ?? "No feasible timetable could be generated.",
                ErrorType.Conflict));
        }

        var version = new TimetableVersion(semester.Id, request.Name, currentUser.UserId.Value, now);
        foreach (var placement in generated.Placements)
        {
            version.Entries.Add(new TimetableEntry(
                version.Id,
                placement.ClassRoomId,
                placement.SubjectId,
                placement.TeacherId,
                placement.WeekNumber,
                placement.DayOfWeek,
                placement.Session,
                placement.PeriodNumber,
                TimetableEntryKind.Curriculum,
                true,
                false,
                placement.IsHomeroom ? "Sinh hoạt lớp" : null));
        }

        repository.AddTimetableVersion(version);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var versionResponse = ToVersionResponse(version, semester.Name);
        return Result.Success(new GenerateTimetableResponse(
            versionResponse,
            autoTeachingAssignments.Count,
            autoHomerooms.Count,
            version.Entries.Count));
    }

    /// <summary>
    /// Ghi chú: ListTimetableVersionsAsync đọc các bản nháp, công bố và lưu trữ của một học kỳ.
    /// </summary>
    public async Task<Result<IReadOnlyList<TimetableVersionResponse>>> ListTimetableVersionsAsync(ListTimetableVersionsQuery request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator()) return Result.Failure<IReadOnlyList<TimetableVersionResponse>>(AdminRequired);
        if (await repository.GetSemesterAsync(request.SemesterId, cancellationToken) is null)
        {
            return Result.Failure<IReadOnlyList<TimetableVersionResponse>>(SchedulingErrors.SemesterNotFound);
        }

        return Result.Success(await repository.ListTimetableVersionsAsync(request.SemesterId, cancellationToken));
    }

    /// <summary>
    /// Ghi chú: GetPublishedTimetableVersionAsync trả phiên bản công bố của học kỳ cho người dùng đã đăng nhập.
    /// </summary>
    public async Task<Result<TimetableVersionResponse>> GetPublishedTimetableVersionAsync(GetPublishedTimetableVersionQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated) return Result.Failure<TimetableVersionResponse>(AdminRequired);
        var version = await repository.GetPublishedTimetableVersionAsync(request.SemesterId, cancellationToken);
        return version is null
            ? Result.Failure<TimetableVersionResponse>(SchedulingErrors.TimetableNotFound)
            : Result.Success(version);
    }

    /// <summary>
    /// Ghi chú: ListTimetableWeeksAsync tạo danh sách tuần Thứ Hai-Thứ Bảy và đánh dấu tuần chứa ngày hiện tại.
    /// </summary>
    public async Task<Result<IReadOnlyList<TimetableWeekResponse>>> ListTimetableWeeksAsync(ListTimetableWeeksQuery request, CancellationToken cancellationToken)
    {
        var semester = await repository.GetSemesterAsync(request.SemesterId, cancellationToken);
        if (semester is null) return Result.Failure<IReadOnlyList<TimetableWeekResponse>>(SchedulingErrors.SemesterNotFound);
        var plans = await repository.GetCurriculumPlansForYearAsync(semester.AcademicYearId, cancellationToken);
        if (plans.Count == 0) return Result.Failure<IReadOnlyList<TimetableWeekResponse>>(SchedulingErrors.GenerationDataIncomplete);
        var weekCount = GetSemesterWeekCount(semester, plans);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().ToOffset(TimeSpan.FromHours(7)).DateTime);
        var weeks = Enumerable.Range(1, weekCount).Select(weekNumber =>
        {
            var dates = TimetableCalendar.GetWeekDates(semester.StartDate, weekNumber);
            return new TimetableWeekResponse(weekNumber, dates.StartDate, dates.EndDate, today >= dates.StartDate && today <= dates.EndDate);
        }).ToList();
        return Result.Success<IReadOnlyList<TimetableWeekResponse>>(weeks);
    }

    /// <summary>
    /// Ghi chú: GetTimetableEntriesAsync đọc lưới tiết học của phiên bản và tùy chọn lọc một lớp.
    /// </summary>
    public async Task<Result<IReadOnlyList<TimetableEntryResponse>>> GetTimetableEntriesAsync(GetTimetableEntriesQuery request, CancellationToken cancellationToken)
    {
        var version = await repository.GetTimetableVersionAsync(request.TimetableVersionId, cancellationToken);
        if (version is null)
        {
            return Result.Failure<IReadOnlyList<TimetableEntryResponse>>(SchedulingErrors.TimetableNotFound);
        }

        if (!IsAcademicAdministrator())
        {
            if (version.Status != TimetableVersionStatus.Published || request.ClassRoomId is null || currentUser.UserId is null || currentUser.Role is null ||
                !await repository.CanViewClassTimetableAsync(currentUser.UserId.Value, currentUser.Role.Value, request.ClassRoomId.Value, version.SemesterId, cancellationToken))
            {
                return Result.Failure<IReadOnlyList<TimetableEntryResponse>>(new Error("Scheduling.TimetableForbidden", "You cannot view this class timetable.", ErrorType.Forbidden));
            }
        }

        return Result.Success(await repository.ListTimetableEntriesAsync(request.TimetableVersionId, request.ClassRoomId, request.WeekNumber, cancellationToken));
    }

    /// <summary>
    /// Ghi chú: PublishTimetableAsync lưu trữ lịch cũ và công bố bản nháp đã chọn.
    /// </summary>
    public async Task<Result<TimetableVersionResponse>> PublishTimetableAsync(PublishTimetableCommand request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator()) return Result.Failure<TimetableVersionResponse>(AdminRequired);
        var version = await repository.GetTimetableVersionAsync(request.TimetableVersionId, cancellationToken);
        if (version is null) return Result.Failure<TimetableVersionResponse>(SchedulingErrors.TimetableNotFound);
        if (version.Status != TimetableVersionStatus.Draft) return Result.Failure<TimetableVersionResponse>(SchedulingErrors.TimetableNotDraft);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var published in await repository.ListPublishedTimetableVersionsAsync(version.SemesterId, version.Id, cancellationToken))
        {
            published.Archive(now);
        }

        version.Publish(now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToVersionResponse(version, version.Semester.Name));
    }

    /// <summary>
    /// Ghi chú: MoveTimetableEntryAsync kiểm tra slot, rule tiết đôi, tải giáo viên rồi hoán đổi hai tiết trong transaction.
    /// </summary>
    public async Task<Result<TimetableEntryResponse>> MoveTimetableEntryAsync(MoveTimetableEntryCommand request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator()) return Result.Failure<TimetableEntryResponse>(AdminRequired);
        var entry = await repository.GetTimetableEntryAsync(request.TimetableEntryId, cancellationToken);
        if (entry is null) return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TimetableEntryNotFound);
        if (entry.TimetableVersion.Status != TimetableVersionStatus.Draft || entry.IsLocked)
        {
            return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TimetableNotDraft);
        }

        if (!IsValidSlot(request.DayOfWeek, request.Session, request.PeriodNumber))
        {
            return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TimetableSlotConflict);
        }

        var target = await repository.GetClassSlotEntryAsync(entry.TimetableVersionId, entry.ClassRoomId, request.WeekNumber, request.DayOfWeek, request.Session, request.PeriodNumber, cancellationToken);
        if (target?.Id == entry.Id) return Result.Success(ToEntryResponse(entry));
        if (target is null || target.IsLocked || entry.Note == "Sinh hoạt lớp" || target.Note == "Sinh hoạt lớp")
        {
            return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TimetableSlotConflict);
        }

        var excludedEntryIds = new[] { entry.Id, target.Id };
        if ((entry.TeacherId.HasValue && await repository.TeacherSlotConflictAsync(entry.TimetableVersionId, entry.TeacherId.Value, target.WeekNumber, target.DayOfWeek, target.Session, target.PeriodNumber, excludedEntryIds, cancellationToken)) ||
            (target.TeacherId.HasValue && await repository.TeacherSlotConflictAsync(entry.TimetableVersionId, target.TeacherId.Value, entry.WeekNumber, entry.DayOfWeek, entry.Session, entry.PeriodNumber, excludedEntryIds, cancellationToken)))
        {
            return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TimetableSlotConflict);
        }

        if (entry.WeekNumber != target.WeekNumber && entry.TeacherId != target.TeacherId)
        {
            if (entry.TeacherId.HasValue)
            {
                var capability = await repository.GetActiveTeacherCapabilityAsync(entry.TeacherId.Value, entry.SubjectId, cancellationToken);
                var targetWeekLoad = await repository.CountTeacherPeriodsAsync(entry.TimetableVersionId, entry.TeacherId.Value, target.WeekNumber, cancellationToken);
                if (capability is null || targetWeekLoad >= capability.MaxPeriodsPerWeek)
                {
                    return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TeacherLoadExceeded);
                }
            }

            if (target.TeacherId.HasValue)
            {
                var capability = await repository.GetActiveTeacherCapabilityAsync(target.TeacherId.Value, target.SubjectId, cancellationToken);
                var sourceWeekLoad = await repository.CountTeacherPeriodsAsync(entry.TimetableVersionId, target.TeacherId.Value, entry.WeekNumber, cancellationToken);
                if (capability is null || sourceWeekLoad >= capability.MaxPeriodsPerWeek)
                {
                    return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TeacherLoadExceeded);
                }
            }
        }

        var sourceEntries = await repository.ListTimetableEntriesAsync(entry.TimetableVersionId, entry.ClassRoomId, entry.WeekNumber, cancellationToken);
        var targetEntries = entry.WeekNumber == target.WeekNumber
            ? sourceEntries
            : await repository.ListTimetableEntriesAsync(entry.TimetableVersionId, entry.ClassRoomId, target.WeekNumber, cancellationToken);
        var affectedEntries = sourceEntries.Concat(targetEntries).DistinctBy(candidate => candidate.Id).ToList();
        var sourceQuota = await repository.GetQuotaForClassSubjectAsync(entry.ClassRoomId, entry.SubjectId, cancellationToken);
        var targetQuota = entry.SubjectId == target.SubjectId
            ? sourceQuota
            : await repository.GetQuotaForClassSubjectAsync(entry.ClassRoomId, target.SubjectId, cancellationToken);
        if (!IsValidAfterSwap(affectedEntries, entry, target, sourceQuota, targetQuota))
        {
            return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TimetableSlotConflict);
        }

        var sourceWeek = entry.WeekNumber;
        var sourceDay = entry.DayOfWeek;
        var sourceSession = entry.Session;
        var sourcePeriod = entry.PeriodNumber;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var temporaryWeek = -1 - Math.Abs(entry.Id.GetHashCode() % 1_000_000);
        entry.Move(temporaryWeek, sourceDay, sourceSession, sourcePeriod, now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        target.Move(sourceWeek, sourceDay, sourceSession, sourcePeriod, now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        entry.Move(request.WeekNumber, request.DayOfWeek, request.Session, request.PeriodNumber, now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToEntryResponse(entry));
    }

    /// <summary>
    /// Ghi chú: AssignClassSubjectTeacherAsync đổi giáo viên của toàn bộ môn trong lớp, đồng bộ assignment và mọi tuần của bản nháp.
    /// </summary>
    public async Task<Result<TimetableEntryResponse>> AssignClassSubjectTeacherAsync(AssignClassSubjectTeacherCommand request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator()) return Result.Failure<TimetableEntryResponse>(AdminRequired);
        var entry = await repository.GetTimetableEntryAsync(request.TimetableEntryId, cancellationToken);
        if (entry is null) return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TimetableEntryNotFound);
        if (entry.TimetableVersion.Status != TimetableVersionStatus.Draft)
        {
            return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TimetableNotDraft);
        }

        if (entry.Note == "Sinh hoạt lớp")
        {
            return Result.Failure<TimetableEntryResponse>(SchedulingErrors.HomeroomTeacherManagedSeparately);
        }

        var capability = await repository.GetActiveTeacherCapabilityAsync(request.TeacherId, entry.SubjectId, cancellationToken);
        if (capability is null) return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TeacherCapabilityRequired);
        if (await repository.IsHomeroomTeacherForClassAsync(request.TeacherId, entry.ClassRoomId, cancellationToken))
        {
            return Result.Failure<TimetableEntryResponse>(SchedulingErrors.HomeroomTeachingConflict);
        }

        var subjectEntries = await repository.ListTimetableEntriesForClassSubjectAsync(entry.TimetableVersionId, entry.ClassRoomId, entry.SubjectId, cancellationToken);
        if (subjectEntries.Count == 0 || subjectEntries.Any(candidate => candidate.IsLocked))
        {
            return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TimetableNotDraft);
        }

        var assignmentScope = await repository.ListActiveTeachingAssignmentsForScopeAsync(
            entry.ClassRoomId,
            entry.SubjectId,
            entry.TimetableVersion.SemesterId,
            cancellationToken);
        if (assignmentScope.Count != 1)
        {
            return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TeachingAssignmentInvalid);
        }

        if (assignmentScope[0].TeacherId == request.TeacherId && subjectEntries.All(candidate => candidate.TeacherId == request.TeacherId))
        {
            return Result.Success(ToEntryResponse(entry, capability.Teacher.FullName));
        }

        var subjectEntryIds = subjectEntries.Select(candidate => candidate.Id).ToHashSet();
        var otherTeacherEntries = (await repository.ListTeacherTimetableEntriesAsync(entry.TimetableVersionId, request.TeacherId, cancellationToken))
            .Where(candidate => !subjectEntryIds.Contains(candidate.Id))
            .ToList();
        var occupiedSlots = otherTeacherEntries
            .Select(candidate => (candidate.WeekNumber, candidate.DayOfWeek, candidate.Session, candidate.PeriodNumber))
            .ToHashSet();
        if (subjectEntries.Any(candidate => occupiedSlots.Contains((candidate.WeekNumber, candidate.DayOfWeek, candidate.Session, candidate.PeriodNumber))))
        {
            return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TimetableSlotConflict);
        }

        foreach (var week in subjectEntries.GroupBy(candidate => candidate.WeekNumber))
        {
            var totalWeekLoad = week.Count() + otherTeacherEntries.Count(candidate => candidate.WeekNumber == week.Key);
            if (totalWeekLoad > capability.MaxPeriodsPerWeek)
            {
                return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TeacherLoadExceeded);
            }

            foreach (var day in week.GroupBy(candidate => candidate.DayOfWeek))
            {
                var totalDayLoad = day.Count() + otherTeacherEntries.Count(candidate => candidate.WeekNumber == week.Key && candidate.DayOfWeek == day.Key);
                if (totalDayLoad > 5)
                {
                    return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TeacherLoadExceeded);
                }
            }
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        assignmentScope[0].ReassignTeacher(request.TeacherId, now);
        foreach (var subjectEntry in subjectEntries)
        {
            subjectEntry.AssignTeacher(request.TeacherId, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToEntryResponse(entry, capability.Teacher.FullName));
    }

    /// <summary>
    /// Ghi chú: SetTimetableEntryLockAsync khóa slot đã chỉnh tay hoặc mở khóa để có thể chỉnh tiếp.
    /// </summary>
    public async Task<Result<TimetableEntryResponse>> SetTimetableEntryLockAsync(SetTimetableEntryLockCommand request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator()) return Result.Failure<TimetableEntryResponse>(AdminRequired);
        var entry = await repository.GetTimetableEntryAsync(request.TimetableEntryId, cancellationToken);
        if (entry is null) return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TimetableEntryNotFound);
        if (entry.TimetableVersion.Status != TimetableVersionStatus.Draft) return Result.Failure<TimetableEntryResponse>(SchedulingErrors.TimetableNotDraft);
        entry.SetLocked(request.IsLocked, timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToEntryResponse(entry));
    }

    /// <summary>
    /// Ghi chú: AutoAssignHomerooms chọn giáo viên ít tải nhất, chưa chủ nhiệm và không dạy lớp cần phân công.
    /// </summary>
    private static List<HomeroomAssignment>? AutoAssignHomerooms(
        IReadOnlyList<ClassRoom> classes,
        IReadOnlyList<EduHub.Domain.Entities.Identity.User> teachers,
        IReadOnlyList<TeachingAssignment> teachingAssignments,
        IReadOnlyList<HomeroomAssignment> existing,
        IReadOnlyDictionary<Guid, int> loadByTeacher,
        DateTime now)
    {
        var result = new List<HomeroomAssignment>();
        var occupiedTeachers = existing.Select(assignment => assignment.TeacherId).ToHashSet();
        var occupiedClasses = existing.Select(assignment => assignment.ClassRoomId).ToHashSet();
        foreach (var classRoom in classes.Where(classRoom => !occupiedClasses.Contains(classRoom.Id)))
        {
            var teacher = teachers.Where(candidate =>
                    !occupiedTeachers.Contains(candidate.Id) &&
                    teachingAssignments.All(assignment => assignment.ClassRoomId != classRoom.Id || assignment.TeacherId != candidate.Id))
                .OrderBy(candidate => loadByTeacher.GetValueOrDefault(candidate.Id))
                .ThenBy(candidate => candidate.FullName)
                .FirstOrDefault();
            if (teacher is null) return null;
            result.Add(new HomeroomAssignment(classRoom.Id, teacher.Id, now));
            occupiedTeachers.Add(teacher.Id);
        }

        return result;
    }

    /// <summary>
    /// Ghi chú: AutoAssignTeachers chọn giáo viên đúng năng lực, ưu tiên môn chính và cân bằng tổng số tiết mỗi tuần.
    /// </summary>
    private static List<TeachingAssignment>? AutoAssignTeachers(
        Guid semesterId,
        IReadOnlyList<ClassRoom> classes,
        IReadOnlyList<CurriculumPlan> plans,
        IReadOnlyList<TeacherSubjectCapability> capabilities,
        IReadOnlyList<TeachingAssignment> existing,
        IReadOnlyList<HomeroomAssignment> homerooms,
        Dictionary<Guid, int> loadByTeacher,
        Semester semester,
        int weekCount,
        DateTime now)
    {
        var result = new List<TeachingAssignment>();
        foreach (var classRoom in classes)
        {
            var plan = plans.Single(candidate => candidate.GradeLevel == classRoom.GradeLevel);
            var homeroomTeacherId = homerooms.Single(assignment => assignment.ClassRoomId == classRoom.Id).TeacherId;
            foreach (var quota in plan.SubjectQuotas)
            {
                if (existing.Concat(result).Any(assignment => assignment.ClassRoomId == classRoom.Id && assignment.SubjectId == quota.SubjectId)) continue;
                Guid? teacherId;
                if (quota.IncludesHomeroom)
                {
                    teacherId = homeroomTeacherId;
                }
                else
                {
                    var weeklyPeriods = GetPeakWeeklyPeriods(quota, semester, weekCount);
                    var capability = capabilities.Where(candidate =>
                            candidate.SubjectId == quota.SubjectId &&
                            candidate.TeacherId != homeroomTeacherId &&
                            loadByTeacher.GetValueOrDefault(candidate.TeacherId) + weeklyPeriods <= candidate.MaxPeriodsPerWeek)
                        .OrderBy(candidate => candidate.Priority)
                        .ThenBy(candidate => loadByTeacher.GetValueOrDefault(candidate.TeacherId))
                        .ThenBy(candidate => candidate.Teacher.FullName)
                        .FirstOrDefault();
                    if (capability is null) return null;
                    teacherId = capability.TeacherId;
                }

                var assignment = new TeachingAssignment(classRoom.Id, quota.SubjectId, teacherId.Value, semesterId, now);
                result.Add(assignment);
                loadByTeacher[teacherId.Value] = loadByTeacher.GetValueOrDefault(teacherId.Value) + GetPeakWeeklyPeriods(quota, semester, weekCount);
            }
        }

        return result;
    }

    /// <summary>
    /// Ghi chú: BuildTeacherLoads tính tải dạy tuần hiện tại từ phân công đã có và quota đúng khối.
    /// </summary>
    private static Dictionary<Guid, int> BuildTeacherLoads(
        IReadOnlyList<TeachingAssignment> assignments,
        IReadOnlyList<ClassRoom> classes,
        IReadOnlyList<CurriculumPlan> plans,
        Semester semester,
        int weekCount)
    {
        var result = new Dictionary<Guid, int>();
        foreach (var assignment in assignments)
        {
            var classRoom = classes.FirstOrDefault(candidate => candidate.Id == assignment.ClassRoomId);
            var quota = classRoom is null
                ? null
                : plans.FirstOrDefault(plan => plan.GradeLevel == classRoom.GradeLevel)?.SubjectQuotas.FirstOrDefault(candidate => candidate.SubjectId == assignment.SubjectId);
            result[assignment.TeacherId] = result.GetValueOrDefault(assignment.TeacherId) + (quota is null ? 0 : GetPeakWeeklyPeriods(quota, semester, weekCount));
        }

        return result;
    }

    /// <summary>
    /// Ghi chú: BuildRequirements phân bổ quota môn của từng lớp thành yêu cầu đầu vào cho từng tuần thực học.
    /// </summary>
    private static List<TimetableGenerationRequirement>? BuildRequirements(
        IReadOnlyList<ClassRoom> classes,
        IReadOnlyList<CurriculumPlan> plans,
        IReadOnlyList<TeachingAssignment> assignments,
        IReadOnlyList<HomeroomAssignment> homerooms,
        Semester semester,
        int weekCount)
    {
        var result = new List<TimetableGenerationRequirement>();
        foreach (var classRoom in classes)
        {
            var plan = plans.Single(candidate => candidate.GradeLevel == classRoom.GradeLevel);
            var weeklyPeriods = BuildWeeklyPeriodPlan(plan, semester, weekCount);
            if (weeklyPeriods is null) return null;
            var homeroomTeacherId = homerooms.Single(assignment => assignment.ClassRoomId == classRoom.Id).TeacherId;
            foreach (var quota in plan.SubjectQuotas)
            {
                var teacherId = assignments.Single(assignment => assignment.ClassRoomId == classRoom.Id && assignment.SubjectId == quota.SubjectId).TeacherId;
                for (var weekNumber = 1; weekNumber <= weekCount; weekNumber++)
                {
                    result.Add(new TimetableGenerationRequirement(classRoom.Id, quota.SubjectId, teacherId, weekNumber, weeklyPeriods[quota.Id][weekNumber - 1], quota.CanDoublePeriod, quota.MaxPeriodsPerDay, quota.IncludesHomeroom, homeroomTeacherId, quota.PreferredSession));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Ghi chú: BuildWeeklyPeriodPlan chia quota thành tải tuần ổn định gồm 29 tiết sáng và các buổi chiều trọn 5 tiết.
    /// </summary>
    private static Dictionary<Guid, int[]>? BuildWeeklyPeriodPlan(CurriculumPlan plan, Semester semester, int weekCount)
    {
        var semesterTotal = plan.SubjectQuotas.Sum(quota => GetSemesterPeriods(quota, semester));
        if (semesterTotal % weekCount != 0) return null;
        var requiredPeriodsPerWeek = semesterTotal / weekCount;
        if (requiredPeriodsPerWeek is < 29 or > 54 || (requiredPeriodsPerWeek - 29) % 5 != 0) return null;
        var weeklyLoads = new int[weekCount];
        var result = new Dictionary<Guid, int[]>();
        foreach (var quota in plan.SubjectQuotas.OrderByDescending(quota => quota.IncludesHomeroom).ThenBy(quota => quota.Subject.SubjectCode))
        {
            var semesterPeriods = GetSemesterPeriods(quota, semester);
            var basePeriods = semesterPeriods / weekCount;
            var remainder = semesterPeriods % weekCount;
            var periods = Enumerable.Repeat(basePeriods, weekCount).ToArray();
            for (var index = 0; index < weekCount; index++) weeklyLoads[index] += basePeriods;

            var offset = quota.Subject.SubjectCode.Aggregate(0, (value, character) => value + character) % weekCount;
            for (var occurrence = 0; occurrence < remainder; occurrence++)
            {
                var targetWeek = Enumerable.Range(0, weekCount)
                    .Where(index => periods[index] == basePeriods)
                    .OrderBy(index => weeklyLoads[index])
                    .ThenBy(index => (index - offset + weekCount) % weekCount)
                    .First();
                periods[targetWeek]++;
                weeklyLoads[targetWeek]++;
            }

            result[quota.Id] = periods;
        }

        return weeklyLoads.All(load => load == requiredPeriodsPerWeek) ? result : null;
    }

    /// <summary>
    /// Ghi chú: IsValidAfterSwap mô phỏng hai tiết sau khi hoán đổi để giữ giới hạn và tính liền nhau của từng môn.
    /// </summary>
    private static bool IsValidAfterSwap(
        IReadOnlyList<TimetableEntryResponse> entries,
        TimetableEntry source,
        TimetableEntry target,
        CurriculumSubjectQuota? sourceQuota,
        CurriculumSubjectQuota? targetQuota)
    {
        var quotas = new Dictionary<Guid, CurriculumSubjectQuota?>
        {
            [source.SubjectId] = sourceQuota,
            [target.SubjectId] = targetQuota
        };
        var positions = entries.Select(candidate =>
        {
            if (candidate.Id == source.Id)
            {
                return new { candidate.SubjectId, WeekNumber = target.WeekNumber, DayOfWeek = target.DayOfWeek, Session = target.Session, PeriodNumber = target.PeriodNumber };
            }

            if (candidate.Id == target.Id)
            {
                return new { candidate.SubjectId, WeekNumber = source.WeekNumber, DayOfWeek = source.DayOfWeek, Session = source.Session, PeriodNumber = source.PeriodNumber };
            }

            return new
            {
                candidate.SubjectId,
                candidate.WeekNumber,
                candidate.DayOfWeek,
                Session = Enum.Parse<TimetableSession>(candidate.Session),
                candidate.PeriodNumber
            };
        }).ToList();

        foreach (var subject in quotas)
        {
            var quota = subject.Value;
            foreach (var day in positions.Where(position => position.SubjectId == subject.Key)
                         .GroupBy(position => new { position.WeekNumber, position.DayOfWeek }))
            {
                var periods = day.OrderBy(position => position.PeriodNumber).ToList();
                var maxPerDay = quota?.MaxPeriodsPerDay ?? 2;
                if (periods.Count > maxPerDay) return false;
                if (periods.Count == 2 && (quota?.CanDoublePeriod != true || periods[0].Session != periods[1].Session || periods[1].PeriodNumber - periods[0].PeriodNumber != 1))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static int GetSemesterWeekCount(Semester semester, IReadOnlyList<CurriculumPlan> plans) =>
        IsSecondSemester(semester) ? plans[0].Semester2Weeks : plans[0].Semester1Weeks;

    private static int GetSemesterPeriods(CurriculumSubjectQuota quota, Semester semester) =>
        IsSecondSemester(semester) ? quota.Semester2Periods : quota.Semester1Periods;

    private static int GetPeakWeeklyPeriods(CurriculumSubjectQuota quota, Semester semester, int weekCount) =>
        (int)Math.Ceiling((double)GetSemesterPeriods(quota, semester) / weekCount);

    private static bool IsSecondSemester(Semester semester) =>
        semester.NormalizedName.Contains('2') || semester.Name.Contains('2');

    private bool IsAcademicAdministrator() => currentUser.Role is UserRole.AcademicAdmin or UserRole.SystemAdmin;

    private static bool IsValidSlot(int dayOfWeek, TimetableSession session, int periodNumber) =>
        dayOfWeek is >= 1 and <= 6 &&
        (session == TimetableSession.Morning
            ? periodNumber is >= 1 and <= 5 && !(dayOfWeek == 3 && periodNumber == 5)
            : dayOfWeek <= 5 && periodNumber is >= 1 and <= 5);

    /// <summary>
    /// Ghi chú: ToCurriculumResponse chuyển chương trình vừa tạo thành response kèm tên các môn.
    /// </summary>
    private static CurriculumPlanResponse ToCurriculumResponse(CurriculumPlan plan, IReadOnlyList<Subject> subjects)
    {
        var subjectMap = subjects.ToDictionary(subject => subject.Id);
        return new CurriculumPlanResponse(
            plan.Id,
            plan.AcademicYearId,
            plan.GradeLevel,
            plan.Name,
            plan.TotalWeeks,
            plan.Semester1Weeks,
            plan.Semester2Weeks,
            plan.SubjectQuotas.Sum(quota => quota.AnnualPeriods),
            plan.IsActive,
            plan.SubjectQuotas.Select(quota => new CurriculumSubjectQuotaResponse(
                quota.Id,
                quota.SubjectId,
                subjectMap[quota.SubjectId].SubjectCode,
                subjectMap[quota.SubjectId].Name,
                quota.Kind.ToString(),
                quota.AnnualPeriods,
                quota.Semester1Periods,
                quota.Semester2Periods,
                quota.CanDoublePeriod,
                quota.MaxPeriodsPerDay,
                quota.IncludesHomeroom,
                quota.PreferredSession?.ToString())).ToList());
    }

    private static TimetableVersionResponse ToVersionResponse(TimetableVersion version, string semesterName) =>
        new(version.Id, version.SemesterId, semesterName, version.Name, version.Status.ToString(), version.GeneratedAtUtc, version.PublishedAtUtc, version.Entries.Count);

    private static TimetableEntryResponse ToEntryResponse(TimetableEntry entry, string? teacherNameOverride = null)
    {
        var week = TimetableCalendar.GetWeekDates(entry.TimetableVersion.Semester.StartDate, entry.WeekNumber);
        var period = TimetableCalendar.GetPeriodTimes(entry.Session, entry.PeriodNumber);
        return new(
            entry.Id,
            entry.TimetableVersionId,
            entry.ClassRoomId,
            entry.ClassRoom.ClassCode,
            entry.ClassRoom.Name,
            entry.SubjectId,
            entry.Subject.SubjectCode,
            entry.Subject.Name,
            entry.TeacherId,
            teacherNameOverride ?? entry.Teacher?.FullName,
            entry.WeekNumber,
            week.StartDate,
            week.EndDate,
            entry.DayOfWeek,
            entry.Session.ToString(),
            entry.PeriodNumber,
            period.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            period.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            entry.Kind.ToString(),
            entry.CountsTowardQuota,
            entry.IsLocked,
            entry.Note);
    }
}
