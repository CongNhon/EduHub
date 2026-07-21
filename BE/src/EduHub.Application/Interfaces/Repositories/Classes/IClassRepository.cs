using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Classes;
using EduHub.Domain.Entities.Academics;

namespace EduHub.Application.Interfaces.Repositories.Classes;

/// <summary>
/// Ghi chú: IClassRepository là interface truy cập dữ liệu lớp học, phân công giáo viên và ghi danh học sinh.
/// </summary>
public interface IClassRepository
{
    /// <summary>
    /// Ghi chú: AcademicYearExistsAsync kiểm tra năm học tồn tại cho lớp học.
    /// </summary>
    Task<bool> AcademicYearExistsAsync(Guid academicYearId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ClassCodeExistsAsync kiểm tra mã lớp đã tồn tại trong năm học.
    /// </summary>
    Task<bool> ClassCodeExistsAsync(Guid academicYearId, string normalizedClassCode, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: AddClassRoom thêm entity lớp học mới vào database context.
    /// </summary>
    void AddClassRoom(ClassRoom classRoom);

    /// <summary>
    /// Ghi chú: GetClassRoomAsync lấy lớp học theo id để cập nhật.
    /// </summary>
    Task<ClassRoom?> GetClassRoomAsync(Guid classRoomId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetActiveClassRoomAsync lấy lớp học active theo id.
    /// </summary>
    Task<ClassRoom?> GetActiveClassRoomAsync(Guid classRoomId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListClassRoomsAsync đọc danh sách lớp học active đã phân trang.
    /// </summary>
    Task<PagedResult<ClassRoomResponse>> ListClassRoomsAsync(Guid? academicYearId, PageRequest pageRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetSemesterAsync lấy học kỳ theo id để kiểm tra scope lớp học.
    /// </summary>
    Task<Semester?> GetSemesterAsync(Guid semesterId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ActiveSubjectExistsAsync kiểm tra môn học active theo id.
    /// </summary>
    Task<bool> ActiveSubjectExistsAsync(Guid subjectId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ActiveTeacherExistsAsync kiểm tra giáo viên active có role Teacher.
    /// </summary>
    Task<bool> ActiveTeacherExistsAsync(Guid teacherId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ActiveTeachingAssignmentExistsAsync kiểm tra phân công giáo viên active bị trùng scope.
    /// </summary>
    Task<bool> ActiveTeachingAssignmentExistsAsync(Guid classRoomId, Guid subjectId, Guid semesterId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ActiveTeacherCapabilityExistsAsync kiểm tra giáo viên được phép dạy đúng môn trước khi tạo assignment.
    /// </summary>
    Task<bool> ActiveTeacherCapabilityExistsAsync(Guid teacherId, Guid subjectId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: IsHomeroomTeacherForClassAsync kiểm tra giáo viên có đang chủ nhiệm lớp cần phân công môn hay không.
    /// </summary>
    Task<bool> IsHomeroomTeacherForClassAsync(Guid teacherId, Guid classRoomId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: AddTeachingAssignment thêm entity phân công giáo viên vào database context.
    /// </summary>
    void AddTeachingAssignment(TeachingAssignment assignment);

    /// <summary>
    /// Ghi chú: ListTeachingAssignmentsAsync đọc lớp-môn-học kỳ được phân công cho giáo viên hiện tại.
    /// </summary>
    Task<IReadOnlyList<TeachingAssignmentSummaryResponse>> ListTeachingAssignmentsAsync(Guid? teacherId, Guid? classRoomId, Guid? semesterId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ActiveStudentExistsAsync kiểm tra học sinh active theo id.
    /// </summary>
    Task<bool> ActiveStudentExistsAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ActiveEnrollmentExistsAsync kiểm tra học sinh đã có enrollment active trong học kỳ.
    /// </summary>
    Task<bool> ActiveEnrollmentExistsAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ReserveSeatAsync tăng sĩ số active của lớp bằng atomic update.
    /// </summary>
    Task<bool> ReserveSeatAsync(Guid classRoomId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ReleaseSeatAsync giảm sĩ số active của lớp khi học sinh rút hoặc chuyển lớp.
    /// </summary>
    Task ReleaseSeatAsync(Guid classRoomId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: AddEnrollment thêm entity ghi danh học sinh vào database context.
    /// </summary>
    void AddEnrollment(Enrollment enrollment);

    /// <summary>
    /// Ghi chú: GetActiveEnrollmentAsync lấy enrollment active của học sinh trong lớp và học kỳ.
    /// </summary>
    Task<Enrollment?> GetActiveEnrollmentAsync(Guid studentId, Guid classRoomId, Guid semesterId, CancellationToken cancellationToken);
}
