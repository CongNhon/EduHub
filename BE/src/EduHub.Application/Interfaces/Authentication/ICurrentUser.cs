using EduHub.Domain.Enums;

namespace EduHub.Application.Interfaces.Authentication;

/// <summary>
/// Ghi chú: ICurrentUser là interface/marker cho icurrent user.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    UserRole? Role { get; }

    string? SecurityStamp { get; }
}
