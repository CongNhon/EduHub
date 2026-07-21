using EduHub.Domain.Common;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Identity;

/// <summary>
/// Ghi chú: User đại diện cho tài khoản người dùng đăng nhập trong hệ thống EduHub.
/// </summary>
public sealed class User : AuditableEntity
{
    private User()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor khởi tạo tài khoản người dùng đăng nhập và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public User(
        string email,
        string normalizedEmail,
        string passwordHash,
        UserRole role,
        string? fullName = null,
        string? referenceCode = null,
        string? phoneNumber = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        Role = role;
        FullName = string.IsNullOrWhiteSpace(fullName) ? email.Trim() : fullName.Trim();
        ReferenceCode = string.IsNullOrWhiteSpace(referenceCode) ? null : referenceCode.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
    }

    public string Email { get; private set; } = null!;

    public string NormalizedEmail { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public string FullName { get; private set; } = null!;

    public string? ReferenceCode { get; private set; }

    public string? PhoneNumber { get; private set; }

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; } = true;

    public Guid SecurityStamp { get; private set; } = Guid.NewGuid();

    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();

    /// <summary>
    /// Ghi chú: UpdateProfile cập nhật hồ sơ hiển thị, role và trạng thái đăng nhập của giáo viên, phụ huynh hoặc quản trị viên.
    /// </summary>
    public void UpdateProfile(string fullName, string? referenceCode, string? phoneNumber, UserRole role, bool isActive, DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        var securityChanged = Role != role || IsActive != isActive;
        FullName = fullName.Trim();
        ReferenceCode = string.IsNullOrWhiteSpace(referenceCode) ? null : referenceCode.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        Role = role;
        IsActive = isActive;
        if (securityChanged)
        {
            SecurityStamp = Guid.NewGuid();
        }

        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chu: UpdatePasswordHash nang cap password hash cua tai khoan va vo hieu access token cu bang SecurityStamp moi.
    /// </summary>
    public void UpdatePasswordHash(string passwordHash, DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
        SecurityStamp = Guid.NewGuid();
        MarkUpdated(updatedAtUtc);
    }
}
