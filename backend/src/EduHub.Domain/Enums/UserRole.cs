namespace EduHub.Domain.Enums;

/// <summary>
/// Ghi chú: UserRole liệt kê các giá trị hợp lệ cho role phân quyền của người dùng.
/// </summary>
public enum UserRole
{
    SystemAdmin,
    AcademicAdmin,
    Teacher,
    Parent,
    Student,
    IntegrationService
}
