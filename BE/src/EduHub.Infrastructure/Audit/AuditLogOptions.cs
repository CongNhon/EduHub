using Microsoft.Extensions.Configuration;

namespace EduHub.Infrastructure.Audit;

/// <summary>
/// Ghi chú: AuditLogOptions chứa cấu hình MongoDB audit log cho Serilog sink.
/// </summary>
public sealed class AuditLogOptions
{
    public bool Enabled { get; init; } = true;

    public string ConnectionString { get; init; } = string.Empty;

    public string DatabaseName { get; init; } = "eduhub_audit";

    public string CollectionName { get; init; } = "audit_logs";

    public int RetentionDays { get; init; } = 90;

    public int ServerSelectionTimeoutMilliseconds { get; init; } = 1000;

    /// <summary>
    /// Ghi chú: FromConfiguration đọc cấu hình Audit:Mongo và ConnectionStrings:Mongo.
    /// </summary>
    public static AuditLogOptions FromConfiguration(IConfiguration configuration) =>
        new()
        {
            Enabled = !bool.TryParse(configuration["Audit:Mongo:Enabled"], out var enabled) || enabled,
            ConnectionString = configuration["Audit:Mongo:ConnectionString"] ??
                configuration.GetConnectionString("Mongo") ??
                string.Empty,
            DatabaseName = configuration["Audit:Mongo:DatabaseName"] ?? "eduhub_audit",
            CollectionName = configuration["Audit:Mongo:CollectionName"] ?? "audit_logs",
            RetentionDays = int.TryParse(configuration["Audit:Mongo:RetentionDays"], out var retentionDays)
                ? retentionDays
                : 90,
            ServerSelectionTimeoutMilliseconds = int.TryParse(
                configuration["Audit:Mongo:ServerSelectionTimeoutMilliseconds"],
                out var timeoutMilliseconds)
                ? timeoutMilliseconds
                : 1000
        };
}
