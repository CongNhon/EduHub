namespace EduHub.Application.Common.Models;

/// <summary>
/// Ghi chú: Result đại diện cho kết quả success/failure của use case trong hệ thống EduHub.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess == (error is not null))
        {
            throw new ArgumentException("A successful result cannot have an error, and a failed result must have one.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    /// <summary>
    /// Ghi chú: Success tạo Result thành công cho use case.
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Ghi chú: Failure tạo Result thất bại kèm Error code cho use case.
    /// </summary>
    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result(false, error);
    }

    public static Result<T> Success<T>(T value) => new(value);

    public static Result<T> Failure<T>(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>(error);
    }
}

/// <summary>
/// Ghi chú: Result đại diện cho kết quả success/failure của use case trong hệ thống EduHub.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T value)
        : base(true, null)
    {
        _value = value;
    }

    internal Result(Error error)
        : base(false, error)
    {
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("A failed result does not contain a value.");

}
