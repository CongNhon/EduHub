using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EduHub.Infrastructure.Audit;

/// <summary>
/// Ghi chú: MongoAuditIndexInitializer tạo index retention/correlation cho audit collection.
/// </summary>
public sealed class MongoAuditIndexInitializer(AuditLogOptions options) : IHostedService
{
    /// <summary>
    /// Ghi chú: StartAsync tạo TTL index timestampUtc và index correlationId trong Mongo audit.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Enabled || string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return;
        }

        try
        {
            var settings = MongoClientSettings.FromConnectionString(options.ConnectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(options.ServerSelectionTimeoutMilliseconds);
            var client = new MongoClient(settings);
            var collection = client
                .GetDatabase(options.DatabaseName)
                .GetCollection<BsonDocument>(options.CollectionName);

            await collection.Indexes.CreateManyAsync(
                [
                    new CreateIndexModel<BsonDocument>(
                        Builders<BsonDocument>.IndexKeys.Ascending("timestampUtc"),
                        new CreateIndexOptions
                        {
                            Name = "ix_audit_logs_timestamp_ttl",
                            ExpireAfter = TimeSpan.FromDays(options.RetentionDays)
                        }),
                    new CreateIndexModel<BsonDocument>(
                        Builders<BsonDocument>.IndexKeys.Ascending("correlationId"),
                        new CreateIndexOptions { Name = "ix_audit_logs_correlation_id" }),
                    new CreateIndexModel<BsonDocument>(
                        Builders<BsonDocument>.IndexKeys.Ascending("actorUserId").Ascending("timestampUtc"),
                        new CreateIndexOptions { Name = "ix_audit_logs_actor_timestamp" })
                ],
                cancellationToken);
        }
        catch
        {
        }
    }

    /// <summary>
    /// Ghi chú: StopAsync không cần xử lý vì Mongo audit initializer không giữ resource.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
