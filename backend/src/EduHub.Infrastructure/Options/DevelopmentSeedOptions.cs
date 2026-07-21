using EduHub.Domain.Enums;

namespace EduHub.Infrastructure.Options;

/// <summary>
/// Ghi chú: DevelopmentSeedOptions chứa cấu hình cho cấu hình seed tài khoản admin dev.
/// </summary>
public sealed class DevelopmentSeedOptions
{
    public const string SectionName = "Auth:DevelopmentSeed";

    public bool Enabled { get; set; }

    public string Email { get; set; } = "admin@eduhub.local";

    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.SystemAdmin;
}
