using EduHub.Domain.Common;
using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: Enrollment đại diện cho việc ghi danh một học sinh vào một lớp trong một học kỳ.
/// </summary>
public sealed class Enrollment : AuditableEntity
{
    private Enrollment()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor khởi tạo ghi danh active cho học sinh vào lớp và học kỳ.
    /// </summary>
    public Enrollment(Guid studentId, Guid classRoomId, Guid semesterId, DateTime enrolledAtUtc)
    {
        StudentId = studentId;
        ClassRoomId = classRoomId;
        SemesterId = semesterId;
        EnrolledAtUtc = UtcDateTime.Require(enrolledAtUtc, nameof(enrolledAtUtc));
    }

    public Guid StudentId { get; private set; }

    public Student Student { get; private set; } = null!;

    public Guid ClassRoomId { get; private set; }

    public ClassRoom ClassRoom { get; private set; } = null!;

    public Guid SemesterId { get; private set; }

    public Semester Semester { get; private set; } = null!;

    public EnrollmentStatus Status { get; private set; } = EnrollmentStatus.Active;

    public DateTime EnrolledAtUtc { get; private set; }

    public DateTime? EndedAtUtc { get; private set; }

    public string? EndReason { get; private set; }

    /// <summary>
    /// Ghi chú: Withdraw rút học sinh khỏi lớp bằng cách đóng enrollment hiện tại.
    /// </summary>
    public void Withdraw(string? reason, DateTime endedAtUtc)
    {
        EndedAtUtc = UtcDateTime.Require(endedAtUtc, nameof(endedAtUtc));
        EndReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        Status = EnrollmentStatus.Withdrawn;
        MarkUpdated(EndedAtUtc.Value);
    }

    /// <summary>
    /// Ghi chú: Complete đánh dấu enrollment đã hoàn tất nhưng vẫn giữ lịch sử.
    /// </summary>
    public void Complete(DateTime endedAtUtc)
    {
        EndedAtUtc = UtcDateTime.Require(endedAtUtc, nameof(endedAtUtc));
        Status = EnrollmentStatus.Completed;
        MarkUpdated(EndedAtUtc.Value);
    }
}
