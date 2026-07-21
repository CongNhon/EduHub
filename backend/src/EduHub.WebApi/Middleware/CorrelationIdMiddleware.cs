using EduHub.Infrastructure.Audit;
using Serilog.Context;

namespace EduHub.WebApi.Middleware;

/// <summary>
/// Ghi chú: CorrelationIdMiddleware chuẩn hóa correlation id và đẩy actor vào Serilog LogContext.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";

    /// <summary>
    /// Ghi chú: InvokeAsync tạo hoặc nhận correlation id an toàn cho request hiện tại.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetSafeCorrelationId(context);
        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RemoteIpHash", AuditRedactionPolicy.HashIp(context.Connection.RemoteIpAddress?.ToString())))
        {
            await next(context);
        }
    }

    private static string GetSafeCorrelationId(HttpContext context)
    {
        var raw = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(raw) || raw.Length > 64 || raw.Any(IsUnsafeCorrelationCharacter))
        {
            return Guid.NewGuid().ToString("N");
        }

        return raw;
    }

    private static bool IsUnsafeCorrelationCharacter(char value) =>
        !char.IsLetterOrDigit(value) && value is not '-' and not '_' and not '.';
}
