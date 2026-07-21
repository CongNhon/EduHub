namespace EduHub.Infrastructure.Options;

/// <summary>
/// Ghi chú: SchoolProfileOptions map cấu hình nhận diện trường duy nhất từ appsettings hoặc environment variables.
/// </summary>
public sealed class SchoolProfileOptions
{
    public string Code { get; set; } = "EDUHUB";
    public string Name { get; set; } = "Trường EduHub";
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
