using EduHub.Domain.Entities.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduHub.Infrastructure.Persistence.Configurations.Reports;

/// <summary>
/// Ghi chú: ReportJobConfiguration cấu hình bảng report_jobs lưu trạng thái sinh PDF.
/// </summary>
public sealed class ReportJobConfiguration : IEntityTypeConfiguration<ReportJob>
{
    /// <summary>
    /// Ghi chú: Configure map ReportJob sang bảng report_jobs với idempotency key.
    /// </summary>
    public void Configure(EntityTypeBuilder<ReportJob> builder)
    {
        builder.ToTable("report_jobs");
        builder.HasKey(job => job.Id);
        builder.Property(job => job.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(job => job.RequesterUserId).HasColumnName("requester_user_id").IsRequired();
        builder.Property(job => job.StudentId).HasColumnName("student_id").IsRequired();
        builder.Property(job => job.SemesterId).HasColumnName("semester_id").IsRequired();
        builder.Property(job => job.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(128).IsRequired();
        builder.Property(job => job.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(job => job.HangfireJobId).HasColumnName("hangfire_job_id").HasMaxLength(128);
        builder.Property(job => job.StorageKey).HasColumnName("storage_key").HasMaxLength(512);
        builder.Property(job => job.ChecksumSha256).HasColumnName("checksum_sha256").HasMaxLength(128);
        builder.Property(job => job.PolicyVersion).HasColumnName("policy_version").HasMaxLength(64);
        builder.Property(job => job.GeneratedAtUtc).HasColumnName("generated_at_utc");
        builder.Property(job => job.ExpiresAtUtc).HasColumnName("expires_at_utc");
        builder.Property(job => job.FailureReason).HasColumnName("failure_reason").HasMaxLength(1024);
        builder.Property(job => job.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(job => job.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(job => new { job.RequesterUserId, job.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("ux_report_jobs_requester_idempotency");

        builder.HasOne(job => job.RequesterUser)
            .WithMany()
            .HasForeignKey(job => job.RequesterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(job => job.Student)
            .WithMany()
            .HasForeignKey(job => job.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(job => job.Semester)
            .WithMany()
            .HasForeignKey(job => job.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
