using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: TeacherSubjectCapabilityConfiguration map năng lực môn chính, môn phụ và tải dạy của giáo viên.
/// </summary>
public sealed class TeacherSubjectCapabilityConfiguration : IEntityTypeConfiguration<TeacherSubjectCapability>
{
    /// <summary>
    /// Ghi chú: Configure cấu hình quan hệ giáo viên-môn và bảo đảm mỗi cặp chỉ có một năng lực.
    /// </summary>
    public void Configure(EntityTypeBuilder<TeacherSubjectCapability> builder)
    {
        builder.ToTable("teacher_subject_capabilities");
        builder.HasKey(capability => capability.Id);
        builder.Property(capability => capability.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(capability => capability.TeacherId).HasColumnName("teacher_id").IsRequired();
        builder.Property(capability => capability.SubjectId).HasColumnName("subject_id").IsRequired();
        builder.Property(capability => capability.Priority).HasColumnName("priority").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(capability => capability.MaxPeriodsPerWeek).HasColumnName("max_periods_per_week").IsRequired();
        builder.Property(capability => capability.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(capability => capability.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(capability => capability.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(capability => new { capability.TeacherId, capability.SubjectId })
            .IsUnique()
            .HasDatabaseName("ux_teacher_subject_capabilities_teacher_subject");
        builder.HasOne(capability => capability.Teacher).WithMany().HasForeignKey(capability => capability.TeacherId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(capability => capability.Subject).WithMany().HasForeignKey(capability => capability.SubjectId).OnDelete(DeleteBehavior.Restrict);
    }
}
