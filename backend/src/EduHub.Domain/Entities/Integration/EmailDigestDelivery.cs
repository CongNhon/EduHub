using EduHub.Domain.Common;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Integration;

/// <summary>
/// Ghi chú: EmailDigestDelivery lưu idempotency key cho fake email digest đã gửi.
/// </summary>
public sealed class EmailDigestDelivery : BaseEntity
{
    private EmailDigestDelivery()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo record chống gửi trùng email digest theo recipient/period/template.
    /// </summary>
    public EmailDigestDelivery(
        Guid recipientUserId,
        string recipientEmail,
        string idempotencyKey,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        string templateVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateVersion);

        RecipientUserId = recipientUserId;
        RecipientEmail = recipientEmail.Trim();
        IdempotencyKey = idempotencyKey.Trim();
        PeriodStartUtc = UtcDateTime.Require(periodStartUtc, nameof(periodStartUtc));
        PeriodEndUtc = UtcDateTime.Require(periodEndUtc, nameof(periodEndUtc));
        TemplateVersion = templateVersion.Trim();
        Status = EmailDigestDeliveryStatus.Pending;
    }

    public Guid RecipientUserId { get; private set; }

    public User RecipientUser { get; private set; } = null!;

    public string RecipientEmail { get; private set; } = null!;

    public string IdempotencyKey { get; private set; } = null!;

    public DateTime PeriodStartUtc { get; private set; }

    public DateTime PeriodEndUtc { get; private set; }

    public string TemplateVersion { get; private set; } = null!;

    public EmailDigestDeliveryStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTime? LastAttemptAtUtc { get; private set; }

    public DateTime? SentAtUtc { get; private set; }

    public string? LastError { get; private set; }

    /// <summary>
    /// Ghi chu: BeginAttempt ghi nhan mot lan job bat dau gui email tong hop cho phu huynh.
    /// </summary>
    public void BeginAttempt(DateTime attemptedAtUtc)
    {
        AttemptCount++;
        LastAttemptAtUtc = UtcDateTime.Require(attemptedAtUtc, nameof(attemptedAtUtc));
        Status = EmailDigestDeliveryStatus.Sending;
        LastError = null;
    }

    /// <summary>
    /// Ghi chu: MarkSent chi danh dau email tong hop da gui sau khi SMTP tra thanh cong.
    /// </summary>
    public void MarkSent(DateTime sentAtUtc)
    {
        SentAtUtc = UtcDateTime.Require(sentAtUtc, nameof(sentAtUtc));
        Status = EmailDigestDeliveryStatus.Sent;
        LastError = null;
    }

    /// <summary>
    /// Ghi chu: MarkFailed luu loi SMTP de Hangfire retry email tong hop trong lan sau.
    /// </summary>
    public void MarkFailed(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        Status = EmailDigestDeliveryStatus.Failed;
        LastError = error.Trim()[..Math.Min(error.Trim().Length, 2048)];
    }
}
