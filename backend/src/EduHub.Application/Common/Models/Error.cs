namespace EduHub.Application.Common.Models;

/// <summary>
/// Ghi chú: ErrorType liệt kê các giá trị hợp lệ cho nhóm lỗi để map sang HTTP status.
/// </summary>
public enum ErrorType
{
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    RateLimited,
    Unavailable,
    Unexpected
}

/// <summary>
/// Ghi chú: Error đại diện cho mã lỗi và loại lỗi trả về API trong hệ thống EduHub.
/// </summary>
public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyDictionary<string, string[]>? Details = null);
