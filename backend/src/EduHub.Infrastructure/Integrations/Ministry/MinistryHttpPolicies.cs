using System.Net;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace EduHub.Infrastructure.Integrations.Ministry;

/// <summary>
/// Ghi chú: MinistryHttpPolicies tạo retry và circuit breaker policy cho Ministry API.
/// </summary>
public static class MinistryHttpPolicies
{
    /// <summary>
    /// Ghi chú: CreateRetryPolicy retry Ministry API khi timeout/408/429/5xx theo backoff và Retry-After.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response =>
                response.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                3,
                (retryAttempt, outcome, _) =>
                    outcome.Result?.Headers.RetryAfter?.Delta ??
                    TimeSpan.FromSeconds(Math.Pow(2d, retryAttempt)) + TimeSpan.FromMilliseconds(retryAttempt * 100),
                (_, _, _, _) => Task.CompletedTask);

    /// <summary>
    /// Ghi chú: CreateTimeoutPolicy giới hạn thời gian từng attempt gọi Ministry API.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> CreateTimeoutPolicy(int timeoutSeconds) =>
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(timeoutSeconds), TimeoutStrategy.Optimistic);

    /// <summary>
    /// Ghi chú: CreateCircuitBreakerPolicy mở circuit khi Ministry API lỗi liên tục theo cấu hình.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(IConfiguration configuration)
    {
        var failureCount = int.TryParse(configuration["MinistryApi:CircuitBreakerFailures"], out var configuredFailures)
            ? configuredFailures
            : 5;
        var breakSeconds = int.TryParse(configuration["MinistryApi:CircuitBreakerSeconds"], out var configuredSeconds)
            ? configuredSeconds
            : 60;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(failureCount, TimeSpan.FromSeconds(breakSeconds));
    }
}
