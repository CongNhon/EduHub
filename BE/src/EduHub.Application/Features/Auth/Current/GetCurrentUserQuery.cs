using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Auth;
using EduHub.Application.Interfaces.Services.Authentication;
using MediatR;

namespace EduHub.Application.Features.Auth.Current;

/// <summary>
/// Ghi chú: GetCurrentUserQueryHandler xử lý thông tin người dùng hiện tại từ JWT, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class GetCurrentUserQueryHandler(IAuthService authService)
    : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển query user hiện tại sang AuthService để đọc dữ liệu từ JWT.
    /// </summary>
    public Task<Result<CurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken) =>
        authService.GetCurrentUserAsync(cancellationToken);
}
