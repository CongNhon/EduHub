namespace EduHub.Application.Interfaces.Authentication;

/// <summary>
/// Ghi chú: IPasswordHashService là service kỹ thuật dùng để ipassword hash.
/// </summary>
public interface IPasswordHashService
{
    string HashPassword(string password);

    bool VerifyPassword(string passwordHash, string password);

    /// <summary>
    /// Ghi chu: NeedsRehash kiem tra password hash cu co can nang cap iteration khi user dang nhap thanh cong hay khong.
    /// </summary>
    bool NeedsRehash(string passwordHash);
}
