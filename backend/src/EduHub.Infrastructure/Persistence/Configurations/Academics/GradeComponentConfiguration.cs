using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: GradeComponentConfiguration cấu hình table, index và constraint cho thành phần điểm.
/// </summary>
public sealed class GradeComponentConfiguration : IEntityTypeConfiguration<GradeComponent>
{
    /// <summary>
    /// Ghi chú: Configure map GradeComponent sang bảng grade_components.
    /// </summary>
    public void Configure(EntityTypeBuilder<GradeComponent> builder)
    {
        builder.ToTable("grade_components", table =>
        {
            table.HasCheckConstraint("ck_grade_components_weight_range", "weight > 0 AND weight <= 1");
            table.HasCheckConstraint("ck_grade_components_max_score_positive", "max_score > 0");
            table.HasCheckConstraint("ck_grade_components_display_order_positive", "display_order > 0");
            table.HasCheckConstraint("ck_grade_components_version_positive", "version > 0");
        });

        builder.HasKey(component => component.Id);
        builder.Property(component => component.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(component => component.SubjectId).HasColumnName("subject_id").IsRequired();
        builder.Property(component => component.SemesterId).HasColumnName("semester_id").IsRequired();
        builder.Property(component => component.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(component => component.NormalizedName).HasColumnName("normalized_name").HasMaxLength(128).IsRequired();
        builder.Property(component => component.Weight).HasColumnName("weight").HasPrecision(5, 4).IsRequired();
        builder.Property(component => component.MaxScore).HasColumnName("max_score").HasPrecision(5, 2).IsRequired();
        builder.Property(component => component.DisplayOrder).HasColumnName("display_order").IsRequired();
        builder.Property(component => component.IsRequired).HasColumnName("is_required").IsRequired();
        builder.Property(component => component.IncludeInGpa).HasColumnName("include_in_gpa").IsRequired();
        builder.Property(component => component.Version).HasColumnName("version").IsRequired();
        builder.Property(component => component.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(component => component.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(component => component.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(component => new { component.SubjectId, component.SemesterId })
            .HasDatabaseName("ix_grade_components_subject_semester");

        builder.HasIndex(component => component.SemesterId)
            .HasDatabaseName("ix_grade_components_semester_id");

        builder.HasIndex(component => new { component.SubjectId, component.SemesterId, component.Version, component.NormalizedName })
            .IsUnique()
            .HasDatabaseName("ux_grade_components_version_name");

        builder.HasIndex(component => new { component.SubjectId, component.SemesterId, component.Version, component.DisplayOrder })
            .IsUnique()
            .HasDatabaseName("ux_grade_components_version_order");

        builder.HasOne(component => component.Subject)
            .WithMany()
            .HasForeignKey(component => component.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(component => component.Semester)
            .WithMany()
            .HasForeignKey(component => component.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
