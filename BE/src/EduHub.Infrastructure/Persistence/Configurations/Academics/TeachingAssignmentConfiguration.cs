using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: TeachingAssignmentConfiguration cấu hình bảng teaching_assignments cho phân công giáo viên.
/// </summary>
public sealed class TeachingAssignmentConfiguration : IEntityTypeConfiguration<TeachingAssignment>
{
    /// <summary>
    /// Ghi chú: Configure map TeachingAssignment sang bảng teaching_assignments và unique assignment active.
    /// </summary>
    public void Configure(EntityTypeBuilder<TeachingAssignment> builder)
    {
        builder.ToTable("teaching_assignments");

        builder.HasKey(assignment => assignment.Id);
        builder.Property(assignment => assignment.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(assignment => assignment.ClassRoomId).HasColumnName("class_room_id").IsRequired();
        builder.Property(assignment => assignment.SubjectId).HasColumnName("subject_id").IsRequired();
        builder.Property(assignment => assignment.TeacherId).HasColumnName("teacher_id").IsRequired();
        builder.Property(assignment => assignment.SemesterId).HasColumnName("semester_id").IsRequired();
        builder.Property(assignment => assignment.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(assignment => assignment.AssignedAtUtc).HasColumnName("assigned_at_utc").IsRequired();
        builder.Property(assignment => assignment.EndedAtUtc).HasColumnName("ended_at_utc");
        builder.Property(assignment => assignment.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(assignment => assignment.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(assignment => new { assignment.SemesterId, assignment.ClassRoomId, assignment.SubjectId })
            .IsUnique()
            .HasFilter("is_active = true")
            .HasDatabaseName("ux_teaching_assignments_active_scope");

        builder.HasIndex(assignment => assignment.ClassRoomId)
            .HasDatabaseName("ix_teaching_assignments_class_room_id");

        builder.HasIndex(assignment => assignment.SubjectId)
            .HasDatabaseName("ix_teaching_assignments_subject_id");

        builder.HasIndex(assignment => assignment.TeacherId)
            .HasDatabaseName("ix_teaching_assignments_teacher_id");

        builder.HasOne(assignment => assignment.ClassRoom)
            .WithMany()
            .HasForeignKey(assignment => assignment.ClassRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.Subject)
            .WithMany()
            .HasForeignKey(assignment => assignment.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.Semester)
            .WithMany()
            .HasForeignKey(assignment => assignment.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.Teacher)
            .WithMany()
            .HasForeignKey(assignment => assignment.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
