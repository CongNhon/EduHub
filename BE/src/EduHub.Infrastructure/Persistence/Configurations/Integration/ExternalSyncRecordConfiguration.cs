using EduHub.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Ghi chú: ExternalSyncRecordConfiguration cấu hình bảng external sync records cho Ministry API.
/// </summary>
public sealed class ExternalSyncRecordConfiguration : IEntityTypeConfiguration<ExternalSyncRecord>
{
    /// <summary>
    /// Ghi chú: Configure map ExternalSyncRecord sang bảng external_sync_records.
    /// </summary>
    public void Configure(EntityTypeBuilder<ExternalSyncRecord> builder)
    {
        builder.ToTable("external_sync_records");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(record => record.AggregateType).HasColumnName("aggregate_type").HasMaxLength(128).IsRequired();
        builder.Property(record => record.AggregateId).HasColumnName("aggregate_id").IsRequired();
        builder.Property(record => record.Version).HasColumnName("version").IsRequired();
        builder.Property(record => record.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(128).IsRequired();
        builder.Property(record => record.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(record => record.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(record => record.Attempts).HasColumnName("attempts").IsRequired();
        builder.Property(record => record.ExternalId).HasColumnName("external_id").HasMaxLength(128);
        builder.Property(record => record.ExternalVersion).HasColumnName("external_version").HasMaxLength(64);
        builder.Property(record => record.LastError).HasColumnName("last_error").HasMaxLength(2048);
        builder.Property(record => record.NextRetryAtUtc).HasColumnName("next_retry_at_utc");
        builder.Property(record => record.SucceededAtUtc).HasColumnName("succeeded_at_utc");
        builder.Property(record => record.LastManualRetryByUserId).HasColumnName("last_manual_retry_by_user_id");
        builder.Property(record => record.LastManualRetryReason).HasColumnName("last_manual_retry_reason").HasMaxLength(512);
        builder.Property(record => record.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(record => record.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(record => new { record.AggregateId, record.Version })
            .IsUnique()
            .HasDatabaseName("ux_external_sync_records_aggregate_version");

        builder.HasIndex(record => new { record.Status, record.NextRetryAtUtc })
            .HasDatabaseName("ix_external_sync_records_status_next_retry");

        builder.HasOne(record => record.LastManualRetryByUser)
            .WithMany()
            .HasForeignKey(record => record.LastManualRetryByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
