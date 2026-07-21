using EduHub.Domain.Common;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Students;

/// <summary>
/// Ghi chú: Student đại diện cho hồ sơ học sinh trong hệ thống EduHub.
/// </summary>
public sealed class Student : AuditableEntity
{
    private Student()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor khởi tạo hồ sơ học sinh và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public Student(
        string studentCode,
        string normalizedStudentCode,
        string fullName,
        string normalizedFullName,
        DateOnly dateOfBirth,
        string? gender = null,
        string? phoneNumber = null,
        string? address = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(studentCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedStudentCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedFullName);

        StudentCode = studentCode;
        NormalizedStudentCode = normalizedStudentCode;
        FullName = fullName;
        NormalizedFullName = normalizedFullName;
        DateOfBirth = dateOfBirth;
        Gender = NormalizeOptional(gender);
        PhoneNumber = NormalizeOptional(phoneNumber);
        Address = NormalizeOptional(address);
    }

    public string StudentCode { get; private set; } = null!;

    public string NormalizedStudentCode { get; private set; } = null!;

    public string FullName { get; private set; } = null!;

    public string NormalizedFullName { get; private set; } = null!;

    public Guid? UserId { get; private set; }

    public User? User { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    public string? Gender { get; private set; }

    public string? PhoneNumber { get; private set; }

    public string? Address { get; private set; }

    public StudentStatus Status { get; private set; } = StudentStatus.Active;

    public int Version { get; private set; } = 1;

    public ICollection<ParentStudent> ParentLinks { get; } = new List<ParentStudent>();

    /// <summary>
    /// Ghi chú: UpdateProfile thực hiện phần xử lý của hồ sơ học sinh.
    /// </summary>
    public void UpdateProfile(string fullName, string normalizedFullName, DateOnly dateOfBirth, StudentStatus status, DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        FullName = fullName.Trim();
        NormalizedFullName = normalizedFullName;
        DateOfBirth = dateOfBirth;
        Status = status;
        Version++;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: ApplyApprovedProfile áp dụng họ tên, ngày sinh, giới tính, điện thoại và địa chỉ đã được học vụ duyệt cho học sinh.
    /// </summary>
    public void ApplyApprovedProfile(
        string fullName,
        string normalizedFullName,
        DateOnly dateOfBirth,
        string? gender,
        string? phoneNumber,
        string? address,
        DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        FullName = fullName.Trim();
        NormalizedFullName = normalizedFullName;
        DateOfBirth = dateOfBirth;
        Gender = NormalizeOptional(gender);
        PhoneNumber = NormalizeOptional(phoneNumber);
        Address = NormalizeOptional(address);
        Version++;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: LinkUserAccount gắn tài khoản role Student với đúng hồ sơ học sinh để truy cập dữ liệu self-service.
    /// </summary>
    public void LinkUserAccount(Guid userId, DateTime updatedAtUtc)
    {
        UserId = userId;
        Version++;
        MarkUpdated(updatedAtUtc);
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
