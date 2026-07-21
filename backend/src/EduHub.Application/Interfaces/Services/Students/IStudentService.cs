using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Students;

namespace EduHub.Application.Interfaces.Services.Students;

/// <summary>
/// Ghi chú: IStudentService là interface cho nghiệp vụ hồ sơ học sinh và liên kết phụ huynh-học sinh.
/// </summary>
public interface IStudentService
{
    /// <summary>
    /// Ghi chú: CreateStudentAsync xử lý tạo hồ sơ học sinh mới.
    /// </summary>
    Task<Result<StudentResponse>> CreateStudentAsync(CreateStudentCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: UpdateStudentAsync xử lý cập nhật hồ sơ học sinh hiện có.
    /// </summary>
    Task<Result<StudentResponse>> UpdateStudentAsync(UpdateStudentCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetStudentByIdAsync xử lý đọc chi tiết học sinh theo quyền truy cập hiện tại.
    /// </summary>
    Task<Result<StudentResponse>> GetStudentByIdAsync(GetStudentByIdQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListStudentsAsync xử lý đọc danh sách học sinh theo quyền truy cập hiện tại.
    /// </summary>
    Task<Result<PagedResult<StudentResponse>>> ListStudentsAsync(ListStudentsQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetStudentDetailAsync đọc hồ sơ, lớp, phụ huynh và tài khoản của học sinh.
    /// </summary>
    Task<Result<StudentDetailResponse>> GetStudentDetailAsync(GetStudentDetailQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListMyChildrenAsync đọc danh sách con thuộc phụ huynh đang đăng nhập.
    /// </summary>
    Task<Result<IReadOnlyList<ChildSummaryResponse>>> ListMyChildrenAsync(ListMyChildrenQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: LinkStudentUserAsync gắn tài khoản Student với hồ sơ học sinh.
    /// </summary>
    Task<Result<StudentResponse>> LinkStudentUserAsync(LinkStudentUserCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: LinkParentStudentAsync xử lý gắn phụ huynh với học sinh.
    /// </summary>
    Task<Result<ParentStudentResponse>> LinkParentStudentAsync(LinkParentStudentCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: UnlinkParentStudentAsync xử lý ngừng liên kết phụ huynh với học sinh.
    /// </summary>
    Task<Result> UnlinkParentStudentAsync(UnlinkParentStudentCommand request, CancellationToken cancellationToken);
}
