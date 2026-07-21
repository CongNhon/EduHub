using EduHub.Domain.Common;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Entities.Students;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: StudentRemark lưu nhận xét môn học của giáo viên cho một học sinh trong teaching assignment.
/// </summary>
public sealed class StudentRemark : AuditableEntity
{
    private StudentRemark() { }

    /// <summary>
    /// Ghi chú: Constructor tạo nhận xét Draft để giáo viên chỉnh sửa trước khi sổ điểm được công bố.
    /// </summary>
    public StudentRemark(Guid studentId, Guid assignmentId, Guid teacherId, string content, DateTime createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        StudentId = studentId;
        AssignmentId = assignmentId;
        TeacherId = teacherId;
        Content = content.Trim();
        Version = 1;
        CreatedAtUtc = UtcDateTime.Require(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid StudentId { get; private set; }
    public Student Student { get; private set; } = null!;
    public Guid AssignmentId { get; private set; }
    public TeachingAssignment Assignment { get; private set; } = null!;
    public Guid TeacherId { get; private set; }
    public User Teacher { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public bool IsPublished { get; private set; }
    public int Version { get; private set; }

    /// <summary>
    /// Ghi chú: UpdateContent sửa nhận xét Draft và kiểm tra optimistic concurrency theo version.
    /// </summary>
    public void UpdateContent(string content, int expectedVersion, DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        if (IsPublished) throw new InvalidOperationException("Published remark must be reopened before editing.");
        if (Version != expectedVersion) throw new InvalidOperationException("Remark version is stale.");
        Content = content.Trim();
        Version++;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: Publish cho phép phụ huynh và học sinh xem nhận xét cùng điểm đã công bố.
    /// </summary>
    public void Publish(DateTime updatedAtUtc)
    {
        IsPublished = true;
        Version++;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: Reopen đưa nhận xét về Draft để giáo viên tiếp tục chỉnh sửa.
    /// </summary>
    public void Reopen(DateTime updatedAtUtc)
    {
        IsPublished = false;
        Version++;
        MarkUpdated(updatedAtUtc);
    }
}
