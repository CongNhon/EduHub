using Microsoft.Extensions.Logging;

namespace EduHub.Infrastructure.Services.Jobs;

/// <summary>
/// Ghi chú: FakeEmailSender giả lập gửi email điểm bằng log, không gọi SMTP provider thật.
/// </summary>
public sealed partial class FakeEmailSender(ILogger<FakeEmailSender> logger) : IEmailSender
{
    /// <summary>
    /// Ghi chú: SendAsync ghi log email điểm của phụ huynh với subject, idempotency key và text preview.
    /// </summary>
    public Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        string textBody,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        LogFakeEmailSent(logger, recipientEmail, subject, idempotencyKey, textBody);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 40,
        Level = LogLevel.Information,
        Message = "Fake email sent. Recipient: {RecipientEmail}; Subject: {Subject}; IdempotencyKey: {IdempotencyKey}; Body: {TextBody}")]
    private static partial void LogFakeEmailSent(
        ILogger logger,
        string recipientEmail,
        string subject,
        string idempotencyKey,
        string textBody);
}
