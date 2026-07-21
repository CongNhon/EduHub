using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Students;
using EduHub.Application.Interfaces.Services.Students;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Students.ParentLinks;

/// <summary>
/// Ghi chú: LinkParentStudentCommandValidator kiểm tra dữ liệu đầu vào cho liên kết phụ huynh với học sinh trước khi handler chạy.
/// </summary>
public sealed class LinkParentStudentCommandValidator : AbstractValidator<LinkParentStudentCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo liên kết phụ huynh với học sinh và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public LinkParentStudentCommandValidator()
    {
        RuleFor(command => command.StudentId).NotEmpty();
        RuleFor(command => command.ParentUserId).NotEmpty();
        RuleFor(command => command.Relationship).NotEmpty().MaximumLength(64);
    }
}

/// <summary>
/// Ghi chú: LinkParentStudentCommandHandler xử lý liên kết phụ huynh với học sinh, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class LinkParentStudentCommandHandler(IStudentService studentService)
    : IRequestHandler<LinkParentStudentCommand, Result<ParentStudentResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command gắn phụ huynh-học sinh sang StudentService.
    /// </summary>
    public Task<Result<ParentStudentResponse>> Handle(
        LinkParentStudentCommand request,
        CancellationToken cancellationToken) =>
        studentService.LinkParentStudentAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: UnlinkParentStudentCommandValidator kiểm tra dữ liệu đầu vào cho ngừng liên kết phụ huynh-học sinh trước khi handler chạy.
/// </summary>
public sealed class UnlinkParentStudentCommandValidator : AbstractValidator<UnlinkParentStudentCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo ngừng liên kết phụ huynh-học sinh và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public UnlinkParentStudentCommandValidator()
    {
        RuleFor(command => command.StudentId).NotEmpty();
        RuleFor(command => command.ParentUserId).NotEmpty();
    }
}

/// <summary>
/// Ghi chú: UnlinkParentStudentCommandHandler xử lý ngừng liên kết phụ huynh-học sinh, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class UnlinkParentStudentCommandHandler(IStudentService studentService)
    : IRequestHandler<UnlinkParentStudentCommand, Result>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command ngừng liên kết phụ huynh-học sinh sang StudentService.
    /// </summary>
    public Task<Result> Handle(UnlinkParentStudentCommand request, CancellationToken cancellationToken) =>
        studentService.UnlinkParentStudentAsync(request, cancellationToken);
}
