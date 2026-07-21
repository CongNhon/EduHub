using EduHub.Domain.Common;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: GradeComponent đại diện cho một thành phần điểm của môn học trong một học kỳ, ví dụ miệng, 15 phút, giữa kỳ, cuối kỳ.
/// </summary>
public sealed class GradeComponent : AuditableEntity
{
    private GradeComponent()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo thành phần điểm thuộc subject-semester-version và kiểm tra weight/max score/order hợp lệ.
    /// </summary>
    public GradeComponent(
        Guid subjectId,
        Guid semesterId,
        string name,
        string normalizedName,
        decimal weight,
        decimal maxScore,
        int displayOrder,
        bool isRequired,
        bool includeInGpa,
        int version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedName);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(weight, 0m);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(weight, 1m);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxScore, 0m);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(displayOrder, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(version, 0);

        SubjectId = subjectId;
        SemesterId = semesterId;
        Name = name.Trim();
        NormalizedName = normalizedName.Trim();
        Weight = weight;
        MaxScore = maxScore;
        DisplayOrder = displayOrder;
        IsRequired = isRequired;
        IncludeInGpa = includeInGpa;
        Version = version;
    }

    public Guid SubjectId { get; private set; }

    public Subject Subject { get; private set; } = null!;

    public Guid SemesterId { get; private set; }

    public Semester Semester { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = null!;

    public decimal Weight { get; private set; }

    public decimal MaxScore { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsRequired { get; private set; }

    public bool IncludeInGpa { get; private set; }

    public int Version { get; private set; }

    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Ghi chú: Deactivate tắt version cấu hình thành phần điểm cũ khi tạo version mới.
    /// </summary>
    public void Deactivate(DateTime updatedAtUtc)
    {
        IsActive = false;
        MarkUpdated(UtcDateTime.Require(updatedAtUtc, nameof(updatedAtUtc)));
    }
}
