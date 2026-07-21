using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Classes;
using EduHub.Application.Features.Classes.Common;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.Classes;
using EduHub.Application.Interfaces.Services.Classes;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Enums;

namespace EduHub.Application.Services.Classes;

/// <summary>
/// Ghi chú: ClassService xử lý nghiệp vụ lớp học, phân công giáo viên và ghi danh học sinh.
/// </summary>
public sealed class ClassService(
    IClassRepository classRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IClassService
{
    /// <summary>
    /// Ghi chú: ListMyTeachingAssignmentsAsync chỉ trả phân công thuộc giáo viên đang đăng nhập.
    /// </summary>
    public async Task<Result<IReadOnlyList<TeachingAssignmentSummaryResponse>>> ListMyTeachingAssignmentsAsync(ListMyTeachingAssignmentsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.Teacher || currentUser.UserId is null)
        {
            return Result.Failure<IReadOnlyList<TeachingAssignmentSummaryResponse>>(new Error("Assignment.TeacherRequired", "Teacher role is required.", ErrorType.Forbidden));
        }

        return Result.Success(await classRepository.ListTeachingAssignmentsAsync(currentUser.UserId.Value, null, request.SemesterId, cancellationToken));
    }

    /// <summary>
    /// Ghi chú: ListTeachingAssignmentsAsync cho quản trị học vụ đọc các phân công giáo viên-lớp-môn-học kỳ.
    /// </summary>
    public async Task<Result<IReadOnlyList<TeachingAssignmentSummaryResponse>>> ListTeachingAssignmentsAsync(ListTeachingAssignmentsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.Role is not (UserRole.AcademicAdmin or UserRole.SystemAdmin))
        {
            return Result.Failure<IReadOnlyList<TeachingAssignmentSummaryResponse>>(new Error("Assignment.AdminRequired", "Academic administrator role is required.", ErrorType.Forbidden));
        }

        return Result.Success(await classRepository.ListTeachingAssignmentsAsync(request.TeacherId, request.ClassRoomId, request.SemesterId, cancellationToken));
    }

    /// <summary>
    /// Ghi chú: CreateClassRoomAsync tạo lớp học mới nếu năm học tồn tại và mã lớp chưa trùng.
    /// </summary>
    public async Task<Result<ClassRoomResponse>> CreateClassRoomAsync(
        CreateClassRoomCommand request,
        CancellationToken cancellationToken)
    {
        var academicYearExists = await classRepository.AcademicYearExistsAsync(request.AcademicYearId, cancellationToken);
        if (!academicYearExists)
        {
            return Result.Failure<ClassRoomResponse>(ClassErrors.AcademicYearNotFound);
        }

        var normalizedCode = ClassNormalization.Code(request.ClassCode);
        var exists = await classRepository.ClassCodeExistsAsync(
            request.AcademicYearId,
            normalizedCode,
            cancellationToken);

        if (exists)
        {
            return Result.Failure<ClassRoomResponse>(ClassErrors.ClassCodeExists);
        }

        var classRoom = new ClassRoom(
            request.ClassCode,
            normalizedCode,
            request.Name,
            request.AcademicYearId,
            request.GradeLevel,
            request.Capacity);

        classRepository.AddClassRoom(classRoom);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(classRoom));
    }

    /// <summary>
    /// Ghi chú: UpdateClassRoomAsync cập nhật lớp học nếu capacity không thấp hơn sĩ số active.
    /// </summary>
    public async Task<Result<ClassRoomResponse>> UpdateClassRoomAsync(
        UpdateClassRoomCommand request,
        CancellationToken cancellationToken)
    {
        var classRoom = await classRepository.GetClassRoomAsync(request.Id, cancellationToken);
        if (classRoom is null)
        {
            return Result.Failure<ClassRoomResponse>(ClassErrors.ClassNotFound);
        }

        if (request.Capacity < classRoom.ActiveEnrollmentCount)
        {
            return Result.Failure<ClassRoomResponse>(ClassErrors.ClassCapacityExceeded);
        }

        classRoom.Update(request.Name, request.GradeLevel, request.Capacity, timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(classRoom));
    }

    /// <summary>
    /// Ghi chú: ListClassRoomsAsync đọc danh sách lớp học active theo năm học, tìm kiếm và phân trang.
    /// </summary>
    public async Task<Result<PagedResult<ClassRoomResponse>>> ListClassRoomsAsync(
        ListClassRoomsQuery request,
        CancellationToken cancellationToken)
    {
        var pageRequest = PageRequest.Create(request.Page, request.PageSize, request.Search);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<PagedResult<ClassRoomResponse>>(pageRequest.Error!);
        }

        return Result.Success(await classRepository.ListClassRoomsAsync(
            request.AcademicYearId,
            pageRequest.Value,
            cancellationToken));
    }

    /// <summary>
    /// Ghi chú: AssignTeacherAsync tạo phân công giáo viên active cho lớp-môn-học kỳ hợp lệ.
    /// </summary>
    public async Task<Result<TeachingAssignmentResponse>> AssignTeacherAsync(
        AssignTeacherCommand request,
        CancellationToken cancellationToken)
    {
        var classRoom = await classRepository.GetActiveClassRoomAsync(request.ClassRoomId, cancellationToken);
        if (classRoom is null)
        {
            return Result.Failure<TeachingAssignmentResponse>(ClassErrors.ClassNotFound);
        }

        var semester = await classRepository.GetSemesterAsync(request.SemesterId, cancellationToken);
        if (semester is null)
        {
            return Result.Failure<TeachingAssignmentResponse>(ClassErrors.SemesterNotFound);
        }

        if (semester.AcademicYearId != classRoom.AcademicYearId)
        {
            return Result.Failure<TeachingAssignmentResponse>(ClassErrors.AssignmentInvalidScope);
        }

        var subjectIsActive = await classRepository.ActiveSubjectExistsAsync(request.SubjectId, cancellationToken);
        if (!subjectIsActive)
        {
            return Result.Failure<TeachingAssignmentResponse>(ClassErrors.SubjectInvalid);
        }

        var teacherIsActive = await classRepository.ActiveTeacherExistsAsync(request.TeacherId, cancellationToken);
        if (!teacherIsActive)
        {
            return Result.Failure<TeachingAssignmentResponse>(ClassErrors.TeacherInvalid);
        }

        if (!await classRepository.ActiveTeacherCapabilityExistsAsync(request.TeacherId, request.SubjectId, cancellationToken))
        {
            return Result.Failure<TeachingAssignmentResponse>(ClassErrors.TeacherCapabilityRequired);
        }

        if (await classRepository.IsHomeroomTeacherForClassAsync(request.TeacherId, request.ClassRoomId, cancellationToken))
        {
            return Result.Failure<TeachingAssignmentResponse>(ClassErrors.HomeroomTeachingConflict);
        }

        var exists = await classRepository.ActiveTeachingAssignmentExistsAsync(
            request.ClassRoomId,
            request.SubjectId,
            request.SemesterId,
            cancellationToken);
        if (exists)
        {
            return Result.Failure<TeachingAssignmentResponse>(ClassErrors.AssignmentExists);
        }

        var assignment = new TeachingAssignment(
            request.ClassRoomId,
            request.SubjectId,
            request.TeacherId,
            request.SemesterId,
            timeProvider.GetUtcNow().UtcDateTime);

        classRepository.AddTeachingAssignment(assignment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(assignment));
    }

    /// <summary>
    /// Ghi chú: EnrollStudentAsync ghi danh một học sinh active vào lớp và reserve sĩ số atomically.
    /// </summary>
    public async Task<Result<EnrollmentResponse>> EnrollStudentAsync(
        EnrollStudentCommand request,
        CancellationToken cancellationToken)
    {
        var validation = await ValidateEnrollmentScopeAsync(
            request.StudentId,
            request.ClassRoomId,
            request.SemesterId,
            allowExistingActiveEnrollment: false,
            cancellationToken);
        if (validation.IsFailure)
        {
            return Result.Failure<EnrollmentResponse>(validation.Error!);
        }

        var capacityResult = await TryReserveSeatAsync(request.ClassRoomId, cancellationToken);
        if (capacityResult.IsFailure)
        {
            return Result.Failure<EnrollmentResponse>(capacityResult.Error!);
        }

        var enrollment = new Enrollment(
            request.StudentId,
            request.ClassRoomId,
            request.SemesterId,
            timeProvider.GetUtcNow().UtcDateTime);

        classRepository.AddEnrollment(enrollment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(enrollment));
    }

    /// <summary>
    /// Ghi chú: BulkEnrollStudentsAsync ghi danh hàng loạt học sinh theo partial-success từng dòng.
    /// </summary>
    public async Task<Result<BulkEnrollmentResponse>> BulkEnrollStudentsAsync(
        BulkEnrollStudentsCommand request,
        CancellationToken cancellationToken)
    {
        var results = new List<BulkEnrollmentItemResult>(request.StudentIds.Count);
        var seenStudentIds = new HashSet<Guid>();

        foreach (var studentId in request.StudentIds)
        {
            if (!seenStudentIds.Add(studentId))
            {
                results.Add(ToFailedItem(studentId, ClassErrors.DuplicateBulkStudent));
                continue;
            }

            await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            var validation = await ValidateEnrollmentScopeAsync(
                studentId,
                request.ClassRoomId,
                request.SemesterId,
                allowExistingActiveEnrollment: false,
                cancellationToken);
            if (validation.IsFailure)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                results.Add(ToFailedItem(studentId, validation.Error!));
                continue;
            }

            var capacityResult = await TryReserveSeatAsync(request.ClassRoomId, cancellationToken);
            if (capacityResult.IsFailure)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                results.Add(ToFailedItem(studentId, capacityResult.Error!));
                continue;
            }

            var enrollment = new Enrollment(
                studentId,
                request.ClassRoomId,
                request.SemesterId,
                timeProvider.GetUtcNow().UtcDateTime);

            classRepository.AddEnrollment(enrollment);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            results.Add(new BulkEnrollmentItemResult(studentId, true, enrollment.Id, null, null));
        }

        var successCount = results.Count(result => result.Success);
        return Result.Success(new BulkEnrollmentResponse(results, successCount, results.Count - successCount));
    }

    /// <summary>
    /// Ghi chú: TransferEnrollmentAsync đóng enrollment lớp cũ và tạo enrollment lớp mới trong một transaction.
    /// </summary>
    public async Task<Result<EnrollmentResponse>> TransferEnrollmentAsync(
        TransferEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        var currentEnrollment = await classRepository.GetActiveEnrollmentAsync(
            request.StudentId,
            request.FromClassRoomId,
            request.SemesterId,
            cancellationToken);
        if (currentEnrollment is null)
        {
            return Result.Failure<EnrollmentResponse>(ClassErrors.EnrollmentNotFound);
        }

        var validation = await ValidateEnrollmentScopeAsync(
            request.StudentId,
            request.ToClassRoomId,
            request.SemesterId,
            allowExistingActiveEnrollment: true,
            cancellationToken);
        if (validation.IsFailure)
        {
            return Result.Failure<EnrollmentResponse>(validation.Error!);
        }

        var capacityResult = await TryReserveSeatAsync(request.ToClassRoomId, cancellationToken);
        if (capacityResult.IsFailure)
        {
            return Result.Failure<EnrollmentResponse>(capacityResult.Error!);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        currentEnrollment.Withdraw(request.Reason, now);
        await classRepository.ReleaseSeatAsync(request.FromClassRoomId, cancellationToken);

        var newEnrollment = new Enrollment(request.StudentId, request.ToClassRoomId, request.SemesterId, now);
        classRepository.AddEnrollment(newEnrollment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(newEnrollment));
    }

    /// <summary>
    /// Ghi chú: WithdrawEnrollmentAsync đóng enrollment active và giảm sĩ số active của lớp.
    /// </summary>
    public async Task<Result> WithdrawEnrollmentAsync(WithdrawEnrollmentCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await classRepository.GetActiveEnrollmentAsync(
            request.StudentId,
            request.ClassRoomId,
            request.SemesterId,
            cancellationToken);
        if (enrollment is null)
        {
            return Result.Failure(ClassErrors.EnrollmentNotFound);
        }

        enrollment.Withdraw(request.Reason, timeProvider.GetUtcNow().UtcDateTime);
        await classRepository.ReleaseSeatAsync(request.ClassRoomId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Ghi chú: ValidateEnrollmentScopeAsync kiểm tra học sinh, lớp, học kỳ và duplicate enrollment.
    /// </summary>
    private async Task<Result> ValidateEnrollmentScopeAsync(
        Guid studentId,
        Guid classRoomId,
        Guid semesterId,
        bool allowExistingActiveEnrollment,
        CancellationToken cancellationToken)
    {
        var studentIsActive = await classRepository.ActiveStudentExistsAsync(studentId, cancellationToken);
        if (!studentIsActive)
        {
            return Result.Failure(ClassErrors.StudentInvalid);
        }

        var classRoom = await classRepository.GetActiveClassRoomAsync(classRoomId, cancellationToken);
        if (classRoom is null)
        {
            return Result.Failure(ClassErrors.ClassNotFound);
        }

        var semester = await classRepository.GetSemesterAsync(semesterId, cancellationToken);
        if (semester is null)
        {
            return Result.Failure(ClassErrors.SemesterNotFound);
        }

        if (semester.AcademicYearId != classRoom.AcademicYearId)
        {
            return Result.Failure(ClassErrors.AssignmentInvalidScope);
        }

        var hasActiveEnrollment = await classRepository.ActiveEnrollmentExistsAsync(
            studentId,
            semesterId,
            cancellationToken);
        if (hasActiveEnrollment && !allowExistingActiveEnrollment)
        {
            return Result.Failure(ClassErrors.EnrollmentExists);
        }

        return Result.Success();
    }

    /// <summary>
    /// Ghi chú: TryReserveSeatAsync reserve một chỗ trong lớp bằng repository atomic update.
    /// </summary>
    private async Task<Result> TryReserveSeatAsync(Guid classRoomId, CancellationToken cancellationToken)
    {
        var reserved = await classRepository.ReserveSeatAsync(classRoomId, cancellationToken);
        return reserved
            ? Result.Success()
            : Result.Failure(ClassErrors.ClassCapacityExceeded);
    }

    /// <summary>
    /// Ghi chú: ToResponse chuyển entity lớp học thành DTO trả về API.
    /// </summary>
    private static ClassRoomResponse ToResponse(ClassRoom classRoom) =>
        new(
            classRoom.Id,
            classRoom.ClassCode,
            classRoom.Name,
            classRoom.AcademicYearId,
            classRoom.GradeLevel,
            classRoom.Capacity,
            classRoom.ActiveEnrollmentCount,
            classRoom.IsActive);

    /// <summary>
    /// Ghi chú: ToResponse chuyển entity phân công giáo viên thành DTO trả về API.
    /// </summary>
    private static TeachingAssignmentResponse ToResponse(TeachingAssignment assignment) =>
        new(
            assignment.Id,
            assignment.ClassRoomId,
            assignment.SubjectId,
            assignment.TeacherId,
            assignment.SemesterId,
            assignment.IsActive);

    /// <summary>
    /// Ghi chú: ToResponse chuyển entity ghi danh học sinh thành DTO trả về API.
    /// </summary>
    private static EnrollmentResponse ToResponse(Enrollment enrollment) =>
        new(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.ClassRoomId,
            enrollment.SemesterId,
            enrollment.Status.ToString(),
            enrollment.EnrolledAtUtc,
            enrollment.EndedAtUtc);

    /// <summary>
    /// Ghi chú: ToFailedItem tạo kết quả thất bại cho một học sinh trong bulk enrollment.
    /// </summary>
    private static BulkEnrollmentItemResult ToFailedItem(Guid studentId, Error error) =>
        new(studentId, false, null, error.Code, error.Message);
}
