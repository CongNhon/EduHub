using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: GradeEntryConfiguration cấu hình bảng điểm từng học sinh theo assignment-component.
/// </summary>
public sealed class GradeEntryConfiguration : IEntityTypeConfiguration<GradeEntry>
{
    /// <summary>
    /// Ghi chú: Configure map GradeEntry sang bảng grade_entries.
    /// </summary>
    public void Configure(EntityTypeBuilder<GradeEntry> builder)
    {
        builder.ToTable("grade_entries", table =>
        {
            table.HasCheckConstraint("ck_grade_entries_score_range", "score >= 0");
            table.HasCheckConstraint("ck_grade_entries_version_positive", "version > 0");
            table.HasCheckConstraint("ck_grade_entries_publication_version_non_negative", "publication_version >= 0");
        });

        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(entry => entry.StudentId).HasColumnName("student_id").IsRequired();
        builder.Property(entry => entry.AssignmentId).HasColumnName("assignment_id").IsRequired();
        builder.Property(entry => entry.ComponentId).HasColumnName("component_id").IsRequired();
        builder.Property(entry => entry.Score).HasColumnName("score").HasPrecision(5, 2).IsRequired();
        builder.Property(entry => entry.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entry => entry.Version).HasColumnName("version").IsConcurrencyToken().IsRequired();
        builder.Property(entry => entry.PublicationVersion).HasColumnName("publication_version").IsRequired();
        builder.Property(entry => entry.SubmittedAtUtc).HasColumnName("submitted_at_utc");
        builder.Property(entry => entry.PublishedAtUtc).HasColumnName("published_at_utc");
        builder.Property(entry => entry.LockedAtUtc).HasColumnName("locked_at_utc");
        builder.Property(entry => entry.ReopenedAtUtc).HasColumnName("reopened_at_utc");
        builder.Property(entry => entry.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(entry => entry.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(entry => new { entry.StudentId, entry.AssignmentId, entry.ComponentId })
            .IsUnique()
            .HasDatabaseName("ux_grade_entries_student_assignment_component");

        builder.HasIndex(entry => new { entry.AssignmentId, entry.Status })
            .HasDatabaseName("ix_grade_entries_assignment_status");

        builder.HasOne(entry => entry.Student)
            .WithMany()
            .HasForeignKey(entry => entry.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entry => entry.Assignment)
            .WithMany()
            .HasForeignKey(entry => entry.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entry => entry.Component)
            .WithMany()
            .HasForeignKey(entry => entry.ComponentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entry => entry.Histories)
            .WithOne(history => history.GradeEntry)
            .HasForeignKey(history => history.GradeEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
