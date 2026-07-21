using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.People;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Enums;

namespace EduHub.Application.Interfaces.Repositories.People;

/// <summary>
/// Ghi chú: IPeopleRepository truy cập tài khoản giáo viên, phụ huynh, học sinh và quản trị viên trong PostgreSQL.
/// </summary>
public interface IPeopleRepository
{
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<User?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<int> CountActiveSystemAdminsAsync(CancellationToken cancellationToken);
    Task<bool> HasActiveRoleDependenciesAsync(Guid userId, UserRole role, CancellationToken cancellationToken);
    Task RevokeActiveRefreshTokensAsync(Guid userId, DateTime revokedAtUtc, CancellationToken cancellationToken);
    Task<PagedResult<UserAccountResponse>> ListAsync(UserRole? role, bool? isActive, string? search, PageRequest pageRequest, CancellationToken cancellationToken);
    void Add(User user);
}
