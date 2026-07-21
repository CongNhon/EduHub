using Microsoft.Extensions.Configuration;

namespace EduHub.Infrastructure.Services.Jobs;

/// <summary>
/// Ghi chú: SmtpEmailOptions chứa cấu hình SMTP để gửi email digest thật.
/// </summary>
public sealed class SmtpEmailOptions
{
    public bool Enabled { get; init; }

    public string Host { get; init; } = "smtp.gmail.com";

    public int Port { get; init; } = 587;

    public string SenderEmail { get; init; } = string.Empty;

    public string SenderName { get; init; } = "EduHub";

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Ghi chú: FromConfiguration đọc cấu hình Email:Smtp cho SMTP email sender.
    /// </summary>
    public static SmtpEmailOptions FromConfiguration(IConfiguration configuration) =>
        new()
        {
            Enabled = bool.TryParse(configuration["Email:Smtp:Enabled"], out var enabled) && enabled,
            Host = configuration["Email:Smtp:Host"] ?? "smtp.gmail.com",
            Port = int.TryParse(configuration["Email:Smtp:Port"], out var port) ? port : 587,
            SenderEmail = configuration["Email:Smtp:SenderEmail"] ?? string.Empty,
            SenderName = configuration["Email:Smtp:SenderName"] ?? "EduHub",
            Username = configuration["Email:Smtp:Username"] ?? string.Empty,
            Password = configuration["Email:Smtp:Password"] ?? string.Empty
        };
}
