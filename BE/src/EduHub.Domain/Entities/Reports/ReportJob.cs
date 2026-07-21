using EduHub.Domain.Common;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Reports;

/// <summary>
/// Ghi chú: ReportJob lưu trạng thái sinh PDF bảng điểm cho một requester và học sinh.
/// </summary>
public sealed class ReportJob : AuditableEntity
{
    private ReportJob()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo report job queued sau khi API đã authorize requester.
    /// </summary>
    public ReportJob(Guid requesterUserId, Guid studentId, Guid semesterId, string idempotencyKey, DateTime createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        RequesterUserId = requesterUserId;
        StudentId = studentId;
        SemesterId = semesterId;
        IdempotencyKey = idempotencyKey.Trim();
        CreatedAtUtc = UtcDateTime.Require(createdAtUtc, nameof(createdAtUtc));
        Status = ReportJobStatus.Queued;
    }

    public Guid RequesterUserId { get; private set; }

    public User RequesterUser { get; private set; } = null!;

    public Guid StudentId { get; private set; }

    public Student Student { get; private set; } = null!;

    public Guid SemesterId { get; private set; }

    public Semester Semester { get; private set; } = null!;

    public string IdempotencyKey { get; private set; } = null!;

    public ReportJobStatus Status { get; private set; }

    public string? HangfireJobId { get; private set; }

    public string? StorageKey { get; private set; }

    public string? ChecksumSha256 { get; private set; }

    public string? PolicyVersion { get; private set; }

    public DateTime? GeneratedAtUtc { get; private set; }

    public DateTime? ExpiresAtUtc { get; private set; }

    public string? FailureReason { get; private set; }

    /// <summary>
    /// Ghi chú: MarkEnqueued lưu Hangfire job id được enqueue cho report job.
    /// </summary>
    public void MarkEnqueued(string hangfireJobId, DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hangfireJobId);
        HangfireJobId = hangfireJobId.Trim();
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: MarkProcessing chuyển report job sang trạng thái đang sinh PDF.
    /// </summary>
    public void MarkProcessing(DateTime updatedAtUtc)
    {
        if (Status is ReportJobStatus.Completed)
        {
            return;
        }

        Status = ReportJobStatus.Processing;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: MarkCompleted lưu file PDF, checksum, policy version và hạn tải.
    /// </summary>
    public void MarkCompleted(
        string storageKey,
        string checksumSha256,
        string policyVersion,
        DateTime generatedAtUtc,
        DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(checksumSha256);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyVersion);

        StorageKey = storageKey.Trim();
        ChecksumSha256 = checksumSha256.Trim();
        PolicyVersion = policyVersion.Trim();
        GeneratedAtUtc = UtcDateTime.Require(generatedAtUtc, nameof(generatedAtUtc));
        ExpiresAtUtc = UtcDateTime.Require(expiresAtUtc, nameof(expiresAtUtc));
        Status = ReportJobStatus.Completed;
        FailureReason = null;
        MarkUpdated(GeneratedAtUtc.Value);
    }

    /// <summary>
    /// Ghi chú: MarkFailed lưu lỗi sinh PDF để admin/user query được trạng thái fail.
    /// </summary>
    public void MarkFailed(string reason, DateTime failedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Status = ReportJobStatus.Failed;
        FailureReason = reason.Trim();
        MarkUpdated(failedAtUtc);
    }

    /// <summary>
    /// Ghi chú: MarkExpired đánh dấu report đã hết hạn tải.
    /// </summary>
    public void MarkExpired(DateTime expiredAtUtc)
    {
        Status = ReportJobStatus.Expired;
        MarkUpdated(expiredAtUtc);
    }
}
