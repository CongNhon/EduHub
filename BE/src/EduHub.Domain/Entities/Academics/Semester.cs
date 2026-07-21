using EduHub.Domain.Common;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: Semester đại diện cho học kỳ trong hệ thống EduHub.
/// </summary>
public sealed class Semester : AuditableEntity
{
    private Semester()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor khởi tạo học kỳ và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public Semester(
        Guid academicYearId,
        string name,
        string normalizedName,
        DateOnly startDate,
        DateOnly endDate,
        DateOnly gradeEntryFrom,
        DateOnly gradeEntryTo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedName);
        if (startDate >= endDate)
        {
            throw new ArgumentException("Start date must be before end date.", nameof(startDate));
        }

        if (gradeEntryFrom > gradeEntryTo)
        {
            throw new ArgumentException("Grade entry window is invalid.", nameof(gradeEntryFrom));
        }

        AcademicYearId = academicYearId;
        Name = name;
        NormalizedName = normalizedName;
        StartDate = startDate;
        EndDate = endDate;
        GradeEntryFrom = gradeEntryFrom;
        GradeEntryTo = gradeEntryTo;
    }

    public Guid AcademicYearId { get; private set; }

    public AcademicYear AcademicYear { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = null!;

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public DateOnly GradeEntryFrom { get; private set; }

    public DateOnly GradeEntryTo { get; private set; }

    public SemesterStatus Status { get; private set; } = SemesterStatus.Planned;
}
