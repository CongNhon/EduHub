using EduHub.Domain.Common;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: AcademicYear đại diện cho năm học trong hệ thống EduHub.
/// </summary>
public sealed class AcademicYear : AuditableEntity
{
    private AcademicYear()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor khởi tạo năm học và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public AcademicYear(string name, string normalizedName, DateOnly startDate, DateOnly endDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedName);
        if (startDate >= endDate)
        {
            throw new ArgumentException("Start date must be before end date.", nameof(startDate));
        }

        Name = name;
        NormalizedName = normalizedName;
        StartDate = startDate;
        EndDate = endDate;
    }

    public string Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = null!;

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public AcademicYearStatus Status { get; private set; } = AcademicYearStatus.Planned;

    public ICollection<Semester> Semesters { get; } = new List<Semester>();
}
