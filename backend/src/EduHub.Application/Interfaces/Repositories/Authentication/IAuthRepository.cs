using EduHub.Domain.Entities.Identity;

namespace EduHub.Application.Interfaces.Repositories.Authentication;

/// <summary>
/// Ghi chú: IAuthRepository là interface truy cập dữ liệu tài khoản và refresh token cho nghiệp vụ xác thực.
/// </summary>
public interface IAuthRepository
{
    /// <summary>
    /// Ghi chú: GetUserByNormalizedEmailAsync lấy tài khoản người dùng theo email đã chuẩn hóa.
    /// </summary>
    Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chu: GetUserByIdAsync lay tai khoan hien tai theo user id da xac thuc.
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: AddRefreshToken thêm refresh token mới cho tài khoản người dùng.
    /// </summary>
    void AddRefreshToken(RefreshToken refreshToken);

    /// <summary>
    /// Ghi chú: GetRefreshTokenWithUserByHashAsync lấy refresh token kèm user theo token hash.
    /// </summary>
    Task<RefreshToken?> GetRefreshTokenWithUserByHashAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: RevokeRefreshTokenAtomicallyAsync thu hồi một refresh token nếu token vẫn active.
    /// </summary>
    Task<int> RevokeRefreshTokenAtomicallyAsync(Guid tokenId, Guid replacementTokenId, DateTime now, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: RevokeRefreshTokenFamilyAsync thu hồi toàn bộ refresh token trong cùng family.
    /// </summary>
    Task RevokeRefreshTokenFamilyAsync(Guid familyId, DateTime now, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetRefreshTokenByUserAndHashAsync lấy refresh token của một user theo token hash.
    /// </summary>
    Task<RefreshToken?> GetRefreshTokenByUserAndHashAsync(Guid userId, string tokenHash, CancellationToken cancellationToken);
}
