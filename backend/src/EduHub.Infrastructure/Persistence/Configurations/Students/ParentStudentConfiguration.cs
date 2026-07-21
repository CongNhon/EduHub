using EduHub.Domain.Entities.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Students;

/// <summary>
/// Ghi chú: ParentStudentConfiguration cấu hình mapping/constraint/index cho liên kết phụ huynh-học sinh.
/// </summary>
public sealed class ParentStudentConfiguration : IEntityTypeConfiguration<ParentStudent>
{
    /// <summary>
    /// Ghi chú: Configure map liên kết phụ huynh-học sinh sang table/column/index/constraint trong EF Core hoặc DI.
    /// </summary>
    public void Configure(EntityTypeBuilder<ParentStudent> builder)
    {
        builder.ToTable("parent_students");

        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(link => link.ParentUserId).HasColumnName("parent_user_id").IsRequired();
        builder.Property(link => link.StudentId).HasColumnName("student_id").IsRequired();
        builder.Property(link => link.Relationship).HasColumnName("relationship").HasMaxLength(64).IsRequired();
        builder.Property(link => link.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(link => link.EffectiveFromUtc).HasColumnName("effective_from_utc").IsRequired();
        builder.Property(link => link.EffectiveToUtc).HasColumnName("effective_to_utc");
        builder.Property(link => link.DeactivatedAtUtc).HasColumnName("deactivated_at_utc");
        builder.Property(link => link.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(link => link.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(link => new { link.ParentUserId, link.StudentId })
            .IsUnique()
            .HasDatabaseName("ux_parent_students_parent_user_id_student_id");

        builder.HasIndex(link => new { link.StudentId, link.IsActive })
            .HasDatabaseName("ix_parent_students_student_id_is_active");

        builder.HasOne(link => link.Student)
            .WithMany(student => student.ParentLinks)
            .HasForeignKey(link => link.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(link => link.ParentUser)
            .WithMany()
            .HasForeignKey(link => link.ParentUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
