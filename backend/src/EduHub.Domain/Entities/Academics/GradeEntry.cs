using EduHub.Domain.Common;
using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: GradeEntry đại diện cho điểm của một học sinh ở một assignment và một grade component.
/// </summary>
public sealed class GradeEntry : AuditableEntity
{
    private GradeEntry()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo điểm Draft ban đầu cho học sinh theo assignment-component.
    /// </summary>
    public GradeEntry(Guid studentId, Guid assignmentId, Guid componentId, decimal score, Guid actorUserId, DateTime changedAtUtc)
    {
        StudentId = studentId;
        AssignmentId = assignmentId;
        ComponentId = componentId;
        Score = score;
        Status = GradeStatus.Draft;
        Version = 1;
        Histories.Add(GradeChangeHistory.Create(Id, null, score, actorUserId, null, changedAtUtc));
    }

    public Guid StudentId { get; private set; }

    public Student Student { get; private set; } = null!;

    public Guid AssignmentId { get; private set; }

    public TeachingAssignment Assignment { get; private set; } = null!;

    public Guid ComponentId { get; private set; }

    public GradeComponent Component { get; private set; } = null!;

    public decimal Score { get; private set; }

    public GradeStatus Status { get; private set; }

    public int Version { get; private set; }

    public int PublicationVersion { get; private set; }

    public DateTime? SubmittedAtUtc { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public DateTime? LockedAtUtc { get; private set; }

    public DateTime? ReopenedAtUtc { get; private set; }

    public List<GradeChangeHistory> Histories { get; private set; } = [];

    /// <summary>
    /// Ghi chú: UpdateScore cập nhật điểm Draft, kiểm tra version và tạo GradeChangeHistory.
    /// </summary>
    public void UpdateScore(decimal score, int expectedVersion, Guid actorUserId, string? reason, DateTime changedAtUtc)
    {
        if (Status is not GradeStatus.Draft)
        {
            throw new InvalidOperationException("Only Draft grade can be updated.");
        }

        if (Version != expectedVersion)
        {
            throw new InvalidOperationException("Grade version is stale.");
        }

        if (Score == score)
        {
            return;
        }

        var oldScore = Score;
        Score = score;
        Version++;
        MarkUpdated(changedAtUtc);
        Histories.Add(GradeChangeHistory.Create(Id, oldScore, score, actorUserId, reason, changedAtUtc));
    }

    /// <summary>
    /// Ghi chú: Submit chuyển điểm Draft sang Submitted.
    /// </summary>
    public void Submit(DateTime submittedAtUtc)
    {
        EnsureStatus(GradeStatus.Draft);
        Status = GradeStatus.Submitted;
        SubmittedAtUtc = UtcDateTime.Require(submittedAtUtc, nameof(submittedAtUtc));
        Version++;
        MarkUpdated(SubmittedAtUtc.Value);
    }

    /// <summary>
    /// Ghi chú: Publish chuyển điểm Submitted sang Published và tăng publication version.
    /// </summary>
    public void Publish(DateTime publishedAtUtc)
    {
        EnsureStatus(GradeStatus.Submitted);
        Status = GradeStatus.Published;
        PublishedAtUtc = UtcDateTime.Require(publishedAtUtc, nameof(publishedAtUtc));
        PublicationVersion++;
        Version++;
        MarkUpdated(PublishedAtUtc.Value);
    }

    /// <summary>
    /// Ghi chú: Reopen mở lại điểm Submitted/Published/Locked về Draft với reason bắt buộc.
    /// </summary>
    public void Reopen(string reason, DateTime reopenedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (Status is GradeStatus.Draft)
        {
            throw new InvalidOperationException("Draft grade cannot be reopened.");
        }

        Status = GradeStatus.Draft;
        ReopenedAtUtc = UtcDateTime.Require(reopenedAtUtc, nameof(reopenedAtUtc));
        Version++;
        MarkUpdated(ReopenedAtUtc.Value);
    }

    /// <summary>
    /// Ghi chú: Lock chuyển điểm Published sang Locked.
    /// </summary>
    public void Lock(DateTime lockedAtUtc)
    {
        EnsureStatus(GradeStatus.Published);
        Status = GradeStatus.Locked;
        LockedAtUtc = UtcDateTime.Require(lockedAtUtc, nameof(lockedAtUtc));
        Version++;
        MarkUpdated(LockedAtUtc.Value);
    }

    private void EnsureStatus(GradeStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidOperationException($"Grade status must be {expectedStatus}.");
        }
    }
}
