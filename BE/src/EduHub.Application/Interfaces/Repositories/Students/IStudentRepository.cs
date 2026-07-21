using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Students;
using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;

namespace EduHub.Application.Interfaces.Repositories.Students;

/// <summary>
/// Ghi chú: IStudentRepository là interface truy cập dữ liệu học sinh và liên kết phụ huynh-học sinh.
/// </summary>
public interface IStudentRepository
{
    /// <summary>
    /// Ghi chú: StudentCodeExistsAsync kiểm tra mã học sinh đã tồn tại theo mã chuẩn hóa.
    /// </summary>
    Task<bool> StudentCodeExistsAsync(string normalizedStudentCode, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: AddStudent thêm entity hồ sơ học sinh mới vào database context.
    /// </summary>
    void AddStudent(Student student);

    /// <summary>
    /// Ghi chú: GetStudentAsync lấy entity học sinh theo id để cập nhật.
    /// </summary>
    Task<Student?> GetStudentAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetStudentByScopeAsync đọc chi tiết học sinh theo role và user hiện tại.
    /// </summary>
    Task<StudentResponse?> GetStudentByScopeAsync(Guid id, UserRole? role, Guid? userId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListStudentsByScopeAsync đọc danh sách học sinh theo role, user, trạng thái và phân trang.
    /// </summary>
    Task<PagedResult<StudentResponse>> ListStudentsByScopeAsync(
        StudentStatus? status,
        Guid? classRoomId,
        PageRequest pageRequest,
        UserRole? role,
        Guid? userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetStudentDetailByScopeAsync đọc hồ sơ, lớp và phụ huynh của học sinh theo quyền hiện tại.
    /// </summary>
    Task<StudentDetailResponse?> GetStudentDetailByScopeAsync(Guid studentId, UserRole? role, Guid? userId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListChildrenAsync đọc các học sinh có liên kết active với phụ huynh hiện tại.
    /// </summary>
    Task<IReadOnlyList<ChildSummaryResponse>> ListChildrenAsync(Guid parentUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: StudentUserIsValidAsync kiểm tra tài khoản active có role Student trước khi gắn hồ sơ.
    /// </summary>
    Task<bool> StudentUserIsValidAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: StudentUserLinkedAsync kiểm tra tài khoản Student đã thuộc hồ sơ khác hay chưa.
    /// </summary>
    Task<bool> StudentUserLinkedAsync(Guid userId, Guid exceptStudentId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: StudentExistsAsync kiểm tra học sinh tồn tại theo id.
    /// </summary>
    Task<bool> StudentExistsAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ParentUserIsValidAsync kiểm tra user phụ huynh active có role Parent.
    /// </summary>
    Task<bool> ParentUserIsValidAsync(Guid parentUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetParentStudentLinkAsync lấy liên kết phụ huynh-học sinh bất kể active hay inactive.
    /// </summary>
    Task<ParentStudent?> GetParentStudentLinkAsync(Guid studentId, Guid parentUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetActiveParentStudentLinkAsync lấy liên kết phụ huynh-học sinh đang active.
    /// </summary>
    Task<ParentStudent?> GetActiveParentStudentLinkAsync(Guid studentId, Guid parentUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: AddParentStudentLink thêm entity liên kết phụ huynh-học sinh vào database context.
    /// </summary>
    void AddParentStudentLink(ParentStudent link);
}
