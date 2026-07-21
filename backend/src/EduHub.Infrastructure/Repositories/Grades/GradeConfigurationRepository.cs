using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Grades;
using EduHub.Application.Interfaces.Repositories.Grades;
using EduHub.Domain.Entities.Academics;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Grades;

/// <summary>
/// Ghi chú: GradeConfigurationRepository dùng EF Core để truy cập cấu hình thành phần điểm.
/// </summary>
public sealed class GradeConfigurationRepository(ApplicationDbContext dbContext) : IGradeConfigurationRepository
{
    /// <summary>
    /// Ghi chú: SubjectExistsAsync kiểm tra môn học tồn tại và đang active.
    /// </summary>
    public Task<bool> SubjectExistsAsync(Guid subjectId, CancellationToken cancellationToken) =>
        dbContext.Subjects.AnyAsync(subject => subject.Id == subjectId && subject.IsActive, cancellationToken);

    /// <summary>
    /// Ghi chú: SemesterExistsAsync kiểm tra học kỳ tồn tại.
    /// </summary>
    public Task<bool> SemesterExistsAsync(Guid semesterId, CancellationToken cancellationToken) =>
        dbContext.Semesters.AnyAsync(semester => semester.Id == semesterId, cancellationToken);

    /// <summary>
    /// Ghi chú: GetNextVersionAsync lấy version tiếp theo dựa trên version lớn nhất hiện có.
    /// </summary>
    public async Task<int> GetNextVersionAsync(Guid subjectId, Guid semesterId, CancellationToken cancellationToken)
    {
        var currentVersion = await dbContext.GradeComponents
            .Where(component => component.SubjectId == subjectId && component.SemesterId == semesterId)
            .Select(component => (int?)component.Version)
            .MaxAsync(cancellationToken);

        return (currentVersion ?? 0) + 1;
    }

    /// <summary>
    /// Ghi chú: DeactivateActiveComponentsAsync tắt các component active cũ trước khi tạo version mới.
    /// </summary>
    public async Task DeactivateActiveComponentsAsync(
        Guid subjectId,
        Guid semesterId,
        DateTime updatedAtUtc,
        CancellationToken cancellationToken)
    {
        var activeComponents = await dbContext.GradeComponents
            .Where(component => component.SubjectId == subjectId && component.SemesterId == semesterId && component.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var component in activeComponents)
        {
            component.Deactivate(updatedAtUtc);
        }
    }

    /// <summary>
    /// Ghi chú: AddRange thêm danh sách GradeComponent mới vào DbContext.
    /// </summary>
    public void AddRange(IReadOnlyList<GradeComponent> components) =>
        dbContext.GradeComponents.AddRange(components);

    /// <summary>
    /// Ghi chú: ListConfigurationsAsync gom GradeComponent theo subject-semester-version để trả cấu hình điểm.
    /// </summary>
    public async Task<PagedResult<GradeConfigurationResponse>> ListConfigurationsAsync(
        Guid? subjectId,
        Guid? semesterId,
        bool? isActive,
        PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        var query = dbContext.GradeComponents.AsNoTracking();
        if (subjectId.HasValue)
        {
            query = query.Where(component => component.SubjectId == subjectId.Value);
        }

        if (semesterId.HasValue)
        {
            query = query.Where(component => component.SemesterId == semesterId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(component => component.IsActive == isActive.Value);
        }

        var grouped = query
            .GroupBy(component => new { component.SubjectId, component.SemesterId, component.Version, component.IsActive })
            .Select(group => new
            {
                group.Key.SubjectId,
                group.Key.SemesterId,
                group.Key.Version,
                group.Key.IsActive
            });

        var total = await grouped.LongCountAsync(cancellationToken);
        var page = await grouped
            .OrderByDescending(item => item.IsActive)
            .ThenBy(item => item.SubjectId)
            .ThenBy(item => item.SemesterId)
            .ThenByDescending(item => item.Version)
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .ToListAsync(cancellationToken);

        var items = new List<GradeConfigurationResponse>();
        foreach (var configuration in page)
        {
            var components = await query
                .Where(component =>
                    component.SubjectId == configuration.SubjectId &&
                    component.SemesterId == configuration.SemesterId &&
                    component.Version == configuration.Version)
                .OrderBy(component => component.DisplayOrder)
                .Select(component => new GradeComponentResponse(
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
                    component.IsActive))
                .ToListAsync(cancellationToken);

            items.Add(new GradeConfigurationResponse(
                configuration.SubjectId,
                configuration.SemesterId,
                configuration.Version,
                configuration.IsActive,
                components.Sum(component => component.Weight),
                components));
        }

        return new PagedResult<GradeConfigurationResponse>(items, pageRequest.Page, pageRequest.PageSize, total);
    }
}
