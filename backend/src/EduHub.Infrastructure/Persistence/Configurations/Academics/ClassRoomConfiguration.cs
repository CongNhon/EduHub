using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: ClassRoomConfiguration cấu hình bảng class_rooms cho lớp học.
/// </summary>
public sealed class ClassRoomConfiguration : IEntityTypeConfiguration<ClassRoom>
{
    /// <summary>
    /// Ghi chú: Configure map ClassRoom sang bảng class_rooms, unique class code theo năm học và check capacity.
    /// </summary>
    public void Configure(EntityTypeBuilder<ClassRoom> builder)
    {
        builder.ToTable("class_rooms", table =>
        {
            table.HasCheckConstraint("ck_class_rooms_capacity_positive", "capacity > 0");
            table.HasCheckConstraint("ck_class_rooms_active_count_valid", "active_enrollment_count >= 0 AND active_enrollment_count <= capacity");
            table.HasCheckConstraint("ck_class_rooms_grade_level_positive", "grade_level > 0");
        });

        builder.HasKey(classRoom => classRoom.Id);
        builder.Property(classRoom => classRoom.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(classRoom => classRoom.ClassCode).HasColumnName("class_code").HasMaxLength(64).IsRequired();
        builder.Property(classRoom => classRoom.NormalizedClassCode).HasColumnName("normalized_class_code").HasMaxLength(64).IsRequired();
        builder.Property(classRoom => classRoom.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(classRoom => classRoom.AcademicYearId).HasColumnName("academic_year_id").IsRequired();
        builder.Property(classRoom => classRoom.GradeLevel).HasColumnName("grade_level").IsRequired();
        builder.Property(classRoom => classRoom.Capacity).HasColumnName("capacity").IsRequired();
        builder.Property(classRoom => classRoom.ActiveEnrollmentCount).HasColumnName("active_enrollment_count").IsRequired();
        builder.Property(classRoom => classRoom.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(classRoom => classRoom.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(classRoom => classRoom.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(classRoom => new { classRoom.AcademicYearId, classRoom.NormalizedClassCode })
            .IsUnique()
            .HasDatabaseName("ux_class_rooms_academic_year_id_normalized_class_code");

        builder.HasOne(classRoom => classRoom.AcademicYear)
            .WithMany()
            .HasForeignKey(classRoom => classRoom.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
