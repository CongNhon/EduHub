using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.People;
using EduHub.Application.Interfaces.Services.People;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.People;

/// <summary>
/// Ghi chú: CreateUserAccountCommandValidator kiểm tra tài khoản người dùng mới trước khi SystemAdmin tạo.
/// </summary>
public sealed class CreateUserAccountCommandValidator : AbstractValidator<CreateUserAccountCommand>
{
    public CreateUserAccountCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.Password).MinimumLength(12).MaximumLength(128);
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(256);
        RuleFor(command => command.ReferenceCode).MaximumLength(64);
        RuleFor(command => command.PhoneNumber).MaximumLength(32);
        RuleFor(command => command.Role).IsInEnum();
    }
}

/// <summary>
/// Ghi chu: UpdateUserAccountCommandValidator kiem tra ho so, role va trang thai tai khoan truoc khi SystemAdmin cap nhat.
/// </summary>
public sealed class UpdateUserAccountCommandValidator : AbstractValidator<UpdateUserAccountCommand>
{
    /// <summary>
    /// Ghi chu: Constructor khai bao rule cho tai khoan can cap nhat.
    /// </summary>
    public UpdateUserAccountCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(256);
        RuleFor(command => command.ReferenceCode).MaximumLength(64);
        RuleFor(command => command.PhoneNumber).MaximumLength(32);
        RuleFor(command => command.Role).IsInEnum();
        RuleFor(command => command.ChangeReason).NotEmpty().MinimumLength(10).MaximumLength(500);
    }
}

/// <summary>
/// Ghi chú: CreateUserAccountCommandHandler chuyển yêu cầu tạo tài khoản sang PeopleService.
/// </summary>
public sealed class CreateUserAccountCommandHandler(IPeopleService service) : IRequestHandler<CreateUserAccountCommand, Result<UserAccountResponse>>
{
    public Task<Result<UserAccountResponse>> Handle(CreateUserAccountCommand request, CancellationToken cancellationToken) => service.CreateAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: UpdateUserAccountCommandHandler chuyển yêu cầu cập nhật tài khoản sang PeopleService.
/// </summary>
public sealed class UpdateUserAccountCommandHandler(IPeopleService service) : IRequestHandler<UpdateUserAccountCommand, Result<UserAccountResponse>>
{
    public Task<Result<UserAccountResponse>> Handle(UpdateUserAccountCommand request, CancellationToken cancellationToken) => service.UpdateAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListUserAccountsQueryHandler đọc danh sách người dùng qua PeopleService.
/// </summary>
public sealed class ListUserAccountsQueryHandler(IPeopleService service) : IRequestHandler<ListUserAccountsQuery, Result<PagedResult<UserAccountResponse>>>
{
    public Task<Result<PagedResult<UserAccountResponse>>> Handle(ListUserAccountsQuery request, CancellationToken cancellationToken) => service.ListAsync(request, cancellationToken);
}
