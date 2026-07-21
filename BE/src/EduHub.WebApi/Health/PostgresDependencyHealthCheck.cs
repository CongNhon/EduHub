using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EduHub.WebApi.Health;

/// <summary>
/// Ghi chú: PostgresDependencyHealthCheck ping PostgreSQL bằng EF Core để kiểm tra database chính EduHub.
/// </summary>
public sealed class PostgresDependencyHealthCheck(ApplicationDbContext dbContext) : IHealthCheck
{
    /// <summary>
    /// Ghi chú: CheckHealthAsync kiểm tra Postgres có kết nối được và trả trạng thái database chính.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Postgres connection is available.")
                : HealthCheckResult.Unhealthy("Postgres connection is unavailable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Postgres connection check failed.", ex);
        }
    }
}
