using EduHub.Domain.Common;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: TimetableVersion đại diện cho một phiên bản thời khóa biểu của học kỳ để hỗ trợ nháp, chỉnh tay và công bố.
/// </summary>
public sealed class TimetableVersion : AuditableEntity
{
    private TimetableVersion()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo bản nháp thời khóa biểu mới cho học kỳ và ghi nhận người yêu cầu sinh lịch.
    /// </summary>
    public TimetableVersion(Guid semesterId, string name, Guid createdByUserId, DateTime generatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        SemesterId = semesterId;
        Name = name.Trim();
        CreatedByUserId = createdByUserId;
        GeneratedAtUtc = UtcDateTime.Require(generatedAtUtc, nameof(generatedAtUtc));
    }

    public Guid SemesterId { get; private set; }
    public Semester Semester { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Guid CreatedByUserId { get; private set; }
    public TimetableVersionStatus Status { get; private set; } = TimetableVersionStatus.Draft;
    public DateTime GeneratedAtUtc { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public ICollection<TimetableEntry> Entries { get; } = new List<TimetableEntry>();

    /// <summary>
    /// Ghi chú: Publish công bố bản thời khóa biểu để giáo viên, học sinh và phụ huynh có thể xem.
    /// </summary>
    public void Publish(DateTime publishedAtUtc)
    {
        if (Status != TimetableVersionStatus.Draft)
        {
            throw new InvalidOperationException("Only a draft timetable can be published.");
        }

        PublishedAtUtc = UtcDateTime.Require(publishedAtUtc, nameof(publishedAtUtc));
        Status = TimetableVersionStatus.Published;
        MarkUpdated(PublishedAtUtc.Value);
    }

    /// <summary>
    /// Ghi chú: Archive lưu trữ phiên bản thời khóa biểu cũ sau khi phiên bản mới được công bố.
    /// </summary>
    public void Archive(DateTime archivedAtUtc)
    {
        Status = TimetableVersionStatus.Archived;
        MarkUpdated(archivedAtUtc);
    }
}
