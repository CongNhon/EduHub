using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

namespace EduHub.Infrastructure.Audit;

/// <summary>
/// Ghi chú: SerilogAuditConfiguration cấu hình Serilog JSON console và Mongo audit sink.
/// </summary>
public static class SerilogAuditConfiguration
{
    /// <summary>
    /// Ghi chú: ConfigureEduHubSerilog thêm enricher, JSON console và Mongo audit sink cho EduHub.
    /// </summary>
    public static LoggerConfiguration ConfigureEduHubSerilog(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var auditOptions = AuditLogOptions.FromConfiguration(configuration);

        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "EduHub")
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            .WriteTo.Console(new CompactJsonFormatter())
            .WriteTo.Sink(new MongoAuditSink(auditOptions));

        return loggerConfiguration;
    }
}
