using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: CurriculumPlanConfiguration map chương trình học theo khối và năm học sang PostgreSQL.
/// </summary>
public sealed class CurriculumPlanConfiguration : IEntityTypeConfiguration<CurriculumPlan>
{
    /// <summary>
    /// Ghi chú: Configure cấu hình số tuần, khối lớp và quan hệ quota môn học của chương trình.
    /// </summary>
    public void Configure(EntityTypeBuilder<CurriculumPlan> builder)
    {
        builder.ToTable("curriculum_plans");
        builder.HasKey(plan => plan.Id);
        builder.Property(plan => plan.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(plan => plan.AcademicYearId).HasColumnName("academic_year_id").IsRequired();
        builder.Property(plan => plan.GradeLevel).HasColumnName("grade_level").IsRequired();
        builder.Property(plan => plan.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
        builder.Property(plan => plan.TotalWeeks).HasColumnName("total_weeks").IsRequired();
        builder.Property(plan => plan.Semester1Weeks).HasColumnName("semester_1_weeks").IsRequired();
        builder.Property(plan => plan.Semester2Weeks).HasColumnName("semester_2_weeks").IsRequired();
        builder.Property(plan => plan.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(plan => plan.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(plan => plan.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(plan => new { plan.AcademicYearId, plan.GradeLevel })
            .IsUnique()
            .HasDatabaseName("ux_curriculum_plans_year_grade");
        builder.HasOne(plan => plan.AcademicYear).WithMany().HasForeignKey(plan => plan.AcademicYearId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(plan => plan.SubjectQuotas).WithOne(quota => quota.CurriculumPlan).HasForeignKey(quota => quota.CurriculumPlanId).OnDelete(DeleteBehavior.Cascade);
    }
}
