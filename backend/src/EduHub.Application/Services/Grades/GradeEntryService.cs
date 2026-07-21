using System.Text.Json;
using EduHub.Application.Common.Caching;
using EduHub.Application.Common.Errors;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Grades;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.Grades;
using EduHub.Application.Interfaces.Services.Caching;
using EduHub.Application.Interfaces.Services.Grades;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Enums;

namespace EduHub.Application.Services.Grades;

/// <summary>
/// Ghi chú: GradeEntryService xử lý nhập điểm, bulk điểm và state machine Draft/Submitted/Published/Locked.
/// </summary>
public sealed class GradeEntryService(
    IGradeEntryRepository gradeEntryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ICacheService cacheService,
    ICacheKeyPolicy cacheKeyPolicy,
    TimeProvider timeProvider)
    : IGradeEntryService
{
    private const int MaxScoreDecimalPlaces = 2;

    /// <summary>
    /// Ghi chú: UpdateGradeAsync tạo hoặc sửa điểm Draft cho một học sinh trong assignment-component.
    /// </summary>
    public async Task<Result<GradeEntryResponse>> UpdateGradeAsync(UpdateGradeCommand request, CancellationToken cancellationToken)
    {
        var validation = await ValidateUpdateScopeAsync(
            request.StudentId,
            request.AssignmentId,
            request.ComponentId,
            request.Score,
            cancellationToken);
        if (validation.IsFailure)
        {
            return Result.Failure<GradeEntryResponse>(validation.Error!);
        }

        var actorUserId = currentUser.UserId!.Value;
        var gradeEntry = await gradeEntryRepository.GetGradeEntryAsync(
            request.StudentId,
            request.AssignmentId,
            request.ComponentId,
            cancellationToken);

        if (gradeEntry is null)
        {
            gradeEntry = new GradeEntry(
                request.StudentId,
                request.AssignmentId,
                request.ComponentId,
                request.Score,
                actorUserId,
                timeProvider.GetUtcNow().UtcDateTime);
            gradeEntryRepository.AddGradeEntry(gradeEntry);
        }
        else
        {
            if (!request.Version.HasValue)
            {
                return Result.Failure<GradeEntryResponse>(GradeErrors.VersionRequired);
            }

            try
            {
                gradeEntry.UpdateScore(
                    request.Score,
                    request.Version.Value,
                    actorUserId,
                    request.Reason,
                    timeProvider.GetUtcNow().UtcDateTime);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("stale", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<GradeEntryResponse>(GradeErrors.StaleVersion);
            }
            catch (InvalidOperationException)
            {
                return Result.Failure<GradeEntryResponse>(GradeErrors.InvalidGradeState);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToResponse(gradeEntry));
    }

    /// <summary>
    /// Ghi chú: BulkUpdateGradesAsync nhập nhiều điểm cho một assignment theo atomic hoặc partial mode.
    /// </summary>
    public async Task<Result<BulkUpdateGradesResponse>> BulkUpdateGradesAsync(
        BulkUpdateGradesCommand request,
        CancellationToken cancellationToken)
    {
        var items = new List<BulkUpdateGradeItemResponse>();
        foreach (var item in request.Items)
        {
            var result = await UpdateGradeAsync(
                new UpdateGradeCommand(item.StudentId, request.AssignmentId, item.ComponentId, item.Score, item.Version, item.Reason),
                cancellationToken);

            if (result.IsSuccess)
            {
                items.Add(new BulkUpdateGradeItemResponse(item.StudentId, item.ComponentId, true, result.Value, null, null));
                continue;
            }

            if (request.Atomic)
            {
                return Result.Failure<BulkUpdateGradesResponse>(result.Error!);
            }

            items.Add(new BulkUpdateGradeItemResponse(
                item.StudentId,
                item.ComponentId,
                false,
                null,
                result.Error!.Code,
                result.Error.Message));
        }

        var successCount = items.Count(item => item.Success);
        return Result.Success(new BulkUpdateGradesResponse(items, successCount, items.Count - successCount));
    }

    /// <summary>
    /// Ghi chú: SubmitGradebookAsync chuyển toàn bộ điểm Draft của assignment sang Submitted nếu đủ required component.
    /// </summary>
    public async Task<Result<GradebookStateResponse>> SubmitGradebookAsync(SubmitGradebookCommand request, CancellationToken cancellationToken)
    {
        var assignment = await ValidateTeacherOwnsAssignmentAsync(request.AssignmentId, cancellationToken);
        if (assignment.IsFailure)
        {
            return Result.Failure<GradebookStateResponse>(assignment.Error!);
        }

        var entries = await gradeEntryRepository.GetGradeEntriesByAssignmentAsync(request.AssignmentId, cancellationToken);
        if (entries.Count == 0)
        {
            return Result.Failure<GradebookStateResponse>(GradeErrors.NoGrades);
        }

        var requiredValidation = await ValidateRequiredGradesAsync(assignment.Value, entries, cancellationToken);
        if (requiredValidation.IsFailure)
        {
            return Result.Failure<GradebookStateResponse>(requiredValidation.Error!);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var entry in entries.Where(entry => entry.Status == GradeStatus.Draft))
        {
            entry.Submit(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new GradebookStateResponse(request.AssignmentId, GradeStatus.Submitted.ToString(), entries.Count, entries.Max(e => e.PublicationVersion)));
    }

    /// <summary>
    /// Ghi chú: PublishGradebookAsync chuyển điểm Submitted sang Published và ghi outbox event.
    /// </summary>
    public Task<Result<GradebookStateResponse>> PublishGradebookAsync(PublishGradebookCommand request, CancellationToken cancellationToken) =>
        ChangeGradebookStateAsync(request.AssignmentId, GradeStatus.Submitted, GradeStatus.Published, null, "GradebookPublished", cancellationToken);

    /// <summary>
    /// Ghi chú: ReopenGradebookAsync mở lại điểm Submitted/Published/Locked về Draft với reason bắt buộc.
    /// </summary>
    public async Task<Result<GradebookStateResponse>> ReopenGradebookAsync(ReopenGradebookCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Result.Failure<GradebookStateResponse>(GradeErrors.ReasonRequired);
        }

        return await ChangeGradebookStateAsync(
            request.AssignmentId,
            null,
            GradeStatus.Draft,
            request.Reason,
            "GradebookReopened",
            cancellationToken);
    }

    /// <summary>
    /// Ghi chú: LockGradebookAsync chuyển điểm Published sang Locked.
    /// </summary>
    public Task<Result<GradebookStateResponse>> LockGradebookAsync(LockGradebookCommand request, CancellationToken cancellationToken) =>
        ChangeGradebookStateAsync(request.AssignmentId, GradeStatus.Published, GradeStatus.Locked, null, "GradebookLocked", cancellationToken);

    /// <summary>
    /// Ghi chú: GetGradebookAsync trả bounded read model cho giáo viên được phân công hoặc quản trị học vụ.
    /// </summary>
    public async Task<Result<GradebookResponse>> GetGradebookAsync(GetGradebookQuery request, CancellationToken cancellationToken)
    {
        var assignment = await gradeEntryRepository.GetAssignmentAsync(request.AssignmentId, cancellationToken);
        if (assignment is null) return Result.Failure<GradebookResponse>(GradeErrors.AssignmentNotFound);
        var canRead = currentUser.Role is UserRole.AcademicAdmin or UserRole.SystemAdmin || (currentUser.Role == UserRole.Teacher && currentUser.UserId == assignment.TeacherId);
        if (!canRead) return Result.Failure<GradebookResponse>(GradeErrors.TeacherForbidden);
        var gradebook = await gradeEntryRepository.GetGradebookAsync(request.AssignmentId, cancellationToken);
        return gradebook is null ? Result.Failure<GradebookResponse>(GradeErrors.AssignmentNotFound) : Result.Success(gradebook);
    }

    /// <summary>
    /// Ghi chú: UpdateStudentRemarkAsync tạo hoặc sửa nhận xét Draft cho học sinh thuộc lớp của giáo viên.
    /// </summary>
    public async Task<Result<StudentRemarkResponse>> UpdateStudentRemarkAsync(UpdateStudentRemarkCommand request, CancellationToken cancellationToken)
    {
        var assignment = await ValidateTeacherOwnsAssignmentAsync(request.AssignmentId, cancellationToken);
        if (assignment.IsFailure) return Result.Failure<StudentRemarkResponse>(assignment.Error!);
        if (!await gradeEntryRepository.StudentIsEnrolledAsync(request.StudentId, assignment.Value.ClassRoomId, assignment.Value.SemesterId, cancellationToken))
            return Result.Failure<StudentRemarkResponse>(GradeErrors.StudentNotEnrolled);

        var remark = await gradeEntryRepository.GetStudentRemarkAsync(request.AssignmentId, request.StudentId, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (remark is null)
        {
            remark = new StudentRemark(request.StudentId, request.AssignmentId, currentUser.UserId!.Value, request.Content, now);
            gradeEntryRepository.AddStudentRemark(remark);
        }
        else
        {
            if (!request.Version.HasValue) return Result.Failure<StudentRemarkResponse>(GradeErrors.VersionRequired);
            try { remark.UpdateContent(request.Content, request.Version.Value, now); }
            catch (InvalidOperationException) { return Result.Failure<StudentRemarkResponse>(GradeErrors.StaleVersion); }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new StudentRemarkResponse(remark.Id, remark.AssignmentId, remark.StudentId, remark.Content, remark.Version, remark.IsPublished));
    }

    /// <summary>
    /// Ghi chú: GetPublishedGradesForParentAsync trả điểm Published/Locked của học sinh nếu user hiện tại là phụ huynh được liên kết.
    /// </summary>
    public async Task<Result<PublishedGradebookResponse>> GetPublishedGradesForParentAsync(
        GetPublishedGradesForParentQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.Parent || currentUser.UserId is null)
        {
            return Result.Failure<PublishedGradebookResponse>(GradeErrors.ParentForbidden);
        }

        var parentUserId = currentUser.UserId.Value;
        var canRead = await gradeEntryRepository.ParentCanReadStudentAsync(parentUserId, request.StudentId, cancellationToken);
        if (!canRead)
        {
            return Result.Failure<PublishedGradebookResponse>(GradeErrors.ParentForbidden);
        }

        var scope = cacheKeyPolicy.PublishedGradesScope(request.AssignmentId);
        var version = await cacheService.GetVersionAsync(scope, cancellationToken);
        var cacheKey = cacheKeyPolicy.PublishedGrades(version, parentUserId, request.StudentId, request.AssignmentId);

        return Result.Success(await cacheService.GetOrCreateAsync(
            cacheKey,
            async token => await gradeEntryRepository.GetPublishedGradebookAsync(request.StudentId, request.AssignmentId, token)
                ?? new PublishedGradebookResponse(request.StudentId, string.Empty, string.Empty, request.AssignmentId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, null, []),
            new CacheEntryOptions(TimeSpan.FromMinutes(5)),
            cancellationToken));
    }

    private async Task<Result<TeachingAssignment>> ValidateUpdateScopeAsync(
        Guid studentId,
        Guid assignmentId,
        Guid componentId,
        decimal score,
        CancellationToken cancellationToken)
    {
        var assignmentResult = await ValidateTeacherOwnsAssignmentAsync(assignmentId, cancellationToken);
        if (assignmentResult.IsFailure)
        {
            return assignmentResult;
        }

        var assignment = assignmentResult.Value;
        var component = await gradeEntryRepository.GetActiveComponentAsync(componentId, cancellationToken);
        if (component is null ||
            component.SubjectId != assignment.SubjectId ||
            component.SemesterId != assignment.SemesterId)
        {
            return Result.Failure<TeachingAssignment>(GradeErrors.ComponentInvalid);
        }

        if (score < 0m || score > component.MaxScore)
        {
            return Result.Failure<TeachingAssignment>(GradeErrors.ScoreOutOfRange);
        }

        if (DecimalPlaces(score) > MaxScoreDecimalPlaces)
        {
            return Result.Failure<TeachingAssignment>(GradeErrors.ScorePrecisionInvalid);
        }

        var enrolled = await gradeEntryRepository.StudentIsEnrolledAsync(
            studentId,
            assignment.ClassRoomId,
            assignment.SemesterId,
            cancellationToken);
        return enrolled
            ? Result.Success(assignment)
            : Result.Failure<TeachingAssignment>(GradeErrors.StudentNotEnrolled);
    }

    private async Task<Result<TeachingAssignment>> ValidateTeacherOwnsAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken)
    {
        var assignment = await gradeEntryRepository.GetAssignmentAsync(assignmentId, cancellationToken);
        if (assignment is null)
        {
            return Result.Failure<TeachingAssignment>(GradeErrors.AssignmentNotFound);
        }

        if (currentUser.Role != UserRole.Teacher || currentUser.UserId != assignment.TeacherId)
        {
            return Result.Failure<TeachingAssignment>(GradeErrors.TeacherForbidden);
        }

        var schoolToday = GetSchoolToday();
        if (schoolToday < assignment.Semester.GradeEntryFrom || schoolToday > assignment.Semester.GradeEntryTo)
        {
            return Result.Failure<TeachingAssignment>(GradeErrors.EntryWindowClosed);
        }

        return Result.Success(assignment);
    }

    /// <summary>
    /// Ghi chu: GetSchoolToday chuyen UTC clock sang ngay Viet Nam de doi chieu cua so nhap diem cua hoc ky.
    /// </summary>
    private DateOnly GetSchoolToday()
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone).DateTime);
    }

    private async Task<Result> ValidateRequiredGradesAsync(
        TeachingAssignment assignment,
        IReadOnlyList<GradeEntry> entries,
        CancellationToken cancellationToken)
    {
        var components = await gradeEntryRepository.GetActiveComponentsAsync(assignment.SubjectId, assignment.SemesterId, cancellationToken);
        if (components.Sum(component => component.Weight) != 1.00m)
        {
            return Result.Failure(GradeErrors.InvalidComponentWeights);
        }

        var requiredComponentIds = components.Where(component => component.IsRequired).Select(component => component.Id).ToHashSet();
        var studentIds = await gradeEntryRepository.GetActiveStudentIdsAsync(assignment.ClassRoomId, assignment.SemesterId, cancellationToken);
        foreach (var studentId in studentIds)
        {
            foreach (var componentId in requiredComponentIds)
            {
                if (!entries.Any(entry => entry.StudentId == studentId && entry.ComponentId == componentId))
                {
                    return Result.Failure(GradeErrors.MissingRequiredGrades);
                }
            }
        }

        return Result.Success();
    }

    private async Task<Result<GradebookStateResponse>> ChangeGradebookStateAsync(
        Guid assignmentId,
        GradeStatus? requiredStatus,
        GradeStatus targetStatus,
        string? reason,
        string outboxType,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role is not (UserRole.AcademicAdmin or UserRole.SystemAdmin))
        {
            return Result.Failure<GradebookStateResponse>(new Error("Grade.AdminRequired", "AcademicAdmin role is required.", ErrorType.Forbidden));
        }

        var entries = await gradeEntryRepository.GetGradeEntriesByAssignmentAsync(assignmentId, cancellationToken);
        if (entries.Count == 0)
        {
            return Result.Failure<GradebookStateResponse>(GradeErrors.NoGrades);
        }

        if (requiredStatus.HasValue && entries.Any(entry => entry.Status != requiredStatus.Value))
        {
            return Result.Failure<GradebookStateResponse>(GradeErrors.InvalidGradeState);
        }

        if (!requiredStatus.HasValue && entries.All(entry => entry.Status == GradeStatus.Draft))
        {
            return Result.Failure<GradebookStateResponse>(GradeErrors.InvalidGradeState);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var remarks = await gradeEntryRepository.GetRemarksByAssignmentAsync(assignmentId, cancellationToken);
        foreach (var entry in entries)
        {
            try
            {
                if (targetStatus == GradeStatus.Published)
                {
                    entry.Publish(now);
                }
                else if (targetStatus == GradeStatus.Draft)
                {
                    entry.Reopen(reason!, now);
                }
                else if (targetStatus == GradeStatus.Locked)
                {
                    entry.Lock(now);
                }
            }
            catch (InvalidOperationException)
            {
                return Result.Failure<GradebookStateResponse>(GradeErrors.InvalidGradeState);
            }
        }

        foreach (var remark in remarks)
        {
            if (targetStatus == GradeStatus.Published && !remark.IsPublished) remark.Publish(now);
            else if (targetStatus == GradeStatus.Draft && remark.IsPublished) remark.Reopen(now);
        }

        var publicationVersion = entries.Max(entry => entry.PublicationVersion);
        gradeEntryRepository.AddOutboxMessage(new OutboxMessage(
            outboxType,
            JsonSerializer.Serialize(new { assignmentId, status = targetStatus.ToString(), publicationVersion }),
            now));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new GradebookStateResponse(assignmentId, targetStatus.ToString(), entries.Count, publicationVersion));
    }

    private static GradeEntryResponse ToResponse(GradeEntry entry) =>
        new(entry.Id, entry.StudentId, entry.AssignmentId, entry.ComponentId, entry.Score, entry.Status.ToString(), entry.Version, entry.PublicationVersion);

    private static int DecimalPlaces(decimal value)
    {
        var bits = decimal.GetBits(value);
        return (bits[3] >> 16) & 0x7F;
    }
}
