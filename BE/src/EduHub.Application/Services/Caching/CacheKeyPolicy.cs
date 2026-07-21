using EduHub.Application.Interfaces.Services.Caching;

namespace EduHub.Application.Services.Caching;

/// <summary>
/// Ghi chú: CacheKeyPolicy tạo key Redis ổn định cho subject catalog và published grade query.
/// </summary>
public sealed class CacheKeyPolicy : ICacheKeyPolicy
{
    /// <summary>
    /// Ghi chú: SubjectCatalog tạo key cache danh sách môn học theo version/filter/page/search.
    /// </summary>
    public string SubjectCatalog(int version, bool? isActive, int page, int pageSize, string? search) =>
        $"v{version}:subjects:active-{isActive?.ToString().ToLowerInvariant() ?? "all"}:p-{page}:ps-{pageSize}:q-{Normalize(search)}";

    /// <summary>
    /// Ghi chú: PublishedGrades tạo key cache điểm published/locked theo phụ huynh, học sinh và assignment.
    /// </summary>
    public string PublishedGrades(int version, Guid parentUserId, Guid studentId, Guid assignmentId) =>
        $"v{version}:published-grades:parent-{parentUserId:N}:student-{studentId:N}:assignment-{assignmentId:N}";

    /// <summary>
    /// Ghi chú: SubjectCatalogScope trả scope version cho toàn bộ cache danh sách môn học.
    /// </summary>
    public string SubjectCatalogScope() => "subjects";

    /// <summary>
    /// Ghi chú: PublishedGradesScope trả scope version cho cache điểm đã công bố của một assignment.
    /// </summary>
    public string PublishedGradesScope(Guid assignmentId) => $"published-grades:{assignmentId:N}";

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "none" : Uri.EscapeDataString(value.Trim().ToLowerInvariant());
}
