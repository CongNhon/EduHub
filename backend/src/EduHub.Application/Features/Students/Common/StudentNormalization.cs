namespace EduHub.Application.Features.Students.Common;

/// <summary>
/// Ghi chú: StudentNormalization chuẩn hóa dữ liệu cho chuẩn hóa mã học sinh trước khi so sánh/lưu.
/// </summary>
public static class StudentNormalization
{
    /// <summary>
    /// Ghi chú: Code thực hiện phần xử lý của chuẩn hóa mã học sinh.
    /// </summary>
    public static string Code(string value) => value.Trim().ToUpperInvariant();

    /// <summary>
    /// Ghi chú: SearchText chuẩn hóa họ tên học sinh để tìm kiếm không phân biệt hoa thường và dấu tiếng Việt.
    /// </summary>
    public static string SearchText(string value)
    {
        var decomposed = value.Trim().Normalize(System.Text.NormalizationForm.FormD);
        var builder = new System.Text.StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character is 'đ' or 'Đ' ? 'D' : char.ToUpperInvariant(character));
            }
        }

        return string.Join(' ', builder.ToString().Normalize(System.Text.NormalizationForm.FormC)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
