using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Grades;

namespace EduHub.Application.Interfaces.Services.Grades;

/// <summary>
/// Ghi chú: IGradeEntryService là interface nghiệp vụ nhập điểm và state machine sổ điểm.
/// </summary>
public interface IGradeEntryService
{
    Task<Result<GradeEntryResponse>> UpdateGradeAsync(UpdateGradeCommand request, CancellationToken cancellationToken);

    Task<Result<BulkUpdateGradesResponse>> BulkUpdateGradesAsync(BulkUpdateGradesCommand request, CancellationToken cancellationToken);

    Task<Result<GradebookStateResponse>> SubmitGradebookAsync(SubmitGradebookCommand request, CancellationToken cancellationToken);

    Task<Result<GradebookStateResponse>> PublishGradebookAsync(PublishGradebookCommand request, CancellationToken cancellationToken);

    Task<Result<GradebookStateResponse>> ReopenGradebookAsync(ReopenGradebookCommand request, CancellationToken cancellationToken);

    Task<Result<GradebookStateResponse>> LockGradebookAsync(LockGradebookCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetGradebookAsync đọc editor sổ điểm cho giáo viên được phân công hoặc quản trị học vụ.
    /// </summary>
    Task<Result<GradebookResponse>> GetGradebookAsync(GetGradebookQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: UpdateStudentRemarkAsync tạo hoặc sửa nhận xét môn học của giáo viên cho học sinh.
    /// </summary>
    Task<Result<StudentRemarkResponse>> UpdateStudentRemarkAsync(UpdateStudentRemarkCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetPublishedGradesForParentAsync cho phụ huynh đọc điểm đã công bố của con trong assignment.
    /// </summary>
    Task<Result<PublishedGradebookResponse>> GetPublishedGradesForParentAsync(
        GetPublishedGradesForParentQuery request,
        CancellationToken cancellationToken);
}
