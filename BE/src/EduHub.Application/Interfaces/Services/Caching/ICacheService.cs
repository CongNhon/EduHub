using EduHub.Application.Common.Caching;

namespace EduHub.Application.Interfaces.Services.Caching;

/// <summary>
/// Ghi chú: ICacheService là interface đọc/ghi cache Redis cho subject catalog và published grade query.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Ghi chú: GetOrCreateAsync lấy dữ liệu cache theo key, nếu miss hoặc Redis lỗi thì chạy factory đọc database.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheEntryOptions options,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: RemoveAsync xóa một cache key cụ thể.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetVersionAsync đọc version cache của một scope như subject catalog hoặc published gradebook.
    /// </summary>
    Task<int> GetVersionAsync(string scope, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: BumpVersionAsync tăng version cache của một scope để vô hiệu hóa các key cũ.
    /// </summary>
    Task<int> BumpVersionAsync(string scope, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetMetrics trả số liệu hit/miss/failure của cache adapter hiện tại.
    /// </summary>
    CacheMetricsSnapshot GetMetrics();
}
