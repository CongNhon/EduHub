using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Auth;
using EduHub.Application.Interfaces.Services.Authentication;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Auth.Refresh;

/// <summary>
/// Ghi chú: RefreshTokenCommandValidator kiểm tra dữ liệu đầu vào cho xoay vòng refresh token và cấp token mới trước khi handler chạy.
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo xoay vòng refresh token và cấp token mới và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty().MaximumLength(512);
        RuleFor(command => command.DeviceId).MaximumLength(256);
    }
}

/// <summary>
/// Ghi chú: RefreshTokenCommandHandler xử lý xoay vòng refresh token và cấp token mới, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class RefreshTokenCommandHandler(IAuthService authService)
    : IRequestHandler<RefreshTokenCommand, Result<AuthTokenResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command refresh token sang AuthService để xoay vòng token.
    /// </summary>
    public Task<Result<AuthTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken) =>
        authService.RefreshAsync(request, cancellationToken);
}
