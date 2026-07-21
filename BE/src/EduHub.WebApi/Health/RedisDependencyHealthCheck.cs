using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EduHub.WebApi.Health;

/// <summary>
/// Ghi chú: RedisDependencyHealthCheck ghi/đọc cache key nhỏ để kiểm tra Redis cache EduHub.
/// </summary>
public sealed class RedisDependencyHealthCheck(IDistributedCache cache, IConfiguration configuration) : IHealthCheck
{
    /// <summary>
    /// Ghi chú: CheckHealthAsync kiểm tra Redis nếu có ConnectionStrings:Redis, thiếu cấu hình thì trả Degraded.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("Redis")))
        {
            return HealthCheckResult.Degraded("Redis connection string is missing; memory cache fallback may be active.");
        }

        try
        {
            var key = $"health:redis:{Guid.NewGuid():N}";
            await cache.SetStringAsync(
                key,
                "ok",
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) },
                cancellationToken);
            var value = await cache.GetStringAsync(key, cancellationToken);

            return value == "ok"
                ? HealthCheckResult.Healthy("Redis cache read/write is available.")
                : HealthCheckResult.Unhealthy("Redis cache read/write returned unexpected value.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis cache check failed.", ex);
        }
    }
}
