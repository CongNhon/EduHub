using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: GradeChangeHistoryConfiguration cấu hình bảng lịch sử đổi điểm append-only.
/// </summary>
public sealed class GradeChangeHistoryConfiguration : IEntityTypeConfiguration<GradeChangeHistory>
{
    /// <summary>
    /// Ghi chú: Configure map GradeChangeHistory sang bảng grade_change_histories.
    /// </summary>
    public void Configure(EntityTypeBuilder<GradeChangeHistory> builder)
    {
        builder.ToTable("grade_change_histories");
        builder.HasKey(history => history.Id);
        builder.Property(history => history.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(history => history.GradeEntryId).HasColumnName("grade_entry_id").IsRequired();
        builder.Property(history => history.OldScore).HasColumnName("old_score").HasPrecision(5, 2);
        builder.Property(history => history.NewScore).HasColumnName("new_score").HasPrecision(5, 2).IsRequired();
        builder.Property(history => history.ChangedByUserId).HasColumnName("changed_by_user_id").IsRequired();
        builder.Property(history => history.Reason).HasColumnName("reason").HasMaxLength(512);
        builder.Property(history => history.ChangedAtUtc).HasColumnName("changed_at_utc").IsRequired();

        builder.HasIndex(history => history.GradeEntryId).HasDatabaseName("ix_grade_change_histories_grade_entry_id");
        builder.HasIndex(history => history.ChangedByUserId).HasDatabaseName("ix_grade_change_histories_changed_by_user_id");

        builder.HasOne(history => history.ChangedByUser)
            .WithMany()
            .HasForeignKey(history => history.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
