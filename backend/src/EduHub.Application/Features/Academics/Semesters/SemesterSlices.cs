using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Academics;
using EduHub.Application.Interfaces.Services.Academics;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Academics.Semesters;

/// <summary>
/// Ghi chú: CreateSemesterCommandValidator kiểm tra dữ liệu đầu vào cho học kỳ mới trước khi handler chạy.
/// </summary>
public sealed class CreateSemesterCommandValidator : AbstractValidator<CreateSemesterCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo học kỳ mới và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public CreateSemesterCommandValidator()
    {
        RuleFor(command => command.AcademicYearId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(128);
        RuleFor(command => command.StartDate)
            .LessThan(command => command.EndDate)
            .WithMessage("StartDate must be before EndDate.");
        RuleFor(command => command.GradeEntryFrom)
            .LessThanOrEqualTo(command => command.GradeEntryTo)
            .WithMessage("Grade entry window is invalid.");
    }
}

/// <summary>
/// Ghi chú: CreateSemesterCommandHandler xử lý học kỳ mới, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class CreateSemesterCommandHandler(IAcademicService academicService)
    : IRequestHandler<CreateSemesterCommand, Result<SemesterResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command tạo học kỳ sang AcademicService.
    /// </summary>
    public Task<Result<SemesterResponse>> Handle(CreateSemesterCommand request, CancellationToken cancellationToken) =>
        academicService.CreateSemesterAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListSemestersQueryHandler xử lý danh sách học kỳ, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class ListSemestersQueryHandler(IAcademicService academicService)
    : IRequestHandler<ListSemestersQuery, Result<PagedResult<SemesterResponse>>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển query danh sách học kỳ sang AcademicService.
    /// </summary>
    public Task<Result<PagedResult<SemesterResponse>>> Handle(
        ListSemestersQuery request,
        CancellationToken cancellationToken) =>
        academicService.ListSemestersAsync(request, cancellationToken);
}
