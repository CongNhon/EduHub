using EduHub.Domain.Entities.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Students;

/// <summary>
/// Ghi chú: StudentConfiguration cấu hình mapping/constraint/index cho hồ sơ học sinh.
/// </summary>
public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    /// <summary>
    /// Ghi chú: Configure map hồ sơ học sinh sang table/column/index/constraint trong EF Core hoặc DI.
    /// </summary>
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");

        builder.HasKey(student => student.Id);
        builder.Property(student => student.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(student => student.StudentCode).HasColumnName("student_code").HasMaxLength(64).IsRequired();
        builder.Property(student => student.NormalizedStudentCode).HasColumnName("normalized_student_code").HasMaxLength(64).IsRequired();
        builder.Property(student => student.FullName).HasColumnName("full_name").HasMaxLength(256).IsRequired();
        builder.Property(student => student.NormalizedFullName).HasColumnName("normalized_full_name").HasMaxLength(256).IsRequired();
        builder.Property(student => student.UserId).HasColumnName("user_id");
        builder.Property(student => student.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date").IsRequired();
        builder.Property(student => student.Gender).HasColumnName("gender").HasMaxLength(32);
        builder.Property(student => student.PhoneNumber).HasColumnName("phone_number").HasMaxLength(32);
        builder.Property(student => student.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(student => student.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(student => student.Version).HasColumnName("version").IsConcurrencyToken().IsRequired();
        builder.Property(student => student.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(student => student.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(student => student.NormalizedStudentCode)
            .IsUnique()
            .HasDatabaseName("ux_students_normalized_student_code");

        builder.HasIndex(student => student.NormalizedFullName)
            .HasDatabaseName("ix_students_normalized_full_name");

        builder.HasIndex(student => student.UserId)
            .IsUnique()
            .HasFilter("user_id IS NOT NULL")
            .HasDatabaseName("ux_students_user_id");

        builder.HasOne(student => student.User)
            .WithOne()
            .HasForeignKey<Student>(student => student.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
