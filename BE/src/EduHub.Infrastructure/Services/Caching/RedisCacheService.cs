using System.Collections.Concurrent;
using System.Text.Json;
using EduHub.Application.Common.Caching;
using EduHub.Application.Interfaces.Services.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace EduHub.Infrastructure.Services.Caching;

/// <summary>
/// Ghi chú: RedisCacheService đọc/ghi cache Redis và fallback về database khi Redis lỗi.
/// </summary>
public sealed partial class RedisCacheService(
    IDistributedCache distributedCache,
    ILogger<RedisCacheService> logger)
    : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> locks = new();
    private long hits;
    private long misses;
    private long failures;

    /// <summary>
    /// Ghi chú: GetOrCreateAsync lấy dữ liệu từ Redis, miss thì khóa theo key và đọc database một lần.
    /// </summary>
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheEntryOptions options,
        CancellationToken cancellationToken)
    {
        var cached = await TryGetAsync<T>(key, cancellationToken);
        if (cached.Found)
        {
            Interlocked.Increment(ref hits);
            return cached.Value!;
        }

        Interlocked.Increment(ref misses);
        var keyLock = locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(cancellationToken);
        try
        {
            cached = await TryGetAsync<T>(key, cancellationToken);
            if (cached.Found)
            {
                Interlocked.Increment(ref hits);
                return cached.Value!;
            }

            var value = await factory(cancellationToken);
            await TrySetAsync(key, value, options, cancellationToken);
            return value;
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <summary>
    /// Ghi chú: RemoveAsync xóa một Redis key cụ thể nếu Redis đang hoạt động.
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            RegisterFailure(ex, key);
        }
    }

    /// <summary>
    /// Ghi chú: GetVersionAsync đọc version scope trong Redis, Redis lỗi thì trả version 1 để tiếp tục đọc database.
    /// </summary>
    public async Task<int> GetVersionAsync(string scope, CancellationToken cancellationToken)
    {
        var key = VersionKey(scope);
        var cached = await TryGetAsync<int>(key, cancellationToken);
        return cached.Found && cached.Value > 0 ? cached.Value : 1;
    }

    /// <summary>
    /// Ghi chú: BumpVersionAsync tăng version scope trong Redis để các cache key version cũ không còn được dùng.
    /// </summary>
    public async Task<int> BumpVersionAsync(string scope, CancellationToken cancellationToken)
    {
        var nextVersion = await GetVersionAsync(scope, cancellationToken) + 1;
        await TrySetAsync(
            VersionKey(scope),
            nextVersion,
            new CacheEntryOptions(TimeSpan.FromDays(30)),
            cancellationToken);

        return nextVersion;
    }

    /// <summary>
    /// Ghi chú: GetMetrics trả tổng hit/miss/failure cache từ lúc application chạy.
    /// </summary>
    public CacheMetricsSnapshot GetMetrics() =>
        new(Interlocked.Read(ref hits), Interlocked.Read(ref misses), Interlocked.Read(ref failures));

    private async Task<(bool Found, T? Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await distributedCache.GetStringAsync(key, cancellationToken);
            if (payload is null)
            {
                return (false, default);
            }

            return (true, JsonSerializer.Deserialize<T>(payload, JsonOptions));
        }
        catch (Exception ex)
        {
            RegisterFailure(ex, key);
            return (false, default);
        }
    }

    private async Task TrySetAsync<T>(string key, T value, CacheEntryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await distributedCache.SetStringAsync(
                key,
                JsonSerializer.Serialize(value, JsonOptions),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            RegisterFailure(ex, key);
        }
    }

    private void RegisterFailure(Exception exception, string key)
    {
        Interlocked.Increment(ref failures);
        LogCacheOperationFailed(logger, exception, key);
    }

    private static string VersionKey(string scope) => $"cache-version:{scope}";

    [LoggerMessage(
        EventId = 20,
        Level = LogLevel.Warning,
        Message = "Cache operation failed for key {CacheKey}.")]
    private static partial void LogCacheOperationFailed(ILogger logger, Exception exception, string cacheKey);
}
