using DevExpress.AspNetCore;
using DevExpress.AspNetCore.Reporting;
using DevExpress.DashboardWeb;
using DevExpress.XtraReports.Services;
using EduHub.Infrastructure.Services.Reports;

namespace EduHub.WebApi.DevExpress;

/// <summary>
/// Ghi chú: DevExpressServiceCollectionExtensions đăng ký nền tảng Dashboard và Report Viewer cho cổng quản trị EduHub.
/// </summary>
public static class DevExpressServiceCollectionExtensions
{
    /// <summary>
    /// Ghi chú: AddEduHubDevExpress thêm MVC controller, Report Viewer cache và Dashboard ở chế độ lưu trữ chỉ đọc trong bộ nhớ.
    /// </summary>
    public static IServiceCollection AddEduHubDevExpress(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddDevExpressControls();
        services.ConfigureReportingServices(configurator =>
        {
            configurator.UseAsyncEngine();
            configurator.ConfigureWebDocumentViewer(viewer =>
                viewer.UseCachedReportSourceBuilder());
        });
        services.AddScoped<IReportProviderAsync, DevExpressAdminReportProvider>();
        services.AddScoped(_ =>
        {
            var configurator = new DashboardConfigurator();
            configurator.SetDashboardStorage(new DashboardInMemoryStorage());
            return configurator;
        });

        return services;
    }
}
