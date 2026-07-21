using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EduHub.Application.Common.Behaviors;

/// <summary>
/// Ghi chú: LoggingBehavior đại diện cho pipeline log tên request và thời gian xử lý trong hệ thống EduHub.
/// </summary>
public sealed partial class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Ghi chú: Handle xử lý pipeline log tên request và thời gian xử lý, gọi database/service cần thiết và trả kết quả.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = Activity.Current?.TraceId.ToString() ?? "background";
        var startTimestamp = Stopwatch.GetTimestamp();

        LogRequestStarted(logger, requestName, correlationId);

        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            var elapsedMilliseconds = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            LogRequestCompleted(logger, requestName, correlationId, elapsedMilliseconds);
        }
    }

    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Information,
        Message = "Handling MediatR request {RequestName} ({CorrelationId}).")]
    /// <summary>
    /// Ghi chú: LogRequestStarted ghi log khi bắt đầu xử lý MediatR request.
    /// </summary>
    private static partial void LogRequestStarted(ILogger logger, string requestName, string correlationId);

    [LoggerMessage(
        EventId = 101,
        Level = LogLevel.Information,
        Message = "Handled MediatR request {RequestName} ({CorrelationId}) in {ElapsedMilliseconds} ms.")]
    /// <summary>
    /// Ghi chú: LogRequestCompleted ghi log khi xử lý xong MediatR request.
    /// </summary>
    private static partial void LogRequestCompleted(
        ILogger logger,
        string requestName,
        string correlationId,
        double elapsedMilliseconds);
}
