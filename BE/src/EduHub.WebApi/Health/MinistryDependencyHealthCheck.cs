using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EduHub.WebApi.Health;

/// <summary>
/// Ghi chú: MinistryDependencyHealthCheck kiểm tra Ministry API sandbox/real bằng endpoint /health nếu nhà cung cấp hỗ trợ.
/// </summary>
public sealed class MinistryDependencyHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IHealthCheck
{
    /// <summary>
    /// Ghi chú: CheckHealthAsync gọi MinistryApi:BaseUrl/health và trả Degraded khi sandbox chưa có hoặc không phản hồi.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["MinistryApi:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return HealthCheckResult.Degraded("MinistryApi:BaseUrl is missing.");
        }

        try
        {
            var client = httpClientFactory.CreateClient("MinistryHealth");
            using var response = await client.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Ministry API health endpoint is available.")
                : HealthCheckResult.Degraded($"Ministry API health endpoint returned {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("Ministry API health endpoint is unavailable.", ex);
        }
    }
}
