using EduHub.Domain.Common;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: Subject đại diện cho môn học trong hệ thống EduHub.
/// </summary>
public sealed class Subject : AuditableEntity
{
    private Subject()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor khởi tạo môn học và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public Subject(string subjectCode, string normalizedSubjectCode, string name, int credits, decimal maxScore = 10m)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedSubjectCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(credits, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxScore, 0m);

        SubjectCode = subjectCode;
        NormalizedSubjectCode = normalizedSubjectCode;
        Name = name;
        Credits = credits;
        MaxScore = maxScore;
    }

    public string SubjectCode { get; private set; } = null!;

    public string NormalizedSubjectCode { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public int Credits { get; private set; }

    public decimal MaxScore { get; private set; } = 10m;

    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Ghi chú: Update thực hiện phần xử lý của môn học.
    /// </summary>
    public void Update(string name, int credits, decimal maxScore, DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(credits, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxScore, 0m);

        Name = name.Trim();
        Credits = credits;
        MaxScore = maxScore;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: Disable thực hiện phần xử lý của môn học.
    /// </summary>
    public void Disable(DateTime updatedAtUtc)
    {
        IsActive = false;
        MarkUpdated(updatedAtUtc);
    }
}
