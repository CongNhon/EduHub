using EduHub.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Notifications;

/// <summary>
/// Ghi chú: NotificationConfiguration cấu hình bảng notifications lưu thông báo của từng user.
/// </summary>
public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    /// <summary>
    /// Ghi chú: Configure map Notification sang bảng notifications với unique idempotency key.
    /// </summary>
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(notification => notification.Id);
        builder.Property(notification => notification.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(notification => notification.RecipientUserId).HasColumnName("recipient_user_id").IsRequired();
        builder.Property(notification => notification.OutboxMessageId).HasColumnName("outbox_message_id").IsRequired();
        builder.Property(notification => notification.Type).HasColumnName("type").HasMaxLength(128).IsRequired();
        builder.Property(notification => notification.Title).HasColumnName("title").HasMaxLength(256).IsRequired();
        builder.Property(notification => notification.Body).HasColumnName("body").HasMaxLength(1024).IsRequired();
        builder.Property(notification => notification.StudentId).HasColumnName("student_id");
        builder.Property(notification => notification.AssignmentId).HasColumnName("assignment_id");
        builder.Property(notification => notification.OccurredAtUtc).HasColumnName("occurred_at_utc").IsRequired();
        builder.Property(notification => notification.ReadAtUtc).HasColumnName("read_at_utc");
        builder.Ignore(notification => notification.IsRead);

        builder.HasIndex(notification => new { notification.RecipientUserId, notification.OccurredAtUtc })
            .HasDatabaseName("ix_notifications_recipient_occurred");

        builder.HasIndex(notification => new { notification.OutboxMessageId, notification.RecipientUserId })
            .IsUnique()
            .HasDatabaseName("ux_notifications_outbox_recipient");

        builder.HasOne(notification => notification.RecipientUser)
            .WithMany()
            .HasForeignKey(notification => notification.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(notification => notification.OutboxMessage)
            .WithMany()
            .HasForeignKey(notification => notification.OutboxMessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
