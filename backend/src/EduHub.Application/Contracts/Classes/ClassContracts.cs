using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using MediatR;

namespace EduHub.Application.Contracts.Classes;

/// <summary>
/// Ghi chú: ClassRoomResponse là dữ liệu trả về cho lớp học.
/// </summary>
public sealed record ClassRoomResponse(
    Guid Id,
    string ClassCode,
    string Name,
    Guid AcademicYearId,
    int GradeLevel,
    int Capacity,
    int ActiveEnrollmentCount,
    bool IsActive);

/// <summary>
/// Ghi chú: CreateClassRoomCommand là command để tạo lớp học mới.
/// </summary>
public sealed record CreateClassRoomCommand(
    string ClassCode,
    string Name,
    Guid AcademicYearId,
    int GradeLevel,
    int Capacity) : ICommand<Result<ClassRoomResponse>>;

/// <summary>
/// Ghi chú: UpdateClassRoomCommand là command để cập nhật lớp học.
/// </summary>
public sealed record UpdateClassRoomCommand(
    Guid Id,
    string Name,
    int GradeLevel,
    int Capacity) : ICommand<Result<ClassRoomResponse>>;

/// <summary>
/// Ghi chú: ListClassRoomsQuery là query để đọc danh sách lớp học.
/// </summary>
public sealed record ListClassRoomsQuery(
    Guid? AcademicYearId = null,
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize,
    string? Search = null) : IQuery<Result<PagedResult<ClassRoomResponse>>>;

/// <summary>
/// Ghi chú: TeachingAssignmentResponse là dữ liệu trả về cho phân công giáo viên.
/// </summary>
public sealed record TeachingAssignmentResponse(
    Guid Id,
    Guid ClassRoomId,
    Guid SubjectId,
    Guid TeacherId,
    Guid SemesterId,
    bool IsActive);

/// <summary>
/// Ghi chú: TeachingAssignmentSummaryResponse chứa lớp, môn, học kỳ và sĩ số để giáo viên chọn sổ điểm.
/// </summary>
public sealed record TeachingAssignmentSummaryResponse(
    Guid Id,
    Guid ClassRoomId,
    string ClassCode,
    string ClassName,
    Guid SubjectId,
    string SubjectCode,
    string SubjectName,
    Guid SemesterId,
    string SemesterName,
    Guid TeacherId,
    string TeacherName,
    int StudentCount,
    string GradebookStatus,
    bool IsActive);

/// <summary>
/// Ghi chú: ListMyTeachingAssignmentsQuery đọc các phân công thuộc giáo viên đang đăng nhập.
/// </summary>
public sealed record ListMyTeachingAssignmentsQuery(Guid? SemesterId = null) : IQuery<Result<IReadOnlyList<TeachingAssignmentSummaryResponse>>>;

/// <summary>
/// Ghi chú: ListTeachingAssignmentsQuery cho quản trị học vụ đọc phân công theo lớp, giáo viên hoặc học kỳ.
/// </summary>
public sealed record ListTeachingAssignmentsQuery(Guid? ClassRoomId = null, Guid? TeacherId = null, Guid? SemesterId = null)
    : IQuery<Result<IReadOnlyList<TeachingAssignmentSummaryResponse>>>;

/// <summary>
/// Ghi chú: AssignTeacherCommand là command để phân công giáo viên dạy lớp-môn-học kỳ.
/// </summary>
public sealed record AssignTeacherCommand(
    Guid ClassRoomId,
    Guid SubjectId,
    Guid TeacherId,
    Guid SemesterId) : ICommand<Result<TeachingAssignmentResponse>>;

/// <summary>
/// Ghi chú: EnrollmentResponse là dữ liệu trả về cho ghi danh học sinh vào lớp.
/// </summary>
public sealed record EnrollmentResponse(
    Guid Id,
    Guid StudentId,
    Guid ClassRoomId,
    Guid SemesterId,
    string Status,
    DateTime EnrolledAtUtc,
    DateTime? EndedAtUtc);

/// <summary>
/// Ghi chú: BulkEnrollmentItemResult là kết quả từng học sinh khi ghi danh hàng loạt.
/// </summary>
public sealed record BulkEnrollmentItemResult(
    Guid StudentId,
    bool Success,
    Guid? EnrollmentId,
    string? ErrorCode,
    string? ErrorMessage);

/// <summary>
/// Ghi chú: BulkEnrollmentResponse là tổng kết ghi danh hàng loạt học sinh.
/// </summary>
public sealed record BulkEnrollmentResponse(
    IReadOnlyList<BulkEnrollmentItemResult> Items,
    int SuccessCount,
    int ErrorCount);

/// <summary>
/// Ghi chú: EnrollStudentCommand là command để ghi danh một học sinh vào lớp.
/// </summary>
public sealed record EnrollStudentCommand(
    Guid ClassRoomId,
    Guid StudentId,
    Guid SemesterId) : ICommand<Result<EnrollmentResponse>>;

/// <summary>
/// Ghi chú: BulkEnrollStudentsCommand là request để ghi danh hàng loạt học sinh vào lớp.
/// </summary>
public sealed record BulkEnrollStudentsCommand(
    Guid ClassRoomId,
    Guid SemesterId,
    IReadOnlyList<Guid> StudentIds) : IRequest<Result<BulkEnrollmentResponse>>;

/// <summary>
/// Ghi chú: TransferEnrollmentCommand là command để chuyển học sinh sang lớp mới.
/// </summary>
public sealed record TransferEnrollmentCommand(
    Guid StudentId,
    Guid FromClassRoomId,
    Guid ToClassRoomId,
    Guid SemesterId,
    string Reason) : ICommand<Result<EnrollmentResponse>>;

/// <summary>
/// Ghi chú: WithdrawEnrollmentCommand là command để rút học sinh khỏi lớp.
/// </summary>
public sealed record WithdrawEnrollmentCommand(
    Guid StudentId,
    Guid ClassRoomId,
    Guid SemesterId,
    string Reason) : ICommand<Result>;
