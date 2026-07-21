using EduHub.Domain.Common;
using EduHub.Domain.Entities.Identity;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: TeachingAssignment đại diện cho phân công giáo viên dạy môn học cho một lớp trong một học kỳ.
/// </summary>
public sealed class TeachingAssignment : AuditableEntity
{
    private TeachingAssignment()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor khởi tạo phân công giáo viên cho lớp, môn học và học kỳ.
    /// </summary>
    public TeachingAssignment(Guid classRoomId, Guid subjectId, Guid teacherId, Guid semesterId, DateTime assignedAtUtc)
    {
        ClassRoomId = classRoomId;
        SubjectId = subjectId;
        TeacherId = teacherId;
        SemesterId = semesterId;
        AssignedAtUtc = UtcDateTime.Require(assignedAtUtc, nameof(assignedAtUtc));
    }

    public Guid ClassRoomId { get; private set; }

    public ClassRoom ClassRoom { get; private set; } = null!;

    public Guid SubjectId { get; private set; }

    public Subject Subject { get; private set; } = null!;

    public Guid TeacherId { get; private set; }

    public User Teacher { get; private set; } = null!;

    public Guid SemesterId { get; private set; }

    public Semester Semester { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public DateTime AssignedAtUtc { get; private set; }

    public DateTime? EndedAtUtc { get; private set; }

    /// <summary>
    /// Ghi chú: ReassignTeacher đổi giáo viên của đúng phân công lớp-môn-học kỳ nhưng giữ nguyên assignment để không tách sổ điểm.
    /// </summary>
    public void ReassignTeacher(Guid teacherId, DateTime updatedAtUtc)
    {
        TeacherId = teacherId;
        MarkUpdated(UtcDateTime.Require(updatedAtUtc, nameof(updatedAtUtc)));
    }

    /// <summary>
    /// Ghi chú: Deactivate ngừng phân công giáo viên nhưng giữ lịch sử phân công.
    /// </summary>
    public void Deactivate(DateTime endedAtUtc)
    {
        EndedAtUtc = UtcDateTime.Require(endedAtUtc, nameof(endedAtUtc));
        IsActive = false;
        MarkUpdated(EndedAtUtc.Value);
    }
}
