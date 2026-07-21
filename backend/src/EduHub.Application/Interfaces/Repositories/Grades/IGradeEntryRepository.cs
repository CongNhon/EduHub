using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Enums;
using EduHub.Application.Contracts.Grades;

namespace EduHub.Application.Interfaces.Repositories.Grades;

/// <summary>
/// Ghi chú: IGradeEntryRepository là interface truy cập dữ liệu điểm, enrollment, assignment và outbox.
/// </summary>
public interface IGradeEntryRepository
{
    Task<TeachingAssignment?> GetAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken);

    Task<GradeComponent?> GetActiveComponentAsync(Guid componentId, CancellationToken cancellationToken);

    Task<bool> StudentIsEnrolledAsync(Guid studentId, Guid classRoomId, Guid semesterId, CancellationToken cancellationToken);

    Task<GradeEntry?> GetGradeEntryAsync(Guid studentId, Guid assignmentId, Guid componentId, CancellationToken cancellationToken);

    Task<List<GradeEntry>> GetGradeEntriesByAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken);

    Task<List<GradeComponent>> GetActiveComponentsAsync(Guid subjectId, Guid semesterId, CancellationToken cancellationToken);

    Task<List<Guid>> GetActiveStudentIdsAsync(Guid classRoomId, Guid semesterId, CancellationToken cancellationToken);

    Task<bool> GradebookHasStatusAsync(Guid assignmentId, GradeStatus status, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ParentCanReadStudentAsync kiểm tra phụ huynh đang active có được đọc dữ liệu của học sinh không.
    /// </summary>
    Task<bool> ParentCanReadStudentAsync(Guid parentUserId, Guid studentId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListPublishedGradesAsync đọc điểm Published/Locked của học sinh trong assignment, không trả Draft/Submitted.
    /// </summary>
    Task<PublishedGradebookResponse?> GetPublishedGradebookAsync(
        Guid studentId,
        Guid assignmentId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetGradebookAsync đọc bounded read model gồm context, roster, components, điểm và nhận xét.
    /// </summary>
    Task<GradebookResponse?> GetGradebookAsync(Guid assignmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetStudentRemarkAsync lấy nhận xét của giáo viên cho một học sinh trong assignment.
    /// </summary>
    Task<StudentRemark?> GetStudentRemarkAsync(Guid assignmentId, Guid studentId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetRemarksByAssignmentAsync lấy nhận xét để đồng bộ trạng thái publish/reopen với sổ điểm.
    /// </summary>
    Task<List<StudentRemark>> GetRemarksByAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken);

    void AddGradeEntry(GradeEntry gradeEntry);

    void AddStudentRemark(StudentRemark remark);

    void AddOutboxMessage(OutboxMessage outboxMessage);
}
