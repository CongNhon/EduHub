using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Grades;
using EduHub.Application.Interfaces.Services.Grades;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Grades.GradeConfigurations;

/// <summary>
/// Ghi chú: CreateGradeConfigurationCommandValidator kiểm tra request tạo cấu hình thành phần điểm trước khi handler chạy.
/// </summary>
public sealed class CreateGradeConfigurationCommandValidator : AbstractValidator<CreateGradeConfigurationCommand>
{
    /// <summary>
    /// Ghi chú: Constructor kiểm tra subject, semester và từng component điểm trong cấu hình mới.
    /// </summary>
    public CreateGradeConfigurationCommandValidator()
    {
        RuleFor(command => command.SubjectId).NotEmpty();
        RuleFor(command => command.SemesterId).NotEmpty();
        RuleFor(command => command.Components).NotEmpty();
        RuleForEach(command => command.Components).ChildRules(component =>
        {
            component.RuleFor(item => item.Name).NotEmpty().MaximumLength(128);
            component.RuleFor(item => item.Weight).GreaterThan(0m).LessThanOrEqualTo(1m);
            component.RuleFor(item => item.MaxScore).GreaterThan(0m);
            component.RuleFor(item => item.DisplayOrder).GreaterThan(0);
        });
    }
}

/// <summary>
/// Ghi chú: CreateGradeConfigurationCommandHandler tạo version cấu hình thành phần điểm mới qua service.
/// </summary>
public sealed class CreateGradeConfigurationCommandHandler(IGradeConfigurationService gradeConfigurationService)
    : IRequestHandler<CreateGradeConfigurationCommand, Result<GradeConfigurationResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command tạo cấu hình điểm sang GradeConfigurationService.
    /// </summary>
    public Task<Result<GradeConfigurationResponse>> Handle(
        CreateGradeConfigurationCommand request,
        CancellationToken cancellationToken) =>
        gradeConfigurationService.CreateGradeConfigurationAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListGradeConfigurationsQueryHandler đọc danh sách cấu hình thành phần điểm qua service.
/// </summary>
public sealed class ListGradeConfigurationsQueryHandler(IGradeConfigurationService gradeConfigurationService)
    : IRequestHandler<ListGradeConfigurationsQuery, Result<PagedResult<GradeConfigurationResponse>>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển query danh sách cấu hình điểm sang GradeConfigurationService.
    /// </summary>
    public Task<Result<PagedResult<GradeConfigurationResponse>>> Handle(
        ListGradeConfigurationsQuery request,
        CancellationToken cancellationToken) =>
        gradeConfigurationService.ListGradeConfigurationsAsync(request, cancellationToken);
}
