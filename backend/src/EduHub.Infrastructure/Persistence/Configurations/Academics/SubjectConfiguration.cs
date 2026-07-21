using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: SubjectConfiguration cấu hình mapping/constraint/index cho môn học.
/// </summary>
public sealed class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    /// <summary>
    /// Ghi chú: Configure map môn học sang table/column/index/constraint trong EF Core hoặc DI.
    /// </summary>
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects", table =>
        {
            table.HasCheckConstraint("ck_subjects_credits_positive", "credits > 0");
            table.HasCheckConstraint("ck_subjects_max_score_positive", "max_score > 0");
        });

        builder.HasKey(subject => subject.Id);
        builder.Property(subject => subject.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(subject => subject.SubjectCode).HasColumnName("subject_code").HasMaxLength(64).IsRequired();
        builder.Property(subject => subject.NormalizedSubjectCode).HasColumnName("normalized_subject_code").HasMaxLength(64).IsRequired();
        builder.Property(subject => subject.Name).HasColumnName("name").HasMaxLength(256).IsRequired();
        builder.Property(subject => subject.Credits).HasColumnName("credits").IsRequired();
        builder.Property(subject => subject.MaxScore).HasColumnName("max_score").HasPrecision(5, 2).IsRequired();
        builder.Property(subject => subject.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(subject => subject.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(subject => subject.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(subject => subject.NormalizedSubjectCode)
            .IsUnique()
            .HasDatabaseName("ux_subjects_normalized_subject_code");
    }
}
