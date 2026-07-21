using EduHub.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduHub.WebApi.Middleware;

/// <summary>
/// Ghi chú: ExceptionHandlingMiddleware đại diện cho middleware đổi exception thành ProblemDetails trong hệ thống EduHub.
/// </summary>
public sealed partial class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <summary>
    /// Ghi chú: InvokeAsync bắt exception của request và trả ProblemDetails nếu có lỗi.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            var mappedException = MapException(exception);

            LogUnhandledRequestFailure(
                logger,
                context.TraceIdentifier,
                exception.GetType().Name,
                mappedException.StatusCode);

            await WriteProblemDetailsAsync(context, mappedException);
        }
    }

    /// <summary>
    /// Ghi chú: MapException đổi exception thành status code/error message an toàn.
    /// </summary>
    private static MappedException MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => new MappedException(
                StatusCodes.Status400BadRequest,
                "Bad request",
                "validation.failed",
                "One or more validation errors occurred.",
                validationException.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).Distinct().ToArray())),
            DomainException domainException => new MappedException(
                StatusCodes.Status409Conflict,
                "Conflict",
                domainException.Code,
                "The request conflicts with the current state.",
                null),
            DbUpdateConcurrencyException => new MappedException(
                StatusCodes.Status409Conflict,
                "Conflict",
                "database.concurrency_conflict",
                "The record was changed by another request. Reload the latest version and retry.",
                null),
            DbUpdateException => new MappedException(
                StatusCodes.Status409Conflict,
                "Conflict",
                "database.constraint_conflict",
                "The request conflicts with a database constraint.",
                null),
            BadHttpRequestException => new MappedException(
                StatusCodes.Status400BadRequest,
                "Bad request",
                "request.invalid",
                "The request is invalid.",
                null),
            _ => new MappedException(
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "internal_error",
                "An unexpected error occurred.",
                null)
        };
    }

    /// <summary>
    /// Ghi chú: WriteProblemDetailsAsync ghi ProblemDetails ra HTTP response.
    /// </summary>
    private static async Task WriteProblemDetailsAsync(HttpContext context, MappedException mappedException)
    {
        ProblemDetails problemDetails = mappedException.Errors is null
            ? new ProblemDetails()
            : new ValidationProblemDetails(mappedException.Errors.ToDictionary(pair => pair.Key, pair => pair.Value));

        problemDetails.Status = mappedException.StatusCode;
        problemDetails.Title = mappedException.Title;
        problemDetails.Detail = mappedException.Detail;
        problemDetails.Type = $"https://httpstatuses.com/{mappedException.StatusCode}";

        problemDetails.Extensions["errorCode"] = mappedException.ErrorCode;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = mappedException.StatusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: CancellationToken.None);
    }

    /// <summary>
    /// Ghi chú: MappedException đóng gói exception đã map sang HTTP status và ProblemDetails.
    /// </summary>
    private sealed record MappedException(
        int StatusCode,
        string Title,
        string ErrorCode,
        string Detail,
        IReadOnlyDictionary<string, string[]>? Errors);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Unhandled request failure. TraceId: {TraceId}; ExceptionType: {ExceptionType}; StatusCode: {StatusCode}")]
    /// <summary>
    /// Ghi chú: LogUnhandledRequestFailure ghi log lỗi request chưa được xử lý.
    /// </summary>
    private static partial void LogUnhandledRequestFailure(ILogger logger, string traceId, string exceptionType, int statusCode);
}
