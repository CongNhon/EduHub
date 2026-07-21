namespace EduHub.WebApi.Dtos.Classes;

/// <summary>
/// Ghi chú: CreateClassRoomRequest là DTO request API dùng để tạo lớp học.
/// </summary>
public sealed record CreateClassRoomRequest(
    string ClassCode,
    string Name,
    Guid AcademicYearId,
    int GradeLevel,
    int Capacity);

/// <summary>
/// Ghi chú: UpdateClassRoomRequest là DTO request API dùng để cập nhật lớp học.
/// </summary>
public sealed record UpdateClassRoomRequest(string Name, int GradeLevel, int Capacity);

/// <summary>
/// Ghi chú: ListClassRoomsRequest là DTO request API dùng để lọc lớp học theo năm học và phân trang.
/// </summary>
public sealed record ListClassRoomsRequest(Guid? AcademicYearId, int? Page, int? PageSize, string? Search);

/// <summary>
/// Ghi chú: ClassRoomDto là DTO response API chứa thông tin lớp học.
/// </summary>
public sealed record ClassRoomDto(
    Guid Id,
    string ClassCode,
    string Name,
    Guid AcademicYearId,
    int GradeLevel,
    int Capacity,
    int ActiveEnrollmentCount,
    bool IsActive);

/// <summary>
/// Ghi chú: AssignTeacherRequest là DTO request API dùng để phân công giáo viên cho lớp-môn-học kỳ.
/// </summary>
public sealed record AssignTeacherRequest(Guid SubjectId, Guid TeacherId, Guid SemesterId);

/// <summary>
/// Ghi chú: TeachingAssignmentDto là DTO response API chứa thông tin phân công giáo viên.
/// </summary>
public sealed record TeachingAssignmentDto(
    Guid Id,
    Guid ClassRoomId,
    Guid SubjectId,
    Guid TeacherId,
    Guid SemesterId,
    bool IsActive);

/// <summary>
/// Ghi chú: TeachingAssignmentSummaryDto trả lớp, môn, học kỳ, sĩ số và trạng thái sổ điểm cho giáo viên.
/// </summary>
public sealed record TeachingAssignmentSummaryDto(Guid Id, Guid ClassRoomId, string ClassCode, string ClassName, Guid SubjectId, string SubjectCode, string SubjectName, Guid SemesterId, string SemesterName, Guid TeacherId, string TeacherName, int StudentCount, string GradebookStatus, bool IsActive);

/// <summary>
/// Ghi chú: ListTeachingAssignmentsRequest chứa bộ lọc phân công dành cho quản trị học vụ.
/// </summary>
public sealed record ListTeachingAssignmentsRequest(Guid? ClassRoomId, Guid? TeacherId, Guid? SemesterId);

/// <summary>
/// Ghi chú: EnrollStudentRequest là DTO request API dùng để ghi danh một học sinh vào lớp.
/// </summary>
public sealed record EnrollStudentRequest(Guid StudentId, Guid SemesterId);

/// <summary>
/// Ghi chú: BulkEnrollStudentsRequest là DTO request API dùng để ghi danh nhiều học sinh vào lớp.
/// </summary>
public sealed record BulkEnrollStudentsRequest(Guid SemesterId, IReadOnlyList<Guid> StudentIds);

/// <summary>
/// Ghi chú: TransferEnrollmentRequest là DTO request API dùng để chuyển học sinh sang lớp mới.
/// </summary>
public sealed record TransferEnrollmentRequest(Guid ToClassRoomId, Guid SemesterId, string Reason);

/// <summary>
/// Ghi chú: WithdrawEnrollmentRequest là DTO request API dùng để rút học sinh khỏi lớp.
/// </summary>
public sealed record WithdrawEnrollmentRequest(Guid SemesterId, string Reason);

/// <summary>
/// Ghi chú: EnrollmentDto là DTO response API chứa thông tin ghi danh học sinh vào lớp.
/// </summary>
public sealed record EnrollmentDto(
    Guid Id,
    Guid StudentId,
    Guid ClassRoomId,
    Guid SemesterId,
    string Status,
    DateTime EnrolledAtUtc,
    DateTime? EndedAtUtc);

/// <summary>
/// Ghi chú: BulkEnrollmentItemDto là DTO response API chứa kết quả từng học sinh trong bulk enrollment.
/// </summary>
public sealed record BulkEnrollmentItemDto(
    Guid StudentId,
    bool Success,
    Guid? EnrollmentId,
    string? ErrorCode,
    string? ErrorMessage);

/// <summary>
/// Ghi chú: BulkEnrollmentDto là DTO response API chứa tổng kết ghi danh hàng loạt học sinh.
/// </summary>
public sealed record BulkEnrollmentDto(
    IReadOnlyList<BulkEnrollmentItemDto> Items,
    int SuccessCount,
    int ErrorCount);
