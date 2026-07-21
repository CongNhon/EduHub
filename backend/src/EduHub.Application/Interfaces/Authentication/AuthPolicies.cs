namespace EduHub.Application.Interfaces.Authentication;

/// <summary>
/// Ghi chú: AuthPolicies đại diện cho tên policy phân quyền theo role trong hệ thống EduHub.
/// </summary>
public static class AuthPolicies
{
    public const string SystemAdmin = nameof(SystemAdmin);
    public const string AcademicAdmin = nameof(AcademicAdmin);
    public const string Teacher = nameof(Teacher);
    public const string Parent = nameof(Parent);
    public const string Student = nameof(Student);
    public const string IntegrationService = nameof(IntegrationService);
}
