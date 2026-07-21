using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: StudentRemarkConfiguration map nhận xét môn học của giáo viên sang bảng student_remarks.
/// </summary>
public sealed class StudentRemarkConfiguration : IEntityTypeConfiguration<StudentRemark>
{
    /// <summary>
    /// Ghi chú: Configure cấu hình khóa, quan hệ và optimistic concurrency cho nhận xét học sinh.
    /// </summary>
    public void Configure(EntityTypeBuilder<StudentRemark> builder)
    {
        builder.ToTable("student_remarks");
        builder.HasKey(remark => remark.Id);
        builder.Property(remark => remark.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(remark => remark.StudentId).HasColumnName("student_id").IsRequired();
        builder.Property(remark => remark.AssignmentId).HasColumnName("assignment_id").IsRequired();
        builder.Property(remark => remark.TeacherId).HasColumnName("teacher_id").IsRequired();
        builder.Property(remark => remark.Content).HasColumnName("content").HasMaxLength(2000).IsRequired();
        builder.Property(remark => remark.IsPublished).HasColumnName("is_published").IsRequired();
        builder.Property(remark => remark.Version).HasColumnName("version").IsConcurrencyToken().IsRequired();
        builder.Property(remark => remark.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(remark => remark.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(remark => new { remark.AssignmentId, remark.StudentId }).IsUnique().HasDatabaseName("ux_student_remarks_assignment_student");
        builder.HasOne(remark => remark.Student).WithMany().HasForeignKey(remark => remark.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(remark => remark.Assignment).WithMany().HasForeignKey(remark => remark.AssignmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(remark => remark.Teacher).WithMany().HasForeignKey(remark => remark.TeacherId).OnDelete(DeleteBehavior.Restrict);
    }
}
