using EduHub.Domain.Common;
using EduHub.Domain.Entities.Identity;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: GradeChangeHistory là lịch sử append-only cho mỗi lần thay đổi điểm.
/// </summary>
public sealed class GradeChangeHistory : BaseEntity
{
    private GradeChangeHistory()
    {
    }

    private GradeChangeHistory(
        Guid gradeEntryId,
        decimal? oldScore,
        decimal newScore,
        Guid changedByUserId,
        string? reason,
        DateTime changedAtUtc)
    {
        GradeEntryId = gradeEntryId;
        OldScore = oldScore;
        NewScore = newScore;
        ChangedByUserId = changedByUserId;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ChangedAtUtc = UtcDateTime.Require(changedAtUtc, nameof(changedAtUtc));
    }

    public Guid GradeEntryId { get; private set; }

    public GradeEntry GradeEntry { get; private set; } = null!;

    public decimal? OldScore { get; private set; }

    public decimal NewScore { get; private set; }

    public Guid ChangedByUserId { get; private set; }

    public User ChangedByUser { get; private set; } = null!;

    public string? Reason { get; private set; }

    public DateTime ChangedAtUtc { get; private set; }

    /// <summary>
    /// Ghi chú: Create tạo history cho một lần đổi điểm với old/new score, actor và thời điểm.
    /// </summary>
    public static GradeChangeHistory Create(
        Guid gradeEntryId,
        decimal? oldScore,
        decimal newScore,
        Guid changedByUserId,
        string? reason,
        DateTime changedAtUtc) =>
        new(gradeEntryId, oldScore, newScore, changedByUserId, reason, changedAtUtc);
}
