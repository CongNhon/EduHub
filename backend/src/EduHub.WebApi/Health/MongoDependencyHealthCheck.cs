using EduHub.Infrastructure.Audit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EduHub.WebApi.Health;

/// <summary>
/// Ghi chú: MongoDependencyHealthCheck ping MongoDB audit database để kiểm tra log/audit sink EduHub.
/// </summary>
public sealed class MongoDependencyHealthCheck(AuditLogOptions options) : IHealthCheck
{
    /// <summary>
    /// Ghi chú: CheckHealthAsync kiểm tra Mongo audit, tắt audit thì trả Degraded thay vì lỗi process API.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!options.Enabled)
        {
            return HealthCheckResult.Degraded("Mongo audit is disabled.");
        }

        try
        {
            var settings = MongoClientSettings.FromConnectionString(options.ConnectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(options.ServerSelectionTimeoutMilliseconds);
            var client = new MongoClient(settings);
            var database = client.GetDatabase(options.DatabaseName);
            await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("Mongo audit connection is available.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("Mongo audit connection check failed.", ex);
        }
    }
}
