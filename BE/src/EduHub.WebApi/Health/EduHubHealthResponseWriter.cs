using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EduHub.WebApi.Health;

/// <summary>
/// Ghi chú: EduHubHealthResponseWriter ghi JSON health response gồm trạng thái từng dependency của API.
/// </summary>
public static class EduHubHealthResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Ghi chú: WriteAsync chuyển HealthReport thành JSON để Swagger/Postman đọc trạng thái Postgres, Redis, Mongo và Ministry.
    /// </summary>
    public static async Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    durationMs = entry.Value.Duration.TotalMilliseconds,
                    error = entry.Value.Exception?.Message
                })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    /// <summary>
    /// Ghi chu: WritePublicAsync chi tra trang thai process API, khong lo ten hoac loi dependency noi bo.
    /// </summary>
    public static Task WritePublicAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { status = report.Status.ToString() }, JsonOptions));
    }

    /// <summary>
    /// Ghi chú: ReadyOptions cấu hình /health/ready trả JSON và giữ Degraded là HTTP 200 để app vẫn xem được tình trạng phụ thuộc.
    /// </summary>
    public static HealthCheckOptions ReadyOptions() => new()
    {
        ResponseWriter = WriteAsync,
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
    };

    /// <summary>
    /// Ghi chú: LiveOptions cấu hình /health/live chỉ kiểm tra process API còn sống, không ping dependency ngoài.
    /// </summary>
    public static HealthCheckOptions LiveOptions() => new()
    {
        Predicate = _ => false,
        ResponseWriter = WritePublicAsync
    };
}
