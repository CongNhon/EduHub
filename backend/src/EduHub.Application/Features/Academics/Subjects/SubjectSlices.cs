using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Academics;
using EduHub.Application.Interfaces.Services.Academics;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Academics.Subjects;

/// <summary>
/// Ghi chú: CreateSubjectCommandValidator kiểm tra dữ liệu đầu vào cho môn học mới trước khi handler chạy.
/// </summary>
public sealed class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo môn học mới và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public CreateSubjectCommandValidator()
    {
        RuleFor(command => command.SubjectCode).NotEmpty().MaximumLength(64);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(256);
        RuleFor(command => command.Credits).GreaterThan(0);
        RuleFor(command => command.MaxScore).GreaterThan(0m).When(command => command.MaxScore.HasValue);
    }
}

/// <summary>
/// Ghi chú: CreateSubjectCommandHandler xử lý môn học mới, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class CreateSubjectCommandHandler(IAcademicService academicService)
    : IRequestHandler<CreateSubjectCommand, Result<SubjectResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command tạo môn học sang AcademicService.
    /// </summary>
    public Task<Result<SubjectResponse>> Handle(CreateSubjectCommand request, CancellationToken cancellationToken) =>
        academicService.CreateSubjectAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: UpdateSubjectCommandValidator kiểm tra dữ liệu đầu vào cho môn học hiện có trước khi handler chạy.
/// </summary>
public sealed class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo môn học hiện có và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public UpdateSubjectCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(256);
        RuleFor(command => command.Credits).GreaterThan(0);
        RuleFor(command => command.MaxScore).GreaterThan(0m);
    }
}

/// <summary>
/// Ghi chú: UpdateSubjectCommandHandler xử lý môn học hiện có, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class UpdateSubjectCommandHandler(IAcademicService academicService)
    : IRequestHandler<UpdateSubjectCommand, Result<SubjectResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command cập nhật môn học sang AcademicService.
    /// </summary>
    public Task<Result<SubjectResponse>> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken) =>
        academicService.UpdateSubjectAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: DisableSubjectCommandValidator kiểm tra dữ liệu đầu vào cho môn học cần vô hiệu hóa trước khi handler chạy.
/// </summary>
public sealed class DisableSubjectCommandValidator : AbstractValidator<DisableSubjectCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo môn học cần vô hiệu hóa và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public DisableSubjectCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}

/// <summary>
/// Ghi chú: DisableSubjectCommandHandler xử lý môn học cần vô hiệu hóa, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class DisableSubjectCommandHandler(IAcademicService academicService)
    : IRequestHandler<DisableSubjectCommand, Result<SubjectResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command vô hiệu hóa môn học sang AcademicService.
    /// </summary>
    public Task<Result<SubjectResponse>> Handle(DisableSubjectCommand request, CancellationToken cancellationToken) =>
        academicService.DisableSubjectAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListSubjectsQueryHandler xử lý danh sách môn học, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class ListSubjectsQueryHandler(IAcademicService academicService)
    : IRequestHandler<ListSubjectsQuery, Result<PagedResult<SubjectResponse>>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển query danh sách môn học sang AcademicService.
    /// </summary>
    public Task<Result<PagedResult<SubjectResponse>>> Handle(
        ListSubjectsQuery request,
        CancellationToken cancellationToken) =>
        academicService.ListSubjectsAsync(request, cancellationToken);
}
