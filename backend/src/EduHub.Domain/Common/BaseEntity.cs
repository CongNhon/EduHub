using System.ComponentModel.DataAnnotations.Schema;

namespace EduHub.Domain.Common;

/// <summary>
/// Ghi chú: BaseEntity đại diện cho entity gốc có Id và domain events trong hệ thống EduHub.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<DomainEvent> _domainEvents = [];

    public Guid Id { get; protected set; } = Guid.NewGuid();

    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Ghi chú: AddDomainEvent thực hiện phần xử lý của entity gốc có Id và domain events.
    /// </summary>
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Ghi chú: ClearDomainEvents thực hiện phần xử lý của entity gốc có Id và domain events.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
