using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Auth;
using EduHub.Application.Interfaces.Services.Authentication;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Auth.Login;

/// <summary>
/// Ghi chú: LoginCommandValidator kiểm tra dữ liệu đầu vào cho đăng nhập bằng email/password trước khi handler chạy.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo đăng nhập bằng email/password và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.Password).NotEmpty().MaximumLength(256);
        RuleFor(command => command.DeviceId).MaximumLength(256);
    }
}

/// <summary>
/// Ghi chú: LoginCommandHandler xử lý đăng nhập bằng email/password, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class LoginCommandHandler(IAuthService authService)
    : IRequestHandler<LoginCommand, Result<AuthTokenResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command đăng nhập sang AuthService để xử lý xác thực và cấp token.
    /// </summary>
    public Task<Result<AuthTokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken) =>
        authService.LoginAsync(request, cancellationToken);
}
