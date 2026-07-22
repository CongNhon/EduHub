using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Repositories.Academics;
using EduHub.Application.Interfaces.Repositories.Analytics;
using EduHub.Application.Interfaces.Repositories.Authentication;
using EduHub.Application.Interfaces.Repositories.Classes;
using EduHub.Application.Interfaces.Repositories.Grades;
using EduHub.Application.Interfaces.Repositories.Integrations;
using EduHub.Application.Interfaces.Repositories.Monitoring;
using EduHub.Application.Interfaces.Repositories.StudentImports;
using EduHub.Application.Interfaces.Repositories.Notifications;
using EduHub.Application.Interfaces.Repositories.People;
using EduHub.Application.Interfaces.Repositories.Profiles;
using EduHub.Application.Interfaces.Repositories.Reports;
using EduHub.Application.Interfaces.Repositories.School;
using EduHub.Application.Interfaces.Repositories.Scheduling;
using EduHub.Application.Interfaces.Repositories.Students;
using EduHub.Application.Interfaces.Services.Caching;
using EduHub.Application.Interfaces.Services.Analytics;
using EduHub.Application.Interfaces.Services.Integrations;
using EduHub.Application.Interfaces.Services.StudentImports;
using EduHub.Application.Interfaces.Services.Reports;
using EduHub.Application.Interfaces.Services.Profiles;
using EduHub.Application.Interfaces.Services.Scheduling;
using EduHub.Infrastructure.Persistence;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Options;
using EduHub.Infrastructure.Repositories.Academics;
using EduHub.Infrastructure.Repositories.Analytics;
using EduHub.Infrastructure.Repositories.Authentication;
using EduHub.Infrastructure.Repositories.Classes;
using EduHub.Infrastructure.Repositories.Grades;
using EduHub.Infrastructure.Repositories.Integrations;
using EduHub.Infrastructure.Repositories.Monitoring;
using EduHub.Infrastructure.Repositories.StudentImports;
using EduHub.Infrastructure.Repositories.Notifications;
using EduHub.Infrastructure.Repositories.People;
using EduHub.Infrastructure.Repositories.Profiles;
using EduHub.Infrastructure.Repositories.Reports;
using EduHub.Infrastructure.Repositories.School;
using EduHub.Infrastructure.Repositories.Scheduling;
using EduHub.Infrastructure.Repositories.Students;
using EduHub.Infrastructure.Services.Authentication;
using EduHub.Infrastructure.Audit;
using EduHub.Infrastructure.Services.Caching;
using EduHub.Infrastructure.Services.Jobs;
using EduHub.Infrastructure.Services.StudentImports;
using EduHub.Infrastructure.Services.Messaging;
using EduHub.Infrastructure.Services.Profiles;
using EduHub.Infrastructure.Services.Reports;
using EduHub.Infrastructure.Services.Scheduling;
using EduHub.Infrastructure.Integrations.Ministry;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace EduHub.Infrastructure;

/// <summary>
/// Ghi chú: DependencyInjection đăng ký dependency injection cho project DependencyInjection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Ghi chú: AddInfrastructure thực hiện phần xử lý của dependency injection.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Postgres is required.");
        }

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddHangfire(hangfire => hangfire
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
        services.AddHangfireServer();

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
        }

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAcademicRepository, AcademicRepository>();
        services.AddScoped<IAdminAnalyticsRepository, AdminAnalyticsRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IGradeConfigurationRepository, GradeConfigurationRepository>();
        services.AddScoped<IGradeEntryRepository, GradeEntryRepository>();
        services.AddScoped<IExternalSyncRepository, ExternalSyncRepository>();
        services.AddScoped<ISystemMonitoringRepository, SystemMonitoringRepository>();
        services.AddScoped<IStudentImportRepository, StudentImportRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IPeopleRepository, PeopleRepository>();
        services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
        services.AddScoped<IReportJobRepository, ReportJobRepository>();
        services.AddSingleton<ISchoolProfileRepository, ConfigurationSchoolProfileRepository>();
        services.AddScoped<ISchedulingRepository, SchedulingRepository>();
        services.AddSingleton<ITimetableGenerator, OrToolsTimetableGenerator>();
        services.AddSingleton(TimeProvider.System);
        var profileEvidenceOptions = ProfileEvidenceStorageOptions.FromConfiguration(configuration);
        services.AddSingleton(profileEvidenceOptions);
        services.AddSingleton<IProfileEvidenceStorage, ProfileEvidenceStorage>();
        var auditLogOptions = AuditLogOptions.FromConfiguration(configuration);
        services.AddSingleton(auditLogOptions);
        services.AddHostedService<MongoAuditIndexInitializer>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddScoped<IAdminAnalyticsReportRenderer, DevExpressAdminAnalyticsReportRenderer>();
        services.AddScoped<IReportJobScheduler, HangfireReportJobScheduler>();
        services.AddScoped<IExternalSyncJobScheduler, HangfireExternalSyncJobScheduler>();
        services.AddScoped<IEducationMinistryGateway, EducationMinistryGateway>();
        services.AddSingleton<IStudentImportWorkbookReader, ClosedXmlStudentImportWorkbookReader>();
        services.AddScoped<IReportFileStorage, LocalReportFileStorage>();
        services.AddScoped<SimplePdfReportGenerator>();
        services.AddScoped<GradeMaintenanceJob>();
        services.AddScoped<DailyDigestJob>();
        services.AddScoped<WeeklyDigestJob>();
        services.AddScoped<WeeklyAdminAnalyticsDigestJob>();
        services.AddScoped<MinistrySyncJob>();
        var smtpEmailOptions = SmtpEmailOptions.FromConfiguration(configuration);
        services.AddSingleton(smtpEmailOptions);
        services.AddScoped<FakeEmailSender>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<IEmailSender>(provider =>
            smtpEmailOptions.Enabled
                ? provider.GetRequiredService<SmtpEmailSender>()
                : provider.GetRequiredService<FakeEmailSender>());
        services.AddHostedService<OutboxProcessor>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = configuration["Auth:Jwt:Issuer"] ?? string.Empty;
            options.Audience = configuration["Auth:Jwt:Audience"] ?? string.Empty;
            options.Secret = configuration["Auth:Jwt:Secret"] ?? string.Empty;
            options.AccessTokenMinutes = int.TryParse(configuration["Auth:Jwt:AccessTokenMinutes"], out var accessMinutes)
                ? accessMinutes
                : 15;
            options.RefreshTokenDays = int.TryParse(configuration["Auth:Jwt:RefreshTokenDays"], out var refreshDays)
                ? refreshDays
                : 7;
        });
        services.Configure<DevelopmentSeedOptions>(options =>
        {
            options.Enabled = bool.TryParse(configuration["Auth:DevelopmentSeed:Enabled"], out var enabled) && enabled;
            options.Email = configuration["Auth:DevelopmentSeed:Email"] ?? "admin@eduhub.local";
            options.Password = configuration["Auth:DevelopmentSeed:Password"] ?? string.Empty;
            options.Role = Enum.TryParse<UserRole>(configuration["Auth:DevelopmentSeed:Role"], out var role)
                ? role
                : UserRole.SystemAdmin;
        });
        services.Configure<SchoolProfileOptions>(configuration.GetSection("School"));

        var ministryBaseUrl = configuration["MinistryApi:BaseUrl"] ?? "https://localhost:9443";
        var ministryTimeoutSeconds = int.TryParse(configuration["MinistryApi:TimeoutSeconds"], out var configuredTimeoutSeconds)
            ? configuredTimeoutSeconds
            : 10;
        services.AddRefitClient<IEducationMinistryApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(ministryBaseUrl);
                client.Timeout = Timeout.InfiniteTimeSpan;
                var apiKey = configuration["MinistryApi:ApiKey"];
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
                }
            })
            .AddPolicyHandler(MinistryHttpPolicies.CreateCircuitBreakerPolicy(configuration))
            .AddPolicyHandler(MinistryHttpPolicies.CreateRetryPolicy())
            .AddPolicyHandler(MinistryHttpPolicies.CreateTimeoutPolicy(ministryTimeoutSeconds));

        return services;
    }
}
