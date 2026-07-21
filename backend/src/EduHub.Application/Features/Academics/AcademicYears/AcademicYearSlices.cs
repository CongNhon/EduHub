using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Academics;
using EduHub.Application.Interfaces.Services.Academics;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Academics.AcademicYears;

/// <summary>
/// Ghi chú: CreateAcademicYearCommandValidator kiểm tra dữ liệu đầu vào cho năm học mới trước khi handler chạy.
/// </summary>
public sealed class CreateAcademicYearCommandValidator : AbstractValidator<CreateAcademicYearCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo năm học mới và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public CreateAcademicYearCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(128);
        RuleFor(command => command.StartDate)
            .LessThan(command => command.EndDate)
            .WithMessage("StartDate must be before EndDate.");
    }
}

/// <summary>
/// Ghi chú: CreateAcademicYearCommandHandler xử lý năm học mới, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class CreateAcademicYearCommandHandler(IAcademicService academicService)
    : IRequestHandler<CreateAcademicYearCommand, Result<AcademicYearResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command tạo năm học sang AcademicService.
    /// </summary>
    public Task<Result<AcademicYearResponse>> Handle(CreateAcademicYearCommand request, CancellationToken cancellationToken) =>
        academicService.CreateAcademicYearAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListAcademicYearsQueryHandler xử lý danh sách năm học, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class ListAcademicYearsQueryHandler(IAcademicService academicService)
    : IRequestHandler<ListAcademicYearsQuery, Result<PagedResult<AcademicYearResponse>>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển query danh sách năm học sang AcademicService.
    /// </summary>
    public Task<Result<PagedResult<AcademicYearResponse>>> Handle(
        ListAcademicYearsQuery request,
        CancellationToken cancellationToken) =>
        academicService.ListAcademicYearsAsync(request, cancellationToken);
}
