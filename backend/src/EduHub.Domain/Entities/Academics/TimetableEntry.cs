using EduHub.Domain.Common;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: TimetableEntry đại diện cho một tiết học cụ thể của lớp trong một tuần thực học của học kỳ.
/// </summary>
public sealed class TimetableEntry : AuditableEntity
{
    private TimetableEntry()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo một tiết học và kiểm tra số tuần, ngày, buổi cùng số tiết hợp lệ.
    /// </summary>
    public TimetableEntry(
        Guid timetableVersionId,
        Guid classRoomId,
        Guid subjectId,
        Guid? teacherId,
        int weekNumber,
        int dayOfWeek,
        TimetableSession session,
        int periodNumber,
        TimetableEntryKind kind,
        bool countsTowardQuota,
        bool isLocked,
        string? note = null)
    {
        ValidateSlot(weekNumber, dayOfWeek, periodNumber);

        TimetableVersionId = timetableVersionId;
        ClassRoomId = classRoomId;
        SubjectId = subjectId;
        TeacherId = teacherId;
        WeekNumber = weekNumber;
        DayOfWeek = dayOfWeek;
        Session = session;
        PeriodNumber = periodNumber;
        Kind = kind;
        CountsTowardQuota = countsTowardQuota;
        IsLocked = isLocked;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    public Guid TimetableVersionId { get; private set; }
    public TimetableVersion TimetableVersion { get; private set; } = null!;
    public Guid ClassRoomId { get; private set; }
    public ClassRoom ClassRoom { get; private set; } = null!;
    public Guid SubjectId { get; private set; }
    public Subject Subject { get; private set; } = null!;
    public Guid? TeacherId { get; private set; }
    public User? Teacher { get; private set; }
    public int WeekNumber { get; private set; }
    public int DayOfWeek { get; private set; }
    public TimetableSession Session { get; private set; }
    public int PeriodNumber { get; private set; }
    public TimetableEntryKind Kind { get; private set; }
    public bool CountsTowardQuota { get; private set; }
    public bool IsLocked { get; private set; }
    public string? Note { get; private set; }

    /// <summary>
    /// Ghi chú: Move chuyển tiết học nháp sang slot khác khi quản trị học vụ chỉnh thời khóa biểu bằng tay.
    /// </summary>
    public void Move(int weekNumber, int dayOfWeek, TimetableSession session, int periodNumber, DateTime updatedAtUtc)
    {
        if (IsLocked)
        {
            throw new InvalidOperationException("A locked timetable entry cannot be moved.");
        }

        ValidateSlot(weekNumber, dayOfWeek, periodNumber);
        WeekNumber = weekNumber;
        DayOfWeek = dayOfWeek;
        Session = session;
        PeriodNumber = periodNumber;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: AssignTeacher đổi giáo viên phụ trách một tiết cụ thể sau khi đã kiểm tra năng lực và xung đột lịch.
    /// </summary>
    public void AssignTeacher(Guid teacherId, DateTime updatedAtUtc)
    {
        if (IsLocked)
        {
            throw new InvalidOperationException("A locked timetable entry cannot change teacher.");
        }

        TeacherId = teacherId;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: SetLocked khóa hoặc mở khóa tiết học để lần sinh lịch tiếp theo giữ nguyên slot đã chỉnh tay.
    /// </summary>
    public void SetLocked(bool isLocked, DateTime updatedAtUtc)
    {
        IsLocked = isLocked;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: ValidateSlot bảo đảm tuần, ngày và số tiết của một slot thời khóa biểu luôn nằm trong giới hạn hệ thống.
    /// </summary>
    private static void ValidateSlot(int weekNumber, int dayOfWeek, int periodNumber)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(weekNumber, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(weekNumber, 40);
        ArgumentOutOfRangeException.ThrowIfLessThan(dayOfWeek, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dayOfWeek, 6);
        ArgumentOutOfRangeException.ThrowIfLessThan(periodNumber, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(periodNumber, 5);
    }
}
