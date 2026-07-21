using EduHub.Domain.Entities.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Reports;

/// <summary>
/// Ghi chú: ReportRequestConfiguration map yêu cầu báo cáo phụ huynh gửi quản trị học vụ sang PostgreSQL.
/// </summary>
public sealed class ReportRequestConfiguration : IEntityTypeConfiguration<ReportRequest>
{
    /// <summary>
    /// Ghi chú: Configure cấu hình trạng thái, reviewer, report job và các quan hệ của yêu cầu báo cáo.
    /// </summary>
    public void Configure(EntityTypeBuilder<ReportRequest> builder)
    {
        builder.ToTable("report_requests");
        builder.HasKey(request => request.Id);
        builder.Property(request => request.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(request => request.RequesterUserId).HasColumnName("requester_user_id").IsRequired();
        builder.Property(request => request.StudentId).HasColumnName("student_id").IsRequired();
        builder.Property(request => request.SemesterId).HasColumnName("semester_id").IsRequired();
        builder.Property(request => request.ReviewerUserId).HasColumnName("reviewer_user_id");
        builder.Property(request => request.ReportJobId).HasColumnName("report_job_id");
        builder.Property(request => request.Purpose).HasColumnName("purpose").HasMaxLength(500).IsRequired();
        builder.Property(request => request.ReviewNote).HasColumnName("review_note").HasMaxLength(1000);
        builder.Property(request => request.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(request => request.RequestedAtUtc).HasColumnName("requested_at_utc").IsRequired();
        builder.Property(request => request.ReviewedAtUtc).HasColumnName("reviewed_at_utc");
        builder.Property(request => request.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(request => request.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(request => new { request.RequesterUserId, request.StudentId, request.SemesterId })
            .IsUnique()
            .HasFilter("status IN ('Pending', 'Approved', 'Generating')")
            .HasDatabaseName("ux_report_requests_open_owner_scope");
        builder.HasOne(request => request.RequesterUser).WithMany().HasForeignKey(request => request.RequesterUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(request => request.ReviewerUser).WithMany().HasForeignKey(request => request.ReviewerUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(request => request.Student).WithMany().HasForeignKey(request => request.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(request => request.Semester).WithMany().HasForeignKey(request => request.SemesterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(request => request.ReportJob).WithMany().HasForeignKey(request => request.ReportJobId).OnDelete(DeleteBehavior.Restrict);
    }
}
