namespace EduHub.WebApi.Middleware;

/// <summary>
/// Ghi chu: SecurityHeadersMiddleware them response headers bao ve API khoi MIME sniffing, framing va lo referrer.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Ghi chu: InvokeAsync gan security headers truoc khi API gui response ve client.
    /// </summary>
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Content-Security-Policy"] = "base-uri 'none'; object-src 'none'; frame-ancestors 'none'; form-action 'none'";
            headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=(), usb=()");

            if (context.Request.Path.StartsWithSegments("/api/v1/auth"))
            {
                headers["Cache-Control"] = "no-store";
                headers["Pragma"] = "no-cache";
            }

            return Task.CompletedTask;
        });

        return next(context);
    }
}
