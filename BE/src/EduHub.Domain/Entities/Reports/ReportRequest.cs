using EduHub.Domain.Common;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Reports;

/// <summary>
/// Ghi chú: ReportRequest đại diện cho yêu cầu phụ huynh gửi quản trị học vụ duyệt trước khi tạo PDF bảng điểm học kỳ.
/// </summary>
public sealed class ReportRequest : AuditableEntity
{
    private ReportRequest() { }

    /// <summary>
    /// Ghi chú: Constructor tạo yêu cầu báo cáo Pending cho một học sinh và học kỳ thuộc quyền phụ huynh.
    /// </summary>
    public ReportRequest(Guid requesterUserId, Guid studentId, Guid semesterId, string purpose, DateTime requestedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        RequesterUserId = requesterUserId;
        StudentId = studentId;
        SemesterId = semesterId;
        Purpose = purpose.Trim();
        RequestedAtUtc = UtcDateTime.Require(requestedAtUtc, nameof(requestedAtUtc));
        Status = ReportRequestStatus.Pending;
    }

    public Guid RequesterUserId { get; private set; }
    public User RequesterUser { get; private set; } = null!;
    public Guid StudentId { get; private set; }
    public Student Student { get; private set; } = null!;
    public Guid SemesterId { get; private set; }
    public Semester Semester { get; private set; } = null!;
    public Guid? ReviewerUserId { get; private set; }
    public User? ReviewerUser { get; private set; }
    public Guid? ReportJobId { get; private set; }
    public ReportJob? ReportJob { get; private set; }
    public string Purpose { get; private set; } = null!;
    public string? ReviewNote { get; private set; }
    public ReportRequestStatus Status { get; private set; }
    public DateTime RequestedAtUtc { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }

    /// <summary>
    /// Ghi chú: Approve ghi nhận quản trị học vụ đã duyệt yêu cầu báo cáo của phụ huynh.
    /// </summary>
    public void Approve(Guid reviewerUserId, string? note, DateTime reviewedAtUtc)
    {
        EnsurePending();
        ReviewerUserId = reviewerUserId;
        ReviewNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        ReviewedAtUtc = UtcDateTime.Require(reviewedAtUtc, nameof(reviewedAtUtc));
        Status = ReportRequestStatus.Approved;
        MarkUpdated(ReviewedAtUtc.Value);
    }

    /// <summary>
    /// Ghi chú: Reject từ chối yêu cầu báo cáo và lưu lý do để phụ huynh biết cách xử lý.
    /// </summary>
    public void Reject(Guid reviewerUserId, string reason, DateTime reviewedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        EnsurePending();
        ReviewerUserId = reviewerUserId;
        ReviewNote = reason.Trim();
        ReviewedAtUtc = UtcDateTime.Require(reviewedAtUtc, nameof(reviewedAtUtc));
        Status = ReportRequestStatus.Rejected;
        MarkUpdated(ReviewedAtUtc.Value);
    }

    /// <summary>
    /// Ghi chú: AttachJob liên kết yêu cầu đã duyệt với Hangfire report job đang sinh PDF.
    /// </summary>
    public void AttachJob(Guid reportJobId, DateTime updatedAtUtc)
    {
        if (Status != ReportRequestStatus.Approved) throw new InvalidOperationException("Only approved request can generate a report.");
        ReportJobId = reportJobId;
        Status = ReportRequestStatus.Generating;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: Complete đánh dấu yêu cầu báo cáo đã có PDF để phụ huynh tải xuống.
    /// </summary>
    public void Complete(DateTime updatedAtUtc)
    {
        Status = ReportRequestStatus.Completed;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: Fail đánh dấu quá trình tạo PDF thất bại để quản trị học vụ có thể retry.
    /// </summary>
    public void Fail(string reason, DateTime updatedAtUtc)
    {
        ReviewNote = reason;
        Status = ReportRequestStatus.Failed;
        MarkUpdated(updatedAtUtc);
    }

    private void EnsurePending()
    {
        if (Status != ReportRequestStatus.Pending) throw new InvalidOperationException("Report request is no longer pending.");
    }
}
