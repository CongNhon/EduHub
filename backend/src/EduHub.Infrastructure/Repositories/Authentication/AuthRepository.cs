using EduHub.Application.Interfaces.Repositories.Authentication;
using EduHub.Domain.Entities.Identity;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Authentication;

/// <summary>
/// Ghi chú: AuthRepository dùng EF Core để truy cập dữ liệu tài khoản và refresh token.
/// </summary>
public sealed class AuthRepository(ApplicationDbContext dbContext) : IAuthRepository
{
    /// <summary>
    /// Ghi chú: GetUserByNormalizedEmailAsync lấy tài khoản người dùng theo email đã chuẩn hóa.
    /// </summary>
    public Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(candidate => candidate.NormalizedEmail == normalizedEmail, cancellationToken);

    /// <summary>
    /// Ghi chu: GetUserByIdAsync lay profile tai khoan da xac thuc de tra ve endpoint auth/me.
    /// </summary>
    public Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

    /// <summary>
    /// Ghi chú: AddRefreshToken thêm refresh token mới vào DbContext.
    /// </summary>
    public void AddRefreshToken(RefreshToken refreshToken) => dbContext.RefreshTokens.Add(refreshToken);

    /// <summary>
    /// Ghi chú: GetRefreshTokenWithUserByHashAsync lấy refresh token kèm tài khoản theo token hash.
    /// </summary>
    public Task<RefreshToken?> GetRefreshTokenWithUserByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    /// <summary>
    /// Ghi chú: RevokeRefreshTokenAtomicallyAsync thu hồi refresh token bằng atomic update.
    /// </summary>
    public Task<int> RevokeRefreshTokenAtomicallyAsync(
        Guid tokenId,
        Guid replacementTokenId,
        DateTime now,
        CancellationToken cancellationToken) =>
        dbContext.RefreshTokens
            .Where(token => token.Id == tokenId && token.RevokedAtUtc == null && token.ExpiresAtUtc > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(token => token.RevokedAtUtc, now)
                .SetProperty(token => token.ReplacedByTokenId, replacementTokenId), cancellationToken);

    /// <summary>
    /// Ghi chú: RevokeRefreshTokenFamilyAsync thu hồi toàn bộ refresh token cùng family.
    /// </summary>
    public async Task RevokeRefreshTokenFamilyAsync(Guid familyId, DateTime now, CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens
            .Where(token => token.FamilyId == familyId && token.RevokedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.RevokedAtUtc, now), cancellationToken);
    }

    /// <summary>
    /// Ghi chú: GetRefreshTokenByUserAndHashAsync lấy refresh token của user theo token hash.
    /// </summary>
    public Task<RefreshToken?> GetRefreshTokenByUserAndHashAsync(
        Guid userId,
        string tokenHash,
        CancellationToken cancellationToken) =>
        dbContext.RefreshTokens.SingleOrDefaultAsync(
            token => token.UserId == userId && token.TokenHash == tokenHash,
            cancellationToken);
}
