using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Auth;
using EduHub.Application.Features.Auth.Common;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.Authentication;
using EduHub.Application.Interfaces.Services.Authentication;
using EduHub.Domain.Entities.Identity;
using Microsoft.Extensions.Options;

namespace EduHub.Application.Services.Authentication;

/// <summary>
/// Ghi chú: AuthService xử lý nghiệp vụ đăng nhập, refresh token, logout và đọc user hiện tại.
/// </summary>
public sealed class AuthService(
    IAuthRepository authRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider)
    : IAuthService
{
    /// <summary>
    /// Ghi chú: LoginAsync kiểm tra email/password, user active và cấp cặp token cho user.
    /// </summary>
    public async Task<Result<AuthTokenResponse>> LoginAsync(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await authRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !passwordHashService.VerifyPassword(user.PasswordHash, request.Password))
        {
            return Result.Failure<AuthTokenResponse>(AuthErrors.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthTokenResponse>(AuthErrors.InvalidCredentials);
        }

        if (passwordHashService.NeedsRehash(user.PasswordHash))
        {
            user.UpdatePasswordHash(
                passwordHashService.HashPassword(request.Password),
                timeProvider.GetUtcNow().UtcDateTime);
        }

        var response = await IssueTokenPairAsync(user, request.DeviceId, cancellationToken);
        return Result.Success(response);
    }

    /// <summary>
    /// Ghi chú: RefreshAsync xoay vòng refresh token và thu hồi family nếu phát hiện token reuse.
    /// </summary>
    public async Task<Result<AuthTokenResponse>> RefreshAsync(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var tokenHash = refreshTokenService.HashToken(request.RefreshToken);
        var storedToken = await authRepository.GetRefreshTokenWithUserByHashAsync(tokenHash, cancellationToken);

        if (storedToken is null)
        {
            return Result.Failure<AuthTokenResponse>(AuthErrors.InvalidRefreshToken);
        }

        if (storedToken.IsRevoked)
        {
            await authRepository.RevokeRefreshTokenFamilyAsync(storedToken.FamilyId, now, cancellationToken);
            return Result.Failure<AuthTokenResponse>(AuthErrors.InvalidRefreshToken);
        }

        if (!storedToken.IsActive(now) || !storedToken.User.IsActive)
        {
            return Result.Failure<AuthTokenResponse>(AuthErrors.InvalidRefreshToken);
        }

        var rawRefreshToken = refreshTokenService.GenerateToken();
        var refreshTokenExpiresAtUtc = now.AddDays(jwtOptions.Value.RefreshTokenDays);
        var replacement = new RefreshToken(
            storedToken.UserId,
            refreshTokenService.HashToken(rawRefreshToken),
            storedToken.FamilyId,
            refreshTokenExpiresAtUtc,
            request.DeviceId ?? storedToken.DeviceId);

        var revokedCount = await authRepository.RevokeRefreshTokenAtomicallyAsync(
            storedToken.Id,
            replacement.Id,
            now,
            cancellationToken);

        if (revokedCount == 0)
        {
            await authRepository.RevokeRefreshTokenFamilyAsync(storedToken.FamilyId, now, cancellationToken);
            return Result.Failure<AuthTokenResponse>(AuthErrors.InvalidRefreshToken);
        }

        authRepository.AddRefreshToken(replacement);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenService.CreateAccessToken(storedToken.User);
        return Result.Success(new AuthTokenResponse(
            accessToken.AccessToken,
            accessToken.ExpiresAtUtc,
            rawRefreshToken,
            refreshTokenExpiresAtUtc));
    }

    /// <summary>
    /// Ghi chú: LogoutAsync thu hồi refresh token do client sở hữu, kể cả khi access token đã hết hạn.
    /// </summary>
    public async Task<Result> LogoutAsync(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenService.HashToken(request.RefreshToken);
        var storedToken = await authRepository.GetRefreshTokenWithUserByHashAsync(tokenHash, cancellationToken);

        if (storedToken is null || storedToken.IsRevoked)
        {
            return Result.Success();
        }

        storedToken.Revoke(timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Ghi chú: GetCurrentUserAsync trả thông tin user đang đăng nhập từ CurrentUser service.
    /// </summary>
    public async Task<Result<CurrentUserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue || currentUser.Role is null)
        {
            return Result.Failure<CurrentUserResponse>(AuthErrors.Unauthorized);
        }

        var user = await authRepository.GetUserByIdAsync(currentUser.UserId.Value, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result.Failure<CurrentUserResponse>(AuthErrors.Unauthorized);
        }

        return Result.Success(new CurrentUserResponse(
            user.Email,
            user.FullName,
            user.Role.ToString()));
    }

    /// <summary>
    /// Ghi chú: IssueTokenPairAsync tạo access token, refresh token và lưu refresh token hash.
    /// </summary>
    private async Task<AuthTokenResponse> IssueTokenPairAsync(
        User user,
        string? deviceId,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var accessToken = jwtTokenService.CreateAccessToken(user);
        var refreshToken = refreshTokenService.GenerateToken();
        var refreshTokenExpiresAtUtc = now.AddDays(jwtOptions.Value.RefreshTokenDays);

        authRepository.AddRefreshToken(new RefreshToken(
            user.Id,
            refreshTokenService.HashToken(refreshToken),
            Guid.NewGuid(),
            refreshTokenExpiresAtUtc,
            deviceId));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokenResponse(
            accessToken.AccessToken,
            accessToken.ExpiresAtUtc,
            refreshToken,
            refreshTokenExpiresAtUtc);
    }

    /// <summary>
    /// Ghi chú: NormalizeEmail chuẩn hóa email đăng nhập để so khớp không phân biệt hoa thường.
    /// </summary>
    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
