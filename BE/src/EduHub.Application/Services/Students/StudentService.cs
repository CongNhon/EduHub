using EduHub.Application.Common.Models;
using EduHub.Application.Features.Students.Common;
using EduHub.Application.Contracts.Students;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.Students;
using EduHub.Application.Interfaces.Services.Students;
using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;

namespace EduHub.Application.Services.Students;

/// <summary>
/// Ghi chú: StudentService xử lý nghiệp vụ hồ sơ học sinh và liên kết phụ huynh-học sinh.
/// </summary>
public sealed class StudentService(
    IStudentRepository studentRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IStudentService
{
    /// <summary>
    /// Ghi chú: CreateStudentAsync tạo hồ sơ học sinh mới nếu mã học sinh chưa tồn tại.
    /// </summary>
    public async Task<Result<StudentResponse>> CreateStudentAsync(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = StudentNormalization.Code(request.StudentCode);
        var exists = await studentRepository.StudentCodeExistsAsync(normalizedCode, cancellationToken);

        if (exists)
        {
            return Result.Failure<StudentResponse>(StudentErrors.StudentCodeExists);
        }

        var student = new Student(
            request.StudentCode.Trim(),
            normalizedCode,
            request.FullName.Trim(),
            StudentNormalization.SearchText(request.FullName),
            request.DateOfBirth);

        studentRepository.AddStudent(student);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(student));
    }

    /// <summary>
    /// Ghi chú: UpdateStudentAsync cập nhật hồ sơ học sinh nếu version còn khớp.
    /// </summary>
    public async Task<Result<StudentResponse>> UpdateStudentAsync(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetStudentAsync(request.Id, cancellationToken);
        if (student is null)
        {
            return Result.Failure<StudentResponse>(StudentErrors.StudentNotFound);
        }

        if (student.Version != request.Version)
        {
            return Result.Failure<StudentResponse>(StudentErrors.ConcurrencyConflict);
        }

        student.UpdateProfile(
            request.FullName,
            StudentNormalization.SearchText(request.FullName),
            request.DateOfBirth,
            request.Status,
            timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(student));
    }

    /// <summary>
    /// Ghi chú: GetStudentByIdAsync đọc chi tiết học sinh theo role hiện tại.
    /// </summary>
    public async Task<Result<StudentResponse>> GetStudentByIdAsync(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetStudentByScopeAsync(
            request.Id,
            currentUser.Role,
            currentUser.UserId,
            cancellationToken);

        return student is null
            ? Result.Failure<StudentResponse>(StudentErrors.StudentNotFound)
            : Result.Success(student);
    }

    /// <summary>
    /// Ghi chú: ListStudentsAsync đọc danh sách học sinh theo role hiện tại, trạng thái và phân trang.
    /// </summary>
    public async Task<Result<PagedResult<StudentResponse>>> ListStudentsAsync(
        ListStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var pageRequest = PageRequest.Create(request.Page, request.PageSize, request.Search);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<PagedResult<StudentResponse>>(pageRequest.Error!);
        }

        return Result.Success(await studentRepository.ListStudentsByScopeAsync(
            request.Status,
            request.ClassRoomId,
            pageRequest.Value,
            currentUser.Role,
            currentUser.UserId,
            cancellationToken));
    }

    /// <summary>
    /// Ghi chú: GetStudentDetailAsync trả hồ sơ, lớp và phụ huynh của học sinh theo scope hiện tại.
    /// </summary>
    public async Task<Result<StudentDetailResponse>> GetStudentDetailAsync(GetStudentDetailQuery request, CancellationToken cancellationToken)
    {
        var detail = await studentRepository.GetStudentDetailByScopeAsync(request.StudentId, currentUser.Role, currentUser.UserId, cancellationToken);
        return detail is null ? Result.Failure<StudentDetailResponse>(StudentErrors.StudentNotFound) : Result.Success(detail);
    }

    /// <summary>
    /// Ghi chú: ListMyChildrenAsync trả danh sách con có liên kết active của phụ huynh đang đăng nhập.
    /// </summary>
    public async Task<Result<IReadOnlyList<ChildSummaryResponse>>> ListMyChildrenAsync(ListMyChildrenQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.Parent || currentUser.UserId is null)
        {
            return Result.Failure<IReadOnlyList<ChildSummaryResponse>>(StudentErrors.ParentRequired);
        }

        return Result.Success(await studentRepository.ListChildrenAsync(currentUser.UserId.Value, cancellationToken));
    }

    /// <summary>
    /// Ghi chú: LinkStudentUserAsync gắn tài khoản Student active vào hồ sơ học sinh chưa có tài khoản khác.
    /// </summary>
    public async Task<Result<StudentResponse>> LinkStudentUserAsync(LinkStudentUserCommand request, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetStudentAsync(request.StudentId, cancellationToken);
        if (student is null) return Result.Failure<StudentResponse>(StudentErrors.StudentNotFound);
        if (!await studentRepository.StudentUserIsValidAsync(request.UserId, cancellationToken)) return Result.Failure<StudentResponse>(StudentErrors.StudentUserInvalid);
        if (await studentRepository.StudentUserLinkedAsync(request.UserId, request.StudentId, cancellationToken)) return Result.Failure<StudentResponse>(StudentErrors.StudentUserAlreadyLinked);

        student.LinkUserAccount(request.UserId, timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToResponse(student));
    }

    /// <summary>
    /// Ghi chú: LinkParentStudentAsync gắn user phụ huynh active vào hồ sơ học sinh.
    /// </summary>
    public async Task<Result<ParentStudentResponse>> LinkParentStudentAsync(
        LinkParentStudentCommand request,
        CancellationToken cancellationToken)
    {
        var studentExists = await studentRepository.StudentExistsAsync(request.StudentId, cancellationToken);
        if (!studentExists)
        {
            return Result.Failure<ParentStudentResponse>(StudentErrors.StudentNotFound);
        }

        var parentIsValid = await studentRepository.ParentUserIsValidAsync(request.ParentUserId, cancellationToken);
        if (!parentIsValid)
        {
            return Result.Failure<ParentStudentResponse>(StudentErrors.ParentUserInvalid);
        }

        var existing = await studentRepository.GetParentStudentLinkAsync(
            request.StudentId,
            request.ParentUserId,
            cancellationToken);

        if (existing is not null && existing.IsActive)
        {
            return Result.Failure<ParentStudentResponse>(StudentErrors.ParentLinkExists);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        ParentStudent link;
        if (existing is null)
        {
            link = new ParentStudent(request.ParentUserId, request.StudentId, request.Relationship, now);
            studentRepository.AddParentStudentLink(link);
        }
        else
        {
            existing.Reactivate(request.Relationship, now);
            link = existing;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(link));
    }

    /// <summary>
    /// Ghi chú: UnlinkParentStudentAsync deactivate liên kết phụ huynh-học sinh active.
    /// </summary>
    public async Task<Result> UnlinkParentStudentAsync(UnlinkParentStudentCommand request, CancellationToken cancellationToken)
    {
        var link = await studentRepository.GetActiveParentStudentLinkAsync(
            request.StudentId,
            request.ParentUserId,
            cancellationToken);

        if (link is null)
        {
            return Result.Failure(StudentErrors.ParentLinkNotFound);
        }

        link.Deactivate(timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Ghi chú: ToResponse chuyển entity hồ sơ học sinh thành DTO trả về API.
    /// </summary>
    private static StudentResponse ToResponse(Student student) =>
        new(student.Id, student.StudentCode, student.FullName, student.DateOfBirth, student.Status.ToString(), student.Version);

    /// <summary>
    /// Ghi chú: ToResponse chuyển entity liên kết phụ huynh-học sinh thành DTO trả về API.
    /// </summary>
    private static ParentStudentResponse ToResponse(ParentStudent link) =>
        new(link.Id, link.StudentId, link.ParentUserId, link.Relationship, link.IsActive);
}
