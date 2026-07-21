namespace EduHub.Application.Common.Models;

/// <summary>
/// Ghi chú: SortDirection liệt kê các giá trị hợp lệ cho sort direction.
/// </summary>
public enum SortDirection
{
    Ascending,
    Descending
}

/// <summary>
/// Ghi chú: PageRequest là dữ liệu HTTP request dùng cho tham số phân trang/tìm kiếm.
/// </summary>
public sealed record PageRequest
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int MaxSearchLength = 200;

    private PageRequest(int page, int pageSize, string? search, string? sortBy, SortDirection sortDirection)
    {
        Page = page;
        PageSize = pageSize;
        Search = search;
        SortBy = sortBy;
        SortDirection = sortDirection;
    }

    public int Page { get; }

    public int PageSize { get; }

    public string? Search { get; }

    public string? SortBy { get; }

    public SortDirection SortDirection { get; }

    public int Skip => (Page - 1) * PageSize;

    /// <summary>
    /// Ghi chú: Create tạo tham số phân trang/tìm kiếm sau khi validate các giá trị đầu vào.
    /// </summary>
    public static Result<PageRequest> Create(
        int page = DefaultPage,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = null,
        SortDirection sortDirection = SortDirection.Ascending,
        IReadOnlySet<string>? allowedSortFields = null)
    {
        if (page < DefaultPage)
        {
            return Result.Failure<PageRequest>(new Error("pagination.page_invalid", "Page must be at least 1.", ErrorType.Validation));
        }

        if (pageSize is < 1 or > MaxPageSize)
        {
            return Result.Failure<PageRequest>(new Error("pagination.page_size_invalid", "Page size must be between 1 and 100.", ErrorType.Validation));
        }

        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        if (normalizedSearch?.Length > MaxSearchLength)
        {
            return Result.Failure<PageRequest>(new Error("pagination.search_too_long", "Search exceeds the allowed length.", ErrorType.Validation));
        }

        var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? null : sortBy.Trim();
        if (normalizedSortBy is not null && allowedSortFields is not null &&
            !allowedSortFields.Any(field => string.Equals(field, normalizedSortBy, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure<PageRequest>(new Error("pagination.sort_invalid", "Sort field is not allowed.", ErrorType.Validation));
        }

        return Result.Success(new PageRequest(page, pageSize, normalizedSearch, normalizedSortBy, sortDirection));
    }
}
