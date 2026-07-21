namespace EduHub.Application.Features.Academics.Common;

/// <summary>
/// Ghi chú: AcademicNormalization chuẩn hóa dữ liệu cho chuẩn hóa mã/tên học vụ trước khi so sánh/lưu.
/// </summary>
public static class AcademicNormalization
{
    /// <summary>
    /// Ghi chú: Name thực hiện phần xử lý của chuẩn hóa mã/tên học vụ.
    /// </summary>
    public static string Name(string value) => value.Trim().ToUpperInvariant();

    /// <summary>
    /// Ghi chú: Code thực hiện phần xử lý của chuẩn hóa mã/tên học vụ.
    /// </summary>
    public static string Code(string value) => value.Trim().ToUpperInvariant();
}
