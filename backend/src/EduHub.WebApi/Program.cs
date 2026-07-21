using Carter;
using EduHub.Application;
using EduHub.Infrastructure;
using EduHub.Infrastructure.Audit;
using EduHub.Application.Interfaces.Services.Notifications;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Infrastructure.Services.Jobs;
using EduHub.WebApi.Hubs;
using EduHub.WebApi.Health;
using EduHub.WebApi.Middleware;
using EduHub.WebApi.OpenApi;
using EduHub.WebApi.Realtime;
using EduHub.WebApi.Security;
using Hangfire;
using Hangfire.Dashboard;
using Serilog;
using Serilog.Formatting.Compact;
using System.Security.Claims;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, loggerConfiguration) =>
    loggerConfiguration.ConfigureEduHubSerilog(context.Configuration, context.HostingEnvironment));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCarter();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IRealtimeNotifier, SignalRRealtimeNotifier>();
builder.Services.AddEduHubSwagger();
builder.Services.AddEduHubSecurity(builder.Configuration);
builder.Services.AddHttpClient("MinistryHealth", client =>
{
    var baseUrl = builder.Configuration["MinistryApi:BaseUrl"] ?? "https://localhost:9443";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(3);
});
builder.Services.AddHealthChecks()
    .AddCheck<PostgresDependencyHealthCheck>("postgres")
    .AddCheck<RedisDependencyHealthCheck>("redis")
    .AddCheck<MongoDependencyHealthCheck>("mongo")
    .AddCheck<MinistryDependencyHealthCheck>("ministry");

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
        diagnosticContext.Set("ActorUserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous");
        diagnosticContext.Set("ActorRole", httpContext.User.FindFirstValue(ClaimTypes.Role) ?? "anonymous");
        diagnosticContext.Set("RemoteIpHash", AuditRedactionPolicy.HashIp(httpContext.Connection.RemoteIpAddress?.ToString()));
    };
});
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "EduHub API v1"));
}
else
{
    app.UseHsts();
}

app.UseAuthentication();
app.UseMiddleware<ActorLogContextMiddleware>();
app.UseAuthorization();

var dashboardEnabled = app.Environment.IsDevelopment() ||
    bool.TryParse(app.Configuration["Hangfire:DashboardEnabled"], out var configuredDashboardEnabled) &&
    configuredDashboardEnabled;
if (dashboardEnabled)
{
    app.UseHangfireDashboard(
        "/hangfire",
        new DashboardOptions
        {
            Authorization = [new HangfireDashboardAuthorizationFilter()]
        });
}

app.MapCarter();
app.MapHub<NotificationsHub>("/hubs/notifications");
app.MapHealthChecks("/health", EduHubHealthResponseWriter.ReadyOptions()).RequireAuthorization(AuthPolicies.SystemAdmin);
app.MapHealthChecks("/health/live", EduHubHealthResponseWriter.LiveOptions()).AllowAnonymous();
app.MapHealthChecks("/health/ready", EduHubHealthResponseWriter.ReadyOptions()).RequireAuthorization(AuthPolicies.SystemAdmin);

var recurringJobs = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobs.AddOrUpdate<GradeMaintenanceJob>(
    "lock-published-grades",
    job => job.LockPublishedGradesAsync(CancellationToken.None),
    Cron.Daily);
recurringJobs.AddOrUpdate<DailyDigestJob>(
    "daily-grade-digest",
    job => job.SendDailyDigestAsync(CancellationToken.None),
    "0 18 * * *",
    new RecurringJobOptions { TimeZone = VietnamTimeZone() });
recurringJobs.AddOrUpdate<WeeklyDigestJob>(
    "weekly-grade-digest",
    job => job.SendWeeklyDigestAsync(CancellationToken.None),
    "0 23 * * 0",
    new RecurringJobOptions { TimeZone = VietnamTimeZone() });

app.Run();
Log.CloseAndFlush();

// Ghi chú: VietnamTimeZone lấy timezone Việt Nam cho lịch Hangfire daily/weekly digest.
static TimeZoneInfo VietnamTimeZone()
{
    foreach (var id in new[] { "Asia/Ho_Chi_Minh", "SE Asia Standard Time" })
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
        }
        catch (InvalidTimeZoneException)
        {
        }
    }

    return TimeZoneInfo.Utc;
}

public partial class Program;
