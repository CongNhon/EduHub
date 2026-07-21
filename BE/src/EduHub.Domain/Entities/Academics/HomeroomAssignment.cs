using EduHub.Domain.Common;
using EduHub.Domain.Entities.Identity;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: HomeroomAssignment gắn một giáo viên chủ nhiệm với một lớp trong năm học.
/// </summary>
public sealed class HomeroomAssignment : AuditableEntity
{
    private HomeroomAssignment()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo phân công giáo viên chủ nhiệm cho lớp và lưu thời điểm bắt đầu.
    /// </summary>
    public HomeroomAssignment(Guid classRoomId, Guid teacherId, DateTime assignedAtUtc)
    {
        ClassRoomId = classRoomId;
        TeacherId = teacherId;
        AssignedAtUtc = UtcDateTime.Require(assignedAtUtc, nameof(assignedAtUtc));
    }

    public Guid ClassRoomId { get; private set; }
    public ClassRoom ClassRoom { get; private set; } = null!;
    public Guid TeacherId { get; private set; }
    public User Teacher { get; private set; } = null!;
    public DateTime AssignedAtUtc { get; private set; }
    public DateTime? EndedAtUtc { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Ghi chú: Deactivate kết thúc nhiệm vụ chủ nhiệm nhưng giữ lịch sử phân công của lớp.
    /// </summary>
    public void Deactivate(DateTime endedAtUtc)
    {
        EndedAtUtc = UtcDateTime.Require(endedAtUtc, nameof(endedAtUtc));
        IsActive = false;
        MarkUpdated(EndedAtUtc.Value);
    }
}
