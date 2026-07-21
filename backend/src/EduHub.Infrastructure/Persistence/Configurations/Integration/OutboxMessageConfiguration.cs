using EduHub.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Ghi chú: OutboxMessageConfiguration cấu hình bảng outbox messages cho event sau commit.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <summary>
    /// Ghi chú: Configure map OutboxMessage sang bảng outbox_messages.
    /// </summary>
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(message => message.Type).HasColumnName("type").HasMaxLength(256).IsRequired();
        builder.Property(message => message.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(message => message.OccurredAtUtc).HasColumnName("occurred_at_utc").IsRequired();
        builder.Property(message => message.ProcessedAtUtc).HasColumnName("processed_at_utc");
        builder.Property(message => message.RetryCount).HasColumnName("retry_count").IsRequired();

        builder.HasIndex(message => new { message.ProcessedAtUtc, message.OccurredAtUtc })
            .HasDatabaseName("ix_outbox_messages_processed_occurred");
    }
}
