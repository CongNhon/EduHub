using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace EduHub.Infrastructure.Services.Jobs;

/// <summary>
/// Ghi chú: SmtpEmailSender gửi email digest thật qua SMTP provider.
/// </summary>
public sealed partial class SmtpEmailSender(SmtpEmailOptions options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    /// <summary>
    /// Ghi chú: SendAsync gửi email digest thật cho phụ huynh qua SMTP.
    /// </summary>
    public async Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        string textBody,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(options.SenderEmail, options.SenderName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain"));
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html"));
        message.To.Add(recipientEmail);

        using var client = new SmtpClient(options.Host, options.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(options.Username, options.Password)
        };

        await client.SendMailAsync(message, cancellationToken);
        LogSmtpEmailSent(logger, recipientEmail, subject, idempotencyKey);
    }

    [LoggerMessage(
        EventId = 41,
        Level = LogLevel.Information,
        Message = "SMTP email sent. Recipient: {RecipientEmail}; Subject: {Subject}; IdempotencyKey: {IdempotencyKey}")]
    private static partial void LogSmtpEmailSent(
        ILogger logger,
        string recipientEmail,
        string subject,
        string idempotencyKey);
}
