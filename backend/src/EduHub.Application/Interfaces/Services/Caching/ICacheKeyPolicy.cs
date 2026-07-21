namespace EduHub.Application.Interfaces.Services.Caching;

/// <summary>
/// Ghi chú: ICacheKeyPolicy tạo cache key có version để tránh trả dữ liệu cũ sau khi dữ liệu gốc thay đổi.
/// </summary>
public interface ICacheKeyPolicy
{
    /// <summary>
    /// Ghi chú: SubjectCatalog tạo cache key cho danh sách môn học theo filter/phân trang.
    /// </summary>
    string SubjectCatalog(int version, bool? isActive, int page, int pageSize, string? search);

    /// <summary>
    /// Ghi chú: PublishedGrades tạo cache key cho điểm đã công bố của một phụ huynh-học sinh-assignment.
    /// </summary>
    string PublishedGrades(int version, Guid parentUserId, Guid studentId, Guid assignmentId);

    /// <summary>
    /// Ghi chú: SubjectCatalogScope là scope version dùng để invalidate cache danh sách môn học.
    /// </summary>
    string SubjectCatalogScope();

    /// <summary>
    /// Ghi chú: PublishedGradesScope là scope version dùng để invalidate cache điểm đã công bố của một assignment.
    /// </summary>
    string PublishedGradesScope(Guid assignmentId);
}
