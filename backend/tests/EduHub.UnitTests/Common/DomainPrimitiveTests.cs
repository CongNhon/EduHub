using EduHub.Domain.Common;
using EduHub.Domain.Exceptions;

namespace EduHub.UnitTests.Common;

public sealed class DomainPrimitiveTests
{
    [Fact]
    public void NewEntity_UsesServerGeneratedUuidAndUtcCreatedTimestamp()
    {
        var entity = new TestEntity();

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(DateTimeKind.Utc, entity.CreatedAtUtc.Kind);
    }

    [Fact]
    public void Entity_TracksAndClearsDomainEvents()
    {
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent(DateTime.UtcNow);

        entity.Record(domainEvent);

        Assert.Contains(domainEvent, entity.DomainEvents);

        entity.ClearDomainEvents();

        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void MarkUpdated_RejectsNonUtcTimestamp()
    {
        var entity = new TestEntity();

        var exception = Assert.Throws<DomainException>(() => entity.Touch(DateTime.Now));

        Assert.Equal("timestamp.not_utc", exception.Code);
    }

    private sealed class TestEntity : AuditableEntity
    {
        public void Record(DomainEvent domainEvent) => AddDomainEvent(domainEvent);

        public void Touch(DateTime updatedAtUtc) => MarkUpdated(updatedAtUtc);
    }

    private sealed record TestDomainEvent(DateTime Timestamp) : DomainEvent(Timestamp);
}
