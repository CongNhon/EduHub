namespace EduHub.Domain.Common;

/// <summary>
/// Ghi chú: DomainEvent đại diện cho sự kiện phát sinh trong domain trong hệ thống EduHub.
/// </summary>
public abstract record DomainEvent
{
    protected DomainEvent(DateTime occurredAtUtc)
    {
        OccurredAtUtc = UtcDateTime.Require(occurredAtUtc, nameof(occurredAtUtc));
    }

    public DateTime OccurredAtUtc { get; }
}
