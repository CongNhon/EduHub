namespace EduHub.Application.Common.Models;

/// <summary>
/// Ghi chú: PagedResult đại diện cho kết quả danh sách có phân trang trong hệ thống EduHub.
/// </summary>
public sealed record PagedResult<T>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo kết quả danh sách có phân trang và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, long totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }

    public int Page { get; }

    public int PageSize { get; }

    public long TotalCount { get; }

    public long TotalPages => TotalCount / PageSize + (TotalCount % PageSize == 0 ? 0 : 1);
}
