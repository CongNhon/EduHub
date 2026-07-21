using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: TimetableVersionConfiguration map phiên bản nháp, công bố và lưu trữ của thời khóa biểu học kỳ.
/// </summary>
public sealed class TimetableVersionConfiguration : IEntityTypeConfiguration<TimetableVersion>
{
    /// <summary>
    /// Ghi chú: Configure cấu hình học kỳ, trạng thái và các tiết thuộc một phiên bản thời khóa biểu.
    /// </summary>
    public void Configure(EntityTypeBuilder<TimetableVersion> builder)
    {
        builder.ToTable("timetable_versions");
        builder.HasKey(version => version.Id);
        builder.Property(version => version.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(version => version.SemesterId).HasColumnName("semester_id").IsRequired();
        builder.Property(version => version.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
        builder.Property(version => version.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(version => version.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(version => version.GeneratedAtUtc).HasColumnName("generated_at_utc").IsRequired();
        builder.Property(version => version.PublishedAtUtc).HasColumnName("published_at_utc");
        builder.Property(version => version.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(version => version.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(version => new { version.SemesterId, version.Status }).HasDatabaseName("ix_timetable_versions_semester_status");
        builder.HasOne(version => version.Semester).WithMany().HasForeignKey(version => version.SemesterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(version => version.Entries).WithOne(entry => entry.TimetableVersion).HasForeignKey(entry => entry.TimetableVersionId).OnDelete(DeleteBehavior.Cascade);
    }
}
