using EduHub.Domain.Enums;

namespace EduHub.WebApi.Dtos.Students;

/// <summary>
/// Ghi chú: CreateStudentRequest là DTO request API dùng để tạo hồ sơ học sinh.
/// </summary>
public sealed record CreateStudentRequest(string StudentCode, string FullName, DateOnly DateOfBirth);

/// <summary>
/// Ghi chú: UpdateStudentRequest là DTO request API dùng để cập nhật hồ sơ học sinh.
/// </summary>
public sealed record UpdateStudentRequest(string FullName, DateOnly DateOfBirth, StudentStatus Status, int Version);

/// <summary>
/// Ghi chú: GetStudentByIdRequest là DTO request API dùng để lấy học sinh theo id trên route.
/// </summary>
public sealed record GetStudentByIdRequest(Guid Id);

/// <summary>
/// Ghi chú: ListStudentsRequest là DTO request API dùng để lọc học sinh theo trạng thái và phân trang.
/// </summary>
public sealed record ListStudentsRequest(StudentStatus? Status, int? Page, int? PageSize, string? Search, Guid? ClassRoomId);

/// <summary>
/// Ghi chú: StudentDto là DTO response API chứa thông tin hồ sơ học sinh.
/// </summary>
public sealed record StudentDto(
    Guid Id,
    string StudentCode,
    string FullName,
    DateOnly DateOfBirth,
    string Status,
    int Version,
    Guid? CurrentClassId,
    string? CurrentClassCode,
    string? CurrentClassName,
    int GuardianCount,
    string? AccountEmail,
    bool? AccountIsActive);

/// <summary>
/// Ghi chú: StudentEnrollmentSummaryDto trả lịch sử lớp của học sinh theo từng học kỳ.
/// </summary>
public sealed record StudentEnrollmentSummaryDto(Guid Id, Guid ClassRoomId, string ClassCode, string ClassName, Guid SemesterId, string SemesterName, string Status, DateTime EnrolledAtUtc, DateTime? EndedAtUtc);

/// <summary>
/// Ghi chú: StudentGuardianDto trả phụ huynh, quan hệ và trạng thái liên kết của học sinh.
/// </summary>
public sealed record StudentGuardianDto(Guid LinkId, Guid ParentUserId, string FullName, string Email, string? PhoneNumber, string Relationship, bool IsActive);

/// <summary>
/// Ghi chú: StudentDetailDto trả hồ sơ đầy đủ dùng trong detail drawer của quản trị học vụ.
/// </summary>
public sealed record StudentDetailDto(StudentDto Student, IReadOnlyList<StudentEnrollmentSummaryDto> Enrollments, IReadOnlyList<StudentGuardianDto> Guardians);

/// <summary>
/// Ghi chú: ChildSummaryDto trả thông tin con để phụ huynh chọn theo tên, mã và lớp.
/// </summary>
public sealed record ChildSummaryDto(Guid Id, string StudentCode, string FullName, DateOnly DateOfBirth, string Relationship, Guid? CurrentClassId, string? CurrentClassCode, string? CurrentClassName, Guid? CurrentSemesterId, string? CurrentSemesterName);

/// <summary>
/// Ghi chú: LinkStudentUserRequest chứa tài khoản Student cần gắn với hồ sơ học sinh.
/// </summary>
public sealed record LinkStudentUserRequest(Guid UserId);

/// <summary>
/// Ghi chú: LinkParentStudentRequest là DTO request API dùng để gắn phụ huynh với học sinh.
/// </summary>
public sealed record LinkParentStudentRequest(string Relationship);

/// <summary>
/// Ghi chú: UnlinkParentStudentRequest là DTO request API dùng để bỏ liên kết phụ huynh-học sinh theo route.
/// </summary>
public sealed record UnlinkParentStudentRequest(Guid Id, Guid ParentUserId);

/// <summary>
/// Ghi chú: ParentStudentDto là DTO response API chứa thông tin liên kết phụ huynh-học sinh.
/// </summary>
public sealed record ParentStudentDto(
    Guid Id,
    Guid StudentId,
    Guid ParentUserId,
    string Relationship,
    bool IsActive);
