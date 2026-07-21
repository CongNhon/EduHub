using EduHub.Application.Common.Models;
using EduHub.WebApi.Dtos.Common;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.WebApi.Extensions;

/// <summary>
/// Ghi chú: ResultExtensions đại diện cho helper đổi Result thành HTTP response trong hệ thống EduHub.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Ghi chú: ToHttpResult thực hiện phần xử lý của helper đổi Result thành HTTP response.
    /// </summary>
    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess ? Results.NoContent() : ToProblem(result.Error!);

    /// <summary>
    /// Ghi chú: ToHttpResult chuyển Result application thành ApiResponse DTO thành công hoặc ProblemDetails lỗi.
    /// </summary>
    public static IResult ToHttpResult<TApplication, TApi>(
        this Result<TApplication> result,
        Func<TApplication, TApi> map) =>
        result.IsSuccess ? Results.Ok(ApiResponse.Ok(map(result.Value))) : ToProblem(result.Error!);

    /// <summary>
    /// Ghi chú: ToCreatedHttpResult chuyển Result application thành HTTP 201 kèm ApiResponse DTO.
    /// </summary>
    public static IResult ToCreatedHttpResult<TApplication, TApi>(
        this Result<TApplication> result,
        string location,
        Func<TApplication, TApi> map) =>
        result.IsSuccess ? Results.Created(location, ApiResponse.Ok(map(result.Value))) : ToProblem(result.Error!);

    /// <summary>
    /// Ghi chú: ToCreatedHttpResult chuyển Result application thành HTTP 201 với location tạo từ dữ liệu success.
    /// </summary>
    public static IResult ToCreatedHttpResult<TApplication, TApi>(
        this Result<TApplication> result,
        Func<TApplication, string> locationFactory,
        Func<TApplication, TApi> map) =>
        result.IsSuccess
            ? Results.Created(locationFactory(result.Value), ApiResponse.Ok(map(result.Value)))
            : ToProblem(result.Error!);

    /// <summary>
    /// Ghi chú: ToProblem thực hiện phần xử lý của helper đổi Result thành HTTP response.
    /// </summary>
    private static IResult ToProblem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.RateLimited => StatusCodes.Status429TooManyRequests,
            ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        ProblemDetails problemDetails = error.Details is null
            ? new ProblemDetails()
            : new ValidationProblemDetails(error.Details.ToDictionary(pair => pair.Key, pair => pair.Value));

        problemDetails.Status = statusCode;
        problemDetails.Title = error.Type.ToString();
        problemDetails.Detail = error.Type is ErrorType.Unexpected ? "An unexpected error occurred." : error.Message;
        problemDetails.Type = $"https://httpstatuses.com/{statusCode}";
        problemDetails.Extensions["errorCode"] = error.Code;

        return Results.Problem(
            detail: problemDetails.Detail,
            instance: problemDetails.Instance,
            statusCode: problemDetails.Status,
            title: problemDetails.Title,
            type: problemDetails.Type,
            extensions: problemDetails.Extensions);
    }
}
