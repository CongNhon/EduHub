using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Classes;

namespace EduHub.Application.Interfaces.Services.Classes;

/// <summary>
/// Ghi chú: IClassService là interface cho nghiệp vụ lớp học, phân công giáo viên và ghi danh học sinh.
/// </summary>
public interface IClassService
{
    /// <summary>
    /// Ghi chú: CreateClassRoomAsync xử lý tạo lớp học mới.
    /// </summary>
    Task<Result<ClassRoomResponse>> CreateClassRoomAsync(CreateClassRoomCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: UpdateClassRoomAsync xử lý cập nhật lớp học hiện có.
    /// </summary>
    Task<Result<ClassRoomResponse>> UpdateClassRoomAsync(UpdateClassRoomCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListClassRoomsAsync xử lý đọc danh sách lớp học.
    /// </summary>
    Task<Result<PagedResult<ClassRoomResponse>>> ListClassRoomsAsync(ListClassRoomsQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: AssignTeacherAsync xử lý phân công giáo viên dạy lớp-môn-học kỳ.
    /// </summary>
    Task<Result<TeachingAssignmentResponse>> AssignTeacherAsync(AssignTeacherCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListMyTeachingAssignmentsAsync trả lớp được phân công cho giáo viên đang đăng nhập.
    /// </summary>
    Task<Result<IReadOnlyList<TeachingAssignmentSummaryResponse>>> ListMyTeachingAssignmentsAsync(ListMyTeachingAssignmentsQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListTeachingAssignmentsAsync trả danh sách phân công để quản trị học vụ kiểm soát giáo viên-lớp-môn.
    /// </summary>
    Task<Result<IReadOnlyList<TeachingAssignmentSummaryResponse>>> ListTeachingAssignmentsAsync(ListTeachingAssignmentsQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: EnrollStudentAsync xử lý ghi danh một học sinh vào lớp.
    /// </summary>
    Task<Result<EnrollmentResponse>> EnrollStudentAsync(EnrollStudentCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: BulkEnrollStudentsAsync xử lý ghi danh nhiều học sinh vào lớp.
    /// </summary>
    Task<Result<BulkEnrollmentResponse>> BulkEnrollStudentsAsync(BulkEnrollStudentsCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: TransferEnrollmentAsync xử lý chuyển học sinh từ lớp cũ sang lớp mới.
    /// </summary>
    Task<Result<EnrollmentResponse>> TransferEnrollmentAsync(TransferEnrollmentCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: WithdrawEnrollmentAsync xử lý rút học sinh khỏi lớp.
    /// </summary>
    Task<Result> WithdrawEnrollmentAsync(WithdrawEnrollmentCommand request, CancellationToken cancellationToken);
}
