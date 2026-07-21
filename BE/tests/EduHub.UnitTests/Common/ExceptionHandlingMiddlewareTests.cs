using System.Text;
using EduHub.Domain.Exceptions;
using EduHub.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace EduHub.UnitTests.Common;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_MapsDomainExceptionToSafeConflictProblemDetails()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new DomainException("student.code_conflict", "Student code already exists."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var body = await ReadBodyAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.Contains("student.code_conflict", body, StringComparison.Ordinal);
        Assert.DoesNotContain("Student code already exists.", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotExposeUnexpectedExceptionDetails()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("ConnectionStrings__Postgres=secret"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var body = await ReadBodyAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains("internal_error", body, StringComparison.Ordinal);
        Assert.DoesNotContain("ConnectionStrings__Postgres=secret", body, StringComparison.Ordinal);
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-p02"
        };

        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadBodyAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}
