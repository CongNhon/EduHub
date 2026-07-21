using EduHub.Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Ghi chú: HomeroomAssignmentConfiguration map giáo viên chủ nhiệm đang phụ trách một lớp.
/// </summary>
public sealed class HomeroomAssignmentConfiguration : IEntityTypeConfiguration<HomeroomAssignment>
{
    /// <summary>
    /// Ghi chú: Configure bảo đảm một lớp và một giáo viên chỉ có tối đa một phân công chủ nhiệm đang hoạt động.
    /// </summary>
    public void Configure(EntityTypeBuilder<HomeroomAssignment> builder)
    {
        builder.ToTable("homeroom_assignments");
        builder.HasKey(assignment => assignment.Id);
        builder.Property(assignment => assignment.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(assignment => assignment.ClassRoomId).HasColumnName("class_room_id").IsRequired();
        builder.Property(assignment => assignment.TeacherId).HasColumnName("teacher_id").IsRequired();
        builder.Property(assignment => assignment.AssignedAtUtc).HasColumnName("assigned_at_utc").IsRequired();
        builder.Property(assignment => assignment.EndedAtUtc).HasColumnName("ended_at_utc");
        builder.Property(assignment => assignment.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(assignment => assignment.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(assignment => assignment.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(assignment => assignment.ClassRoomId).IsUnique().HasFilter("is_active = true").HasDatabaseName("ux_homeroom_assignments_active_class");
        builder.HasIndex(assignment => assignment.TeacherId).IsUnique().HasFilter("is_active = true").HasDatabaseName("ux_homeroom_assignments_active_teacher");
        builder.HasOne(assignment => assignment.ClassRoom).WithMany().HasForeignKey(assignment => assignment.ClassRoomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(assignment => assignment.Teacher).WithMany().HasForeignKey(assignment => assignment.TeacherId).OnDelete(DeleteBehavior.Restrict);
    }
}
