namespace EduHub.Application.Features.Classes.Common;

/// <summary>
/// Ghi chú: ClassNormalization chuẩn hóa mã lớp trước khi so sánh/lưu.
/// </summary>
public static class ClassNormalization
{
    /// <summary>
    /// Ghi chú: Code trim và uppercase mã lớp để kiểm tra trùng không phân biệt hoa thường/khoảng trắng.
    /// </summary>
    public static string Code(string value) => value.Trim().ToUpperInvariant();
}
