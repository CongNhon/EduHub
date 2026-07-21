using EduHub.Domain.Entities.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Students;

/// <summary>
/// Ghi chú: StudentProfileChangeRequestConfiguration map yêu cầu sửa hồ sơ và ảnh bằng chứng của học sinh.
/// </summary>
public sealed class StudentProfileChangeRequestConfiguration : IEntityTypeConfiguration<StudentProfileChangeRequest>
{
    /// <summary>
    /// Ghi chú: Configure cấu hình dữ liệu đề nghị, người duyệt và unique constraint chỉ cho một yêu cầu chờ duyệt.
    /// </summary>
    public void Configure(EntityTypeBuilder<StudentProfileChangeRequest> builder)
    {
        builder.ToTable("student_profile_change_requests");
        builder.HasKey(request => request.Id);
        builder.Property(request => request.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(request => request.StudentId).HasColumnName("student_id").IsRequired();
        builder.Property(request => request.RequesterUserId).HasColumnName("requester_user_id").IsRequired();
        builder.Property(request => request.RequestedFullName).HasColumnName("requested_full_name").HasMaxLength(256).IsRequired();
        builder.Property(request => request.RequestedDateOfBirth).HasColumnName("requested_date_of_birth").HasColumnType("date").IsRequired();
        builder.Property(request => request.RequestedGender).HasColumnName("requested_gender").HasMaxLength(32);
        builder.Property(request => request.RequestedPhoneNumber).HasColumnName("requested_phone_number").HasMaxLength(32);
        builder.Property(request => request.RequestedAddress).HasColumnName("requested_address").HasMaxLength(500);
        builder.Property(request => request.Reason).HasColumnName("reason").HasMaxLength(1000).IsRequired();
        builder.Property(request => request.EvidenceObjectKey).HasColumnName("evidence_object_key").HasMaxLength(500).IsRequired();
        builder.Property(request => request.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(request => request.ReviewerUserId).HasColumnName("reviewer_user_id");
        builder.Property(request => request.ReviewNote).HasColumnName("review_note").HasMaxLength(1000);
        builder.Property(request => request.ReviewedAtUtc).HasColumnName("reviewed_at_utc");
        builder.Property(request => request.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(request => request.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(request => request.StudentId).IsUnique().HasFilter("status = 'Pending'").HasDatabaseName("ux_student_profile_change_requests_pending_student");
        builder.HasOne(request => request.Student).WithMany().HasForeignKey(request => request.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(request => request.RequesterUser).WithMany().HasForeignKey(request => request.RequesterUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(request => request.ReviewerUser).WithMany().HasForeignKey(request => request.ReviewerUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
