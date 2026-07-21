using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: SemesterConfiguration cấu hình mapping/constraint/index cho học kỳ.
/// </summary>
public sealed class SemesterConfiguration : IEntityTypeConfiguration<Semester>
{
    /// <summary>
    /// Ghi chú: Configure map học kỳ sang table/column/index/constraint trong EF Core hoặc DI.
    /// </summary>
    public void Configure(EntityTypeBuilder<Semester> builder)
    {
        builder.ToTable("semesters", table =>
        {
            table.HasCheckConstraint("ck_semesters_date_range", "start_date < end_date");
            table.HasCheckConstraint("ck_semesters_grade_entry_range", "grade_entry_from <= grade_entry_to");
        });

        builder.HasKey(semester => semester.Id);
        builder.Property(semester => semester.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(semester => semester.AcademicYearId).HasColumnName("academic_year_id").IsRequired();
        builder.Property(semester => semester.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(semester => semester.NormalizedName).HasColumnName("normalized_name").HasMaxLength(128).IsRequired();
        builder.Property(semester => semester.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();
        builder.Property(semester => semester.EndDate).HasColumnName("end_date").HasColumnType("date").IsRequired();
        builder.Property(semester => semester.GradeEntryFrom).HasColumnName("grade_entry_from").HasColumnType("date").IsRequired();
        builder.Property(semester => semester.GradeEntryTo).HasColumnName("grade_entry_to").HasColumnType("date").IsRequired();
        builder.Property(semester => semester.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(semester => semester.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(semester => semester.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(semester => new { semester.AcademicYearId, semester.NormalizedName })
            .IsUnique()
            .HasDatabaseName("ux_semesters_academic_year_id_normalized_name");

        builder.HasIndex(semester => semester.AcademicYearId)
            .HasDatabaseName("ix_semesters_academic_year_id");

        builder.HasOne(semester => semester.AcademicYear)
            .WithMany(year => year.Semesters)
            .HasForeignKey(semester => semester.AcademicYearId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
