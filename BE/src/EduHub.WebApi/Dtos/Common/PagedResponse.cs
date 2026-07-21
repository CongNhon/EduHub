namespace EduHub.WebApi.Dtos.Common;

/// <summary>
/// Ghi chú: PagedResponse là DTO response chuẩn cho danh sách API có phân trang.
/// </summary>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages);
