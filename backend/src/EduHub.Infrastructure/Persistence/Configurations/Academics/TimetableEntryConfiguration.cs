using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: TimetableEntryConfiguration map từng tiết học và các unique constraint chống trùng lớp hoặc giáo viên.
/// </summary>
public sealed class TimetableEntryConfiguration : IEntityTypeConfiguration<TimetableEntry>
{
    /// <summary>
    /// Ghi chú: Configure cấu hình số tuần thực tế, ngày, buổi, tiết, lớp, môn và giáo viên của từng tiết học.
    /// </summary>
    public void Configure(EntityTypeBuilder<TimetableEntry> builder)
    {
        builder.ToTable("timetable_entries", tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "ck_timetable_entries_period_number",
                "period_number BETWEEN 1 AND 5"));
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(entry => entry.TimetableVersionId).HasColumnName("timetable_version_id").IsRequired();
        builder.Property(entry => entry.ClassRoomId).HasColumnName("class_room_id").IsRequired();
        builder.Property(entry => entry.SubjectId).HasColumnName("subject_id").IsRequired();
        builder.Property(entry => entry.TeacherId).HasColumnName("teacher_id");
        builder.Property(entry => entry.WeekNumber).HasColumnName("week_number").IsRequired();
        builder.Property(entry => entry.DayOfWeek).HasColumnName("day_of_week").IsRequired();
        builder.Property(entry => entry.Session).HasColumnName("session").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(entry => entry.PeriodNumber).HasColumnName("period_number").IsRequired();
        builder.Property(entry => entry.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(entry => entry.CountsTowardQuota).HasColumnName("counts_toward_quota").IsRequired();
        builder.Property(entry => entry.IsLocked).HasColumnName("is_locked").IsRequired();
        builder.Property(entry => entry.Note).HasColumnName("note").HasMaxLength(500);
        builder.Property(entry => entry.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(entry => entry.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(entry => new { entry.TimetableVersionId, entry.ClassRoomId, entry.WeekNumber, entry.DayOfWeek, entry.Session, entry.PeriodNumber })
            .IsUnique()
            .HasDatabaseName("ux_timetable_entries_class_slot");
        builder.HasIndex(entry => new { entry.TimetableVersionId, entry.TeacherId, entry.WeekNumber, entry.DayOfWeek, entry.Session, entry.PeriodNumber })
            .IsUnique()
            .HasFilter("teacher_id IS NOT NULL")
            .HasDatabaseName("ux_timetable_entries_teacher_slot");
        builder.HasOne(entry => entry.ClassRoom).WithMany().HasForeignKey(entry => entry.ClassRoomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entry => entry.Subject).WithMany().HasForeignKey(entry => entry.SubjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entry => entry.Teacher).WithMany().HasForeignKey(entry => entry.TeacherId).OnDelete(DeleteBehavior.Restrict);
    }
}
