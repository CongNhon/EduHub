using EduHub.Domain.Common;
using EduHub.Domain.Entities.Identity;

namespace EduHub.Domain.Entities.Students;

/// <summary>
/// Ghi chú: ParentStudent đại diện cho liên kết phụ huynh-học sinh trong hệ thống EduHub.
/// </summary>
public sealed class ParentStudent : AuditableEntity
{
    private ParentStudent()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor khởi tạo liên kết phụ huynh-học sinh và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public ParentStudent(Guid parentUserId, Guid studentId, string relationship, DateTime effectiveFromUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relationship);

        ParentUserId = parentUserId;
        StudentId = studentId;
        Relationship = relationship.Trim();
        EffectiveFromUtc = UtcDateTime.Require(effectiveFromUtc, nameof(effectiveFromUtc));
    }

    public Guid ParentUserId { get; private set; }

    public User ParentUser { get; private set; } = null!;

    public Guid StudentId { get; private set; }

    public Student Student { get; private set; } = null!;

    public string Relationship { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public DateTime EffectiveFromUtc { get; private set; }

    public DateTime? EffectiveToUtc { get; private set; }

    public DateTime? DeactivatedAtUtc { get; private set; }

    /// <summary>
    /// Ghi chú: Reactivate thực hiện phần xử lý của liên kết phụ huynh-học sinh.
    /// </summary>
    public void Reactivate(string relationship, DateTime effectiveFromUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relationship);

        Relationship = relationship.Trim();
        EffectiveFromUtc = UtcDateTime.Require(effectiveFromUtc, nameof(effectiveFromUtc));
        EffectiveToUtc = null;
        DeactivatedAtUtc = null;
        IsActive = true;
        MarkUpdated(EffectiveFromUtc);
    }

    /// <summary>
    /// Ghi chú: Deactivate thực hiện phần xử lý của liên kết phụ huynh-học sinh.
    /// </summary>
    public void Deactivate(DateTime deactivatedAtUtc)
    {
        var utcNow = UtcDateTime.Require(deactivatedAtUtc, nameof(deactivatedAtUtc));
        IsActive = false;
        EffectiveToUtc = utcNow;
        DeactivatedAtUtc = utcNow;
        MarkUpdated(utcNow);
    }
}
