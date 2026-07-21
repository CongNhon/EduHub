using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: AcademicYearConfiguration cấu hình mapping/constraint/index cho năm học.
/// </summary>
public sealed class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    /// <summary>
    /// Ghi chú: Configure map năm học sang table/column/index/constraint trong EF Core hoặc DI.
    /// </summary>
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.ToTable("academic_years", table =>
        {
            table.HasCheckConstraint("ck_academic_years_date_range", "start_date < end_date");
        });

        builder.HasKey(year => year.Id);
        builder.Property(year => year.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(year => year.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(year => year.NormalizedName).HasColumnName("normalized_name").HasMaxLength(128).IsRequired();
        builder.Property(year => year.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();
        builder.Property(year => year.EndDate).HasColumnName("end_date").HasColumnType("date").IsRequired();
        builder.Property(year => year.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(year => year.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(year => year.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(year => year.NormalizedName)
            .IsUnique()
            .HasDatabaseName("ux_academic_years_normalized_name");
    }
}
