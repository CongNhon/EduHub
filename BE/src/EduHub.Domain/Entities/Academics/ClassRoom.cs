using EduHub.Domain.Common;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: ClassRoom đại diện cho lớp học thuộc một năm học, có mã lớp, khối lớp và sức chứa.
/// </summary>
public sealed class ClassRoom : AuditableEntity
{
    private ClassRoom()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor khởi tạo lớp học và kiểm tra mã lớp, tên lớp, khối lớp, sức chứa.
    /// </summary>
    public ClassRoom(
        string classCode,
        string normalizedClassCode,
        string name,
        Guid academicYearId,
        int gradeLevel,
        int capacity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(classCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedClassCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(gradeLevel, 1);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(capacity, 0);

        ClassCode = classCode.Trim();
        NormalizedClassCode = normalizedClassCode;
        Name = name.Trim();
        AcademicYearId = academicYearId;
        GradeLevel = gradeLevel;
        Capacity = capacity;
    }

    public string ClassCode { get; private set; } = null!;

    public string NormalizedClassCode { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public Guid AcademicYearId { get; private set; }

    public AcademicYear AcademicYear { get; private set; } = null!;

    public int GradeLevel { get; private set; }

    public int Capacity { get; private set; }

    public int ActiveEnrollmentCount { get; private set; }

    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Ghi chú: Update cập nhật tên lớp, khối lớp và sức chứa của lớp học.
    /// </summary>
    public void Update(string name, int gradeLevel, int capacity, DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(gradeLevel, 1);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(capacity, 0);

        Name = name.Trim();
        GradeLevel = gradeLevel;
        Capacity = capacity;
        MarkUpdated(updatedAtUtc);
    }

    /// <summary>
    /// Ghi chú: SynchronizeActiveEnrollmentCount đồng bộ sĩ số lớp từ enrollment đang hoạt động sau seed hoặc tác vụ đối soát.
    /// </summary>
    public void SynchronizeActiveEnrollmentCount(int activeEnrollmentCount, DateTime updatedAtUtc)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(activeEnrollmentCount);
        if (activeEnrollmentCount > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(activeEnrollmentCount), "Active enrollment count cannot exceed class capacity.");
        }

        ActiveEnrollmentCount = activeEnrollmentCount;
        MarkUpdated(updatedAtUtc);
    }
}
