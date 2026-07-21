using EduHub.Domain.Common;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Students;

/// <summary>
/// Ghi chú: StudentProfileChangeRequest lưu đề nghị sửa hồ sơ học sinh, lý do, ảnh bằng chứng và kết quả duyệt.
/// </summary>
public sealed class StudentProfileChangeRequest : AuditableEntity
{
    private StudentProfileChangeRequest()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo yêu cầu sửa thông tin cho đúng học sinh đang đăng nhập và bắt buộc có ảnh bằng chứng.
    /// </summary>
    public StudentProfileChangeRequest(
        Guid studentId,
        Guid requesterUserId,
        string requestedFullName,
        DateOnly requestedDateOfBirth,
        string? requestedGender,
        string? requestedPhoneNumber,
        string? requestedAddress,
        string reason,
        string evidenceObjectKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestedFullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceObjectKey);
        StudentId = studentId;
        RequesterUserId = requesterUserId;
        RequestedFullName = requestedFullName.Trim();
        RequestedDateOfBirth = requestedDateOfBirth;
        RequestedGender = Normalize(requestedGender);
        RequestedPhoneNumber = Normalize(requestedPhoneNumber);
        RequestedAddress = Normalize(requestedAddress);
        Reason = reason.Trim();
        EvidenceObjectKey = evidenceObjectKey.Trim();
    }

    public Guid StudentId { get; private set; }
    public Student Student { get; private set; } = null!;
    public Guid RequesterUserId { get; private set; }
    public User RequesterUser { get; private set; } = null!;
    public string RequestedFullName { get; private set; } = null!;
    public DateOnly RequestedDateOfBirth { get; private set; }
    public string? RequestedGender { get; private set; }
    public string? RequestedPhoneNumber { get; private set; }
    public string? RequestedAddress { get; private set; }
    public string Reason { get; private set; } = null!;
    public string EvidenceObjectKey { get; private set; } = null!;
    public ProfileChangeRequestStatus Status { get; private set; } = ProfileChangeRequestStatus.Pending;
    public Guid? ReviewerUserId { get; private set; }
    public User? ReviewerUser { get; private set; }
    public string? ReviewNote { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }

    /// <summary>
    /// Ghi chú: Approve đánh dấu yêu cầu hồ sơ học sinh đã được quản trị học vụ duyệt.
    /// </summary>
    public void Approve(Guid reviewerUserId, string? reviewNote, DateTime reviewedAtUtc)
    {
        EnsurePending();
        ReviewerUserId = reviewerUserId;
        ReviewNote = Normalize(reviewNote);
        ReviewedAtUtc = UtcDateTime.Require(reviewedAtUtc, nameof(reviewedAtUtc));
        Status = ProfileChangeRequestStatus.Approved;
        MarkUpdated(ReviewedAtUtc.Value);
    }

    /// <summary>
    /// Ghi chú: Reject từ chối yêu cầu sửa hồ sơ học sinh và lưu lý do của quản trị học vụ.
    /// </summary>
    public void Reject(Guid reviewerUserId, string reviewNote, DateTime reviewedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewNote);
        EnsurePending();
        ReviewerUserId = reviewerUserId;
        ReviewNote = reviewNote.Trim();
        ReviewedAtUtc = UtcDateTime.Require(reviewedAtUtc, nameof(reviewedAtUtc));
        Status = ProfileChangeRequestStatus.Rejected;
        MarkUpdated(ReviewedAtUtc.Value);
    }

    /// <summary>
    /// Ghi chú: EnsurePending ngăn một yêu cầu hồ sơ đã duyệt hoặc từ chối bị xử lý lần thứ hai.
    /// </summary>
    private void EnsurePending()
    {
        if (Status != ProfileChangeRequestStatus.Pending)
        {
            throw new InvalidOperationException("Only a pending profile change request can be reviewed.");
        }
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
