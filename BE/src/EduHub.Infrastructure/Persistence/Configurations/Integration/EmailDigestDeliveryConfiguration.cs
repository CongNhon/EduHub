using EduHub.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Ghi chú: EmailDigestDeliveryConfiguration cấu hình bảng chống gửi trùng fake email digest.
/// </summary>
public sealed class EmailDigestDeliveryConfiguration : IEntityTypeConfiguration<EmailDigestDelivery>
{
    /// <summary>
    /// Ghi chú: Configure map EmailDigestDelivery sang bảng email_digest_deliveries.
    /// </summary>
    public void Configure(EntityTypeBuilder<EmailDigestDelivery> builder)
    {
        builder.ToTable("email_digest_deliveries");
        builder.HasKey(delivery => delivery.Id);
        builder.Property(delivery => delivery.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(delivery => delivery.RecipientUserId).HasColumnName("recipient_user_id").IsRequired();
        builder.Property(delivery => delivery.RecipientEmail).HasColumnName("recipient_email").HasMaxLength(320).IsRequired();
        builder.Property(delivery => delivery.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(256).IsRequired();
        builder.Property(delivery => delivery.PeriodStartUtc).HasColumnName("period_start_utc").IsRequired();
        builder.Property(delivery => delivery.PeriodEndUtc).HasColumnName("period_end_utc").IsRequired();
        builder.Property(delivery => delivery.TemplateVersion).HasColumnName("template_version").HasMaxLength(64).IsRequired();
        builder.Property(delivery => delivery.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(delivery => delivery.AttemptCount).HasColumnName("attempt_count").IsRequired();
        builder.Property(delivery => delivery.LastAttemptAtUtc).HasColumnName("last_attempt_at_utc");
        builder.Property(delivery => delivery.SentAtUtc).HasColumnName("sent_at_utc");
        builder.Property(delivery => delivery.LastError).HasColumnName("last_error").HasMaxLength(2048);

        builder.HasIndex(delivery => delivery.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("ux_email_digest_deliveries_idempotency_key");

        builder.HasOne(delivery => delivery.RecipientUser)
            .WithMany()
            .HasForeignKey(delivery => delivery.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
