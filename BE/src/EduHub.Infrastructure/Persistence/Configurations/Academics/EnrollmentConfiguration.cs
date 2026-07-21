using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: EnrollmentConfiguration cấu hình bảng enrollments cho ghi danh học sinh vào lớp.
/// </summary>
public sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    /// <summary>
    /// Ghi chú: Configure map Enrollment sang bảng enrollments, unique active enrollment và FK liên quan.
    /// </summary>
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("enrollments");

        builder.HasKey(enrollment => enrollment.Id);
        builder.Property(enrollment => enrollment.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(enrollment => enrollment.StudentId).HasColumnName("student_id").IsRequired();
        builder.Property(enrollment => enrollment.ClassRoomId).HasColumnName("class_room_id").IsRequired();
        builder.Property(enrollment => enrollment.SemesterId).HasColumnName("semester_id").IsRequired();
        builder.Property(enrollment => enrollment.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(enrollment => enrollment.EnrolledAtUtc).HasColumnName("enrolled_at_utc").IsRequired();
        builder.Property(enrollment => enrollment.EndedAtUtc).HasColumnName("ended_at_utc");
        builder.Property(enrollment => enrollment.EndReason).HasColumnName("end_reason").HasMaxLength(512);
        builder.Property(enrollment => enrollment.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(enrollment => enrollment.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(enrollment => new { enrollment.SemesterId, enrollment.StudentId })
            .IsUnique()
            .HasFilter("status = 'Active'")
            .HasDatabaseName("ux_enrollments_active_semester_student");

        builder.HasIndex(enrollment => enrollment.ClassRoomId)
            .HasDatabaseName("ix_enrollments_class_room_id");

        builder.HasIndex(enrollment => enrollment.StudentId)
            .HasDatabaseName("ix_enrollments_student_id");

        builder.HasIndex(enrollment => new { enrollment.SemesterId, enrollment.ClassRoomId, enrollment.StudentId })
            .IsUnique()
            .HasDatabaseName("ux_enrollments_semester_class_room_student");

        builder.HasOne(enrollment => enrollment.Student)
            .WithMany()
            .HasForeignKey(enrollment => enrollment.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(enrollment => enrollment.ClassRoom)
            .WithMany()
            .HasForeignKey(enrollment => enrollment.ClassRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(enrollment => enrollment.Semester)
            .WithMany()
            .HasForeignKey(enrollment => enrollment.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
