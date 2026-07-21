namespace EduHub.WebApi.Dtos.Common;

/// <summary>
/// Ghi chú: ApiResponse là DTO response chuẩn bọc dữ liệu trả về thành công của API.
/// </summary>
public sealed record ApiResponse<T>(bool Success, T Data);

/// <summary>
/// Ghi chú: ApiResponse tạo DTO response chuẩn cho API.
/// </summary>
public static class ApiResponse
{
    /// <summary>
    /// Ghi chú: Ok tạo ApiResponse thành công chứa DTO dữ liệu trả về cho client.
    /// </summary>
    public static ApiResponse<T> Ok<T>(T data) => new(true, data);
}
