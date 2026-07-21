namespace EduHub.WebApi.Dtos.School;

/// <summary>
/// Ghi chú: SchoolProfileDto trả nhận diện trường duy nhất cho giao diện và báo cáo.
/// </summary>
public sealed record SchoolProfileDto(string Code, string Name, string? LogoUrl, string? Address, string? Email, string? PhoneNumber);
