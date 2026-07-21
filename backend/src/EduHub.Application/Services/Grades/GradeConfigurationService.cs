using EduHub.Application.Common.Errors;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Grades;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.Grades;
using EduHub.Application.Interfaces.Services.Grades;
using EduHub.Domain.Entities.Academics;

namespace EduHub.Application.Services.Grades;

/// <summary>
/// Ghi chú: GradeConfigurationService xử lý nghiệp vụ tạo và đọc cấu hình thành phần điểm.
/// </summary>
public sealed class GradeConfigurationService(
    IGradeConfigurationRepository gradeConfigurationRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IGradeConfigurationService
{
    /// <summary>
    /// Ghi chú: CreateGradeConfigurationAsync tạo version cấu hình active mới nếu component hợp lệ và tổng weight bằng 1.00.
    /// </summary>
    public async Task<Result<GradeConfigurationResponse>> CreateGradeConfigurationAsync(
        CreateGradeConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        if (!await gradeConfigurationRepository.SubjectExistsAsync(request.SubjectId, cancellationToken))
        {
            return Result.Failure<GradeConfigurationResponse>(GradeErrors.SubjectNotFound);
        }

        if (!await gradeConfigurationRepository.SemesterExistsAsync(request.SemesterId, cancellationToken))
        {
            return Result.Failure<GradeConfigurationResponse>(GradeErrors.SemesterNotFound);
        }

        var normalizedNames = request.Components.Select(component => Normalize(component.Name)).ToList();
        if (normalizedNames.Count != normalizedNames.Distinct(StringComparer.Ordinal).Count())
        {
            return Result.Failure<GradeConfigurationResponse>(GradeErrors.DuplicateComponentName);
        }

        var orders = request.Components.Select(component => component.DisplayOrder).ToList();
        if (orders.Count != orders.Distinct().Count())
        {
            return Result.Failure<GradeConfigurationResponse>(GradeErrors.DuplicateComponentOrder);
        }

        var totalWeight = request.Components.Sum(component => component.Weight);
        if (totalWeight != 1.00m)
        {
            return Result.Failure<GradeConfigurationResponse>(GradeErrors.InvalidComponentWeights);
        }

        var version = await gradeConfigurationRepository.GetNextVersionAsync(
            request.SubjectId,
            request.SemesterId,
            cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        await gradeConfigurationRepository.DeactivateActiveComponentsAsync(request.SubjectId, request.SemesterId, now, cancellationToken);

        var components = request.Components
            .OrderBy(component => component.DisplayOrder)
            .Select(component => new GradeComponent(
                request.SubjectId,
                request.SemesterId,
                component.Name,
                Normalize(component.Name),
                component.Weight,
                component.MaxScore,
                component.DisplayOrder,
                component.IsRequired,
                component.IncludeInGpa,
                version))
            .ToList();

        gradeConfigurationRepository.AddRange(components);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(request.SubjectId, request.SemesterId, version, true, components));
    }

    /// <summary>
    /// Ghi chú: ListGradeConfigurationsAsync đọc danh sách version cấu hình điểm theo filter và phân trang.
    /// </summary>
    public async Task<Result<PagedResult<GradeConfigurationResponse>>> ListGradeConfigurationsAsync(
        ListGradeConfigurationsQuery request,
        CancellationToken cancellationToken)
    {
        var pageRequest = PageRequest.Create(request.Page, request.PageSize, null);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<PagedResult<GradeConfigurationResponse>>(pageRequest.Error!);
        }

        return Result.Success(await gradeConfigurationRepository.ListConfigurationsAsync(
            request.SubjectId,
            request.SemesterId,
            request.IsActive,
            pageRequest.Value,
            cancellationToken));
    }

    private static GradeConfigurationResponse ToResponse(
        Guid subjectId,
        Guid semesterId,
        int version,
        bool isActive,
        IReadOnlyList<GradeComponent> components) =>
        new(
            subjectId,
            semesterId,
            version,
            isActive,
            components.Sum(component => component.Weight),
            components.Select(ToResponse).ToList());

    private static GradeComponentResponse ToResponse(GradeComponent component) =>
        new(
            component.Id,
            component.SubjectId,
            component.SemesterId,
            component.Name,
            component.Weight,
            component.MaxScore,
            component.DisplayOrder,
            component.IsRequired,
            component.IncludeInGpa,
            component.Version,
            component.IsActive);

    private static string Normalize(string value) =>
        value.Trim().ToUpperInvariant();
}
