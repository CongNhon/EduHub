using EduHub.Domain.Common;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: TeacherSubjectCapability mô tả môn chính hoặc môn phụ và tải dạy tối đa của một giáo viên.
/// </summary>
public sealed class TeacherSubjectCapability : AuditableEntity
{
    private TeacherSubjectCapability()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor gắn giáo viên với môn có thể dạy để hệ thống tự động phân công.
    /// </summary>
    public TeacherSubjectCapability(
        Guid teacherId,
        Guid subjectId,
        TeacherSubjectPriority priority,
        int maxPeriodsPerWeek)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxPeriodsPerWeek, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxPeriodsPerWeek, 35);
        TeacherId = teacherId;
        SubjectId = subjectId;
        Priority = priority;
        MaxPeriodsPerWeek = maxPeriodsPerWeek;
    }

    public Guid TeacherId { get; private set; }
    public User Teacher { get; private set; } = null!;
    public Guid SubjectId { get; private set; }
    public Subject Subject { get; private set; } = null!;
    public TeacherSubjectPriority Priority { get; private set; }
    public int MaxPeriodsPerWeek { get; private set; }
    public bool IsActive { get; private set; } = true;
}
