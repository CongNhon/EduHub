using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EduHub.IntegrationTests.Support;

/// <summary>
/// Ghi chú: EduHubWebApplicationFactory tạo test host API EduHub với connection string container thật.
/// </summary>
public sealed class EduHubWebApplicationFactory(IReadOnlyDictionary<string, string?> configuration)
    : WebApplicationFactory<Program>
{
    /// <summary>
    /// Ghi chú: ConfigureWebHost nạp cấu hình test cho Postgres, Redis, Mongo, Ministry và tắt SMTP thật.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        foreach (var setting in configuration)
        {
            if (setting.Value is not null)
            {
                builder.UseSetting(setting.Key, setting.Value);
            }
        }
    }
}
