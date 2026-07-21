using EduHub.Domain.Common;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Entities.Integration;

namespace EduHub.Domain.Entities.Notifications;

/// <summary>
/// Ghi chú: Notification lưu thông báo bền vững cho một user nhận được từ outbox event.
/// </summary>
public sealed class Notification : BaseEntity
{
    private Notification()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo notification cho recipient cụ thể sau khi outbox event được xử lý.
    /// </summary>
    public Notification(
        Guid recipientUserId,
        Guid outboxMessageId,
        string type,
        string title,
        string body,
        Guid? studentId,
        Guid? assignmentId,
        DateTime occurredAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        RecipientUserId = recipientUserId;
        OutboxMessageId = outboxMessageId;
        Type = type.Trim();
        Title = title.Trim();
        Body = body.Trim();
        StudentId = studentId;
        AssignmentId = assignmentId;
        OccurredAtUtc = UtcDateTime.Require(occurredAtUtc, nameof(occurredAtUtc));
    }

    public Guid RecipientUserId { get; private set; }

    public User RecipientUser { get; private set; } = null!;

    public Guid OutboxMessageId { get; private set; }

    public OutboxMessage OutboxMessage { get; private set; } = null!;

    public string Type { get; private set; } = null!;

    public string Title { get; private set; } = null!;

    public string Body { get; private set; } = null!;

    public Guid? StudentId { get; private set; }

    public Guid? AssignmentId { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    public DateTime? ReadAtUtc { get; private set; }

    public bool IsRead => ReadAtUtc.HasValue;

    /// <summary>
    /// Ghi chú: MarkRead đánh dấu notification của user hiện tại đã được đọc.
    /// </summary>
    public void MarkRead(DateTime readAtUtc)
    {
        ReadAtUtc ??= UtcDateTime.Require(readAtUtc, nameof(readAtUtc));
    }
}
