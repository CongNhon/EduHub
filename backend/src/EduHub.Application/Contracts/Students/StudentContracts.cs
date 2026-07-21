using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Domain.Enums;

namespace EduHub.Application.Contracts.Students;

/// <summary>
/// Ghi chú: StudentResponse là dữ liệu trả về cho hồ sơ học sinh.
/// </summary>
public sealed record StudentResponse(
    Guid Id,
    string StudentCode,
    string FullName,
    DateOnly DateOfBirth,
    string Status,
    int Version,
    Guid? CurrentClassId = null,
    string? CurrentClassCode = null,
    string? CurrentClassName = null,
    int GuardianCount = 0,
    string? AccountEmail = null,
    bool? AccountIsActive = null);

/// <summary>
/// Ghi chú: CreateStudentCommand là command để tạo hồ sơ học sinh mới.
/// </summary>
public sealed record CreateStudentCommand(
    string StudentCode,
    string FullName,
    DateOnly DateOfBirth) : ICommand<Result<StudentResponse>>;

/// <summary>
/// Ghi chú: UpdateStudentCommand là command để cập nhật hồ sơ học sinh.
/// </summary>
public sealed record UpdateStudentCommand(
    Guid Id,
    string FullName,
    DateOnly DateOfBirth,
    StudentStatus Status,
    int Version) : ICommand<Result<StudentResponse>>;

/// <summary>
/// Ghi chú: GetStudentByIdQuery là query để đọc chi tiết học sinh theo id.
/// </summary>
public sealed record GetStudentByIdQuery(Guid Id) : IQuery<Result<StudentResponse>>;

/// <summary>
/// Ghi chú: ListStudentsQuery là query để đọc danh sách học sinh.
/// </summary>
public sealed record ListStudentsQuery(
    StudentStatus? Status = null,
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize,
    string? Search = null,
    Guid? ClassRoomId = null) : IQuery<Result<PagedResult<StudentResponse>>>;

/// <summary>
/// Ghi chú: StudentEnrollmentSummaryResponse chứa lịch sử lớp học của một học sinh theo học kỳ.
/// </summary>
public sealed record StudentEnrollmentSummaryResponse(Guid Id, Guid ClassRoomId, string ClassCode, string ClassName, Guid SemesterId, string SemesterName, string Status, DateTime EnrolledAtUtc, DateTime? EndedAtUtc);

/// <summary>
/// Ghi chú: StudentGuardianResponse chứa phụ huynh đang hoặc từng được liên kết với học sinh.
/// </summary>
public sealed record StudentGuardianResponse(Guid LinkId, Guid ParentUserId, string FullName, string Email, string? PhoneNumber, string Relationship, bool IsActive);

/// <summary>
/// Ghi chú: StudentDetailResponse gom hồ sơ, lớp, phụ huynh và tài khoản của một học sinh cho detail drawer.
/// </summary>
public sealed record StudentDetailResponse(StudentResponse Student, IReadOnlyList<StudentEnrollmentSummaryResponse> Enrollments, IReadOnlyList<StudentGuardianResponse> Guardians);

/// <summary>
/// Ghi chú: GetStudentDetailQuery đọc toàn bộ ngữ cảnh học vụ của một học sinh theo quyền hiện tại.
/// </summary>
public sealed record GetStudentDetailQuery(Guid StudentId) : IQuery<Result<StudentDetailResponse>>;

/// <summary>
/// Ghi chú: ChildSummaryResponse chứa thông tin con đã liên kết để phụ huynh chọn mà không dùng UUID thủ công.
/// </summary>
public sealed record ChildSummaryResponse(Guid Id, string StudentCode, string FullName, DateOnly DateOfBirth, string Relationship, Guid? CurrentClassId, string? CurrentClassCode, string? CurrentClassName, Guid? CurrentSemesterId, string? CurrentSemesterName);

/// <summary>
/// Ghi chú: ListMyChildrenQuery đọc danh sách con thuộc liên kết active của phụ huynh đang đăng nhập.
/// </summary>
public sealed record ListMyChildrenQuery : IQuery<Result<IReadOnlyList<ChildSummaryResponse>>>;

/// <summary>
/// Ghi chú: LinkStudentUserCommand gắn tài khoản role Student với hồ sơ học sinh tương ứng.
/// </summary>
public sealed record LinkStudentUserCommand(Guid StudentId, Guid UserId) : ICommand<Result<StudentResponse>>;

/// <summary>
/// Ghi chú: ParentStudentResponse là dữ liệu trả về cho liên kết phụ huynh-học sinh.
/// </summary>
public sealed record ParentStudentResponse(
    Guid Id,
    Guid StudentId,
    Guid ParentUserId,
    string Relationship,
    bool IsActive);

/// <summary>
/// Ghi chú: LinkParentStudentCommand là command để gắn phụ huynh với học sinh.
/// </summary>
public sealed record LinkParentStudentCommand(
    Guid StudentId,
    Guid ParentUserId,
    string Relationship) : ICommand<Result<ParentStudentResponse>>;

/// <summary>
/// Ghi chú: UnlinkParentStudentCommand là command để ngừng liên kết phụ huynh-học sinh.
/// </summary>
public sealed record UnlinkParentStudentCommand(
    Guid StudentId,
    Guid ParentUserId) : ICommand<Result>;
