namespace EduHub.Infrastructure.Services.Jobs;

/// <summary>
/// Ghi chú: IEmailSender định nghĩa adapter gửi email điểm thật hoặc fake cho phụ huynh.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Ghi chú: SendAsync gửi email điểm cho một phụ huynh với subject, HTML body, text body và idempotency key đã lưu.
    /// </summary>
    Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        string textBody,
        string idempotencyKey,
        CancellationToken cancellationToken = default);
}
