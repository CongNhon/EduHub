using EduHub.Application.Contracts.People;
using EduHub.WebApi.Dtos.People;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: PeopleMappings chuyển DTO quản lý tài khoản sang Application contracts và ngược lại.
/// </summary>
public static class PeopleMappings
{
    public static ListUserAccountsQuery ToQuery(this ListUserAccountsRequest request) =>
        new(request.Role, request.IsActive, request.Search, request.Page ?? 1, request.PageSize ?? 20);

    public static CreateUserAccountCommand ToCommand(this CreateUserAccountRequest request) =>
        new(request.Email, request.Password, request.FullName, request.ReferenceCode, request.PhoneNumber, request.Role);

    public static UpdateUserAccountCommand ToCommand(this UpdateUserAccountRequest request, Guid id) =>
        new(id, request.FullName, request.ReferenceCode, request.PhoneNumber, request.Role, request.IsActive, request.ChangeReason);

    public static UserAccountDto ToDto(this UserAccountResponse response) =>
        new(response.Id, response.Email, response.FullName, response.ReferenceCode, response.PhoneNumber, response.Role, response.IsActive);
}
