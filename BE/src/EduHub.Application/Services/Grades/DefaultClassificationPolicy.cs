using EduHub.Application.Contracts.Grades;

namespace EduHub.Application.Services.Grades;

/// <summary>
/// Ghi chú: DefaultClassificationPolicy cung cấp policy xếp loại mặc định có version/effective date.
/// </summary>
public static class DefaultClassificationPolicy
{
    /// <summary>
    /// Ghi chú: Current là policy mặc định Excellent/Good/Average/Weak dùng khi trường chưa cấu hình policy riêng.
    /// </summary>
    public static ClassificationPolicy Current { get; } = new(
        "default-2026-07-13",
        new DateOnly(2026, 7, 13),
        new[]
        {
            new ClassificationThreshold("Excellent", 8.50m),
            new ClassificationThreshold("Good", 7.00m),
            new ClassificationThreshold("Average", 5.00m),
            new ClassificationThreshold("Weak", 0.00m)
        });
}
