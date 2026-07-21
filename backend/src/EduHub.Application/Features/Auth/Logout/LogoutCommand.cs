using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Auth;
using EduHub.Application.Interfaces.Services.Authentication;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Auth.Logout;

/// <summary>
/// Ghi chú: LogoutCommandValidator kiểm tra dữ liệu đầu vào cho đăng xuất và thu hồi refresh token hiện tại trước khi handler chạy.
/// </summary>
public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo đăng xuất và thu hồi refresh token hiện tại và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public LogoutCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty().MaximumLength(512);
    }
}

/// <summary>
/// Ghi chú: LogoutCommandHandler xử lý đăng xuất và thu hồi refresh token hiện tại, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class LogoutCommandHandler(IAuthService authService)
    : IRequestHandler<LogoutCommand, Result>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command logout sang AuthService để thu hồi refresh token.
    /// </summary>
    public Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken) =>
        authService.LogoutAsync(request, cancellationToken);
}
