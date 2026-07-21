using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EduHub.Application.Common.Behaviors;

/// <summary>
/// Ghi chú: PerformanceBehavior đại diện cho pipeline cảnh báo request chạy chậm trong hệ thống EduHub.
/// </summary>
public sealed partial class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
    PerformanceOptions options)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Ghi chú: Handle xử lý pipeline cảnh báo request chạy chậm, gọi database/service cần thiết và trả kết quả.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var startTimestamp = Stopwatch.GetTimestamp();
        var response = await next(cancellationToken);
        var elapsedMilliseconds = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

        if (elapsedMilliseconds >= options.WarningThresholdMilliseconds)
        {
            LogSlowRequest(logger, typeof(TRequest).Name, elapsedMilliseconds, options.WarningThresholdMilliseconds);
        }

        return response;
    }

    [LoggerMessage(
        EventId = 102,
        Level = LogLevel.Warning,
        Message = "Slow MediatR request {RequestName}: {ElapsedMilliseconds} ms exceeded {ThresholdMilliseconds} ms.")]
    /// <summary>
    /// Ghi chú: LogSlowRequest ghi warning khi MediatR request vượt ngưỡng thời gian.
    /// </summary>
    private static partial void LogSlowRequest(
        ILogger logger,
        string requestName,
        double elapsedMilliseconds,
        int thresholdMilliseconds);
}
