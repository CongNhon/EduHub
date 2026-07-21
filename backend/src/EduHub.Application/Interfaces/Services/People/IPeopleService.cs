using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.People;

namespace EduHub.Application.Interfaces.Services.People;

/// <summary>
/// Ghi chú: IPeopleService định nghĩa nghiệp vụ quản lý tài khoản người dùng nội bộ của trường.
/// </summary>
public interface IPeopleService
{
    Task<Result<PagedResult<UserAccountResponse>>> ListAsync(ListUserAccountsQuery request, CancellationToken cancellationToken);
    Task<Result<UserAccountResponse>> CreateAsync(CreateUserAccountCommand request, CancellationToken cancellationToken);
    Task<Result<UserAccountResponse>> UpdateAsync(UpdateUserAccountCommand request, CancellationToken cancellationToken);
}
