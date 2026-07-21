using Carter;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.People;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.People;

/// <summary>
/// Ghi chú: PeopleModule đăng ký API quản lý tài khoản giáo viên, phụ huynh, học sinh và quản trị viên.
/// </summary>
public sealed class PeopleModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes phân quyền đọc tài khoản cho học vụ và thay đổi tài khoản cho SystemAdmin.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users").WithTags("People");
        group.MapGet("/", ListAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("ListUserAccounts");
        group.MapPost("/", CreateAsync).RequireAuthorization(AuthPolicies.SystemAdmin).WithName("CreateUserAccount");
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(AuthPolicies.SystemAdmin).WithName("UpdateUserAccount");
    }

    private static async Task<IResult> ListAsync([AsParameters] ListUserAccountsRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken)).ToHttpResult(result => result.ToPagedResponse(PeopleMappings.ToDto));

    private static async Task<IResult> CreateAsync(CreateUserAccountRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result.ToCreatedHttpResult(response => $"/api/v1/users/{response.Id}", PeopleMappings.ToDto);
    }

    private static async Task<IResult> UpdateAsync(Guid id, UpdateUserAccountRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(id), cancellationToken)).ToHttpResult(PeopleMappings.ToDto);
}
