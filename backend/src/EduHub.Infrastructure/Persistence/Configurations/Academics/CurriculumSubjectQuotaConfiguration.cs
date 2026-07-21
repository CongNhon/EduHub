using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: CurriculumSubjectQuotaConfiguration map quota tiết của từng môn trong chương trình học.
/// </summary>
public sealed class CurriculumSubjectQuotaConfiguration : IEntityTypeConfiguration<CurriculumSubjectQuota>
{
    /// <summary>
    /// Ghi chú: Configure cấu hình quota năm, từng học kỳ và quy tắc tiết đôi của môn học.
    /// </summary>
    public void Configure(EntityTypeBuilder<CurriculumSubjectQuota> builder)
    {
        builder.ToTable("curriculum_subject_quotas");
        builder.HasKey(quota => quota.Id);
        builder.Property(quota => quota.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(quota => quota.CurriculumPlanId).HasColumnName("curriculum_plan_id").IsRequired();
        builder.Property(quota => quota.SubjectId).HasColumnName("subject_id").IsRequired();
        builder.Property(quota => quota.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(quota => quota.AnnualPeriods).HasColumnName("annual_periods").IsRequired();
        builder.Property(quota => quota.Semester1Periods).HasColumnName("semester_1_periods").IsRequired();
        builder.Property(quota => quota.Semester2Periods).HasColumnName("semester_2_periods").IsRequired();
        builder.Property(quota => quota.CanDoublePeriod).HasColumnName("can_double_period").IsRequired();
        builder.Property(quota => quota.MaxPeriodsPerDay).HasColumnName("max_periods_per_day").IsRequired();
        builder.Property(quota => quota.IncludesHomeroom).HasColumnName("includes_homeroom").IsRequired();
        builder.Property(quota => quota.PreferredSession).HasColumnName("preferred_session").HasConversion<string>().HasMaxLength(16);
        builder.Property(quota => quota.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(quota => quota.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(quota => new { quota.CurriculumPlanId, quota.SubjectId })
            .IsUnique()
            .HasDatabaseName("ux_curriculum_subject_quotas_plan_subject");
        builder.HasOne(quota => quota.Subject).WithMany().HasForeignKey(quota => quota.SubjectId).OnDelete(DeleteBehavior.Restrict);
    }
}
