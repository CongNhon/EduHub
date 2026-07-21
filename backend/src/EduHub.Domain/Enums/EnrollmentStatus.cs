namespace EduHub.Domain.Enums;

/// <summary>
/// Ghi chú: EnrollmentStatus liệt kê trạng thái hợp lệ của ghi danh học sinh vào lớp/học kỳ.
/// </summary>
public enum EnrollmentStatus
{
    Pending,
    Active,
    Completed,
    Withdrawn,
    Rejected
}
