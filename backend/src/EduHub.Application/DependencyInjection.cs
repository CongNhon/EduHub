using EduHub.Application.Common.Behaviors;
using EduHub.Application.Interfaces.Services.Academics;
using EduHub.Application.Interfaces.Services.Authentication;
using EduHub.Application.Interfaces.Services.Caching;
using EduHub.Application.Interfaces.Services.Classes;
using EduHub.Application.Interfaces.Services.Grades;
using EduHub.Application.Interfaces.Services.Integrations;
using EduHub.Application.Interfaces.Services.StudentImports;
using EduHub.Application.Interfaces.Services.Notifications;
using EduHub.Application.Interfaces.Services.People;
using EduHub.Application.Interfaces.Services.Profiles;
using EduHub.Application.Interfaces.Services.Reports;
using EduHub.Application.Interfaces.Services.School;
using EduHub.Application.Interfaces.Services.Scheduling;
using EduHub.Application.Interfaces.Services.Students;
using EduHub.Application.Services.Academics;
using EduHub.Application.Services.Authentication;
using EduHub.Application.Services.Caching;
using EduHub.Application.Services.Classes;
using EduHub.Application.Services.Grades;
using EduHub.Application.Services.Integrations;
using EduHub.Application.Services.StudentImports;
using EduHub.Application.Services.Notifications;
using EduHub.Application.Services.People;
using EduHub.Application.Services.Profiles;
using EduHub.Application.Services.Reports;
using EduHub.Application.Services.School;
using EduHub.Application.Services.Scheduling;
using EduHub.Application.Services.Students;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EduHub.Application;

/// <summary>
/// Ghi chú: DependencyInjection đăng ký dependency injection cho project DependencyInjection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Ghi chú: AddApplication thực hiện phần xử lý của dependency injection.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(new PerformanceOptions());
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAcademicService, AcademicService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IGradeConfigurationService, GradeConfigurationService>();
        services.AddScoped<IGradeEntryService, GradeEntryService>();
        services.AddScoped<IExternalSyncService, ExternalSyncService>();
        services.AddScoped<IStudentImportService, StudentImportService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPeopleService, PeopleService>();
        services.AddScoped<IStudentProfileService, StudentProfileService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ISchoolProfileService, SchoolProfileService>();
        services.AddScoped<ISchedulingService, SchedulingService>();
        services.AddSingleton<ICacheKeyPolicy, CacheKeyPolicy>();
        services.AddSingleton<IGpaCalculator, GpaCalculator>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            configuration.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        return services;
    }
}
