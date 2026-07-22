using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Monitoring;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Repositories.Monitoring;
using EduHub.Application.Interfaces.Services.Caching;
using EduHub.Application.Interfaces.Services.Monitoring;
using EduHub.Domain.Enums;

namespace EduHub.Application.Services.Monitoring;

/// <summary>
/// Ghi chú: SystemMonitoringService kiểm tra quyền SystemAdmin và ghép Redis cache metrics với operational snapshot.
/// </summary>
public sealed class SystemMonitoringService(
    ISystemMonitoringRepository repository,
    ICacheService cacheService,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : ISystemMonitoringService
{
    /// <summary>
    /// Ghi chú: GetAsync trả số liệu vận hành hiện tại hoặc từ chối tài khoản không phải SystemAdmin.
    /// </summary>
    public async Task<Result<SystemMonitoringResponse>> GetAsync(GetSystemMonitoringQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.SystemAdmin)
        {
            return Result.Failure<SystemMonitoringResponse>(new Error(
                "Monitoring.SystemAdminRequired",
                "System administrator role is required.",
                ErrorType.Forbidden));
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var snapshot = await repository.GetSnapshotAsync(nowUtc, cancellationToken);
        var cache = cacheService.GetMetrics();
        var cacheRequests = cache.Hits + cache.Misses;
        decimal? hitRate = cacheRequests == 0
            ? null
            : decimal.Round(cache.Hits * 100m / cacheRequests, 2, MidpointRounding.AwayFromZero);

        return Result.Success(new SystemMonitoringResponse(
            nowUtc,
            new CacheMonitoringResponse(cache.Hits, cache.Misses, cache.Failures, hitRate),
            snapshot.Hangfire,
            snapshot.Outbox,
            snapshot.ExternalSyncs,
            snapshot.EmailDigests,
            snapshot.ReportJobs,
            snapshot.NotificationsLast24Hours));
    }
}
