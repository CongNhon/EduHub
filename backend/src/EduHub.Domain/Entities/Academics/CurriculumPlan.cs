using EduHub.Domain.Common;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: CurriculumPlan đại diện cho chương trình học của một khối trong một năm học, gồm số tuần và quota môn học.
/// </summary>
public sealed class CurriculumPlan : AuditableEntity
{
    private CurriculumPlan()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo chương trình học cho khối 10, 11 hoặc 12 và kiểm tra tổng số tuần của hai học kỳ.
    /// </summary>
    public CurriculumPlan(
        Guid academicYearId,
        int gradeLevel,
        string name,
        int totalWeeks,
        int semester1Weeks,
        int semester2Weeks)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(gradeLevel, 10);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(gradeLevel, 12);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(totalWeeks, 0);
        if (totalWeeks != 35 || semester1Weeks != 18 || semester2Weeks != 17)
        {
            throw new ArgumentException("A high-school curriculum must contain 35 weeks split into 18 and 17 weeks.", nameof(totalWeeks));
        }

        if (semester1Weeks + semester2Weeks != totalWeeks)
        {
            throw new ArgumentException("Semester weeks must equal total weeks.", nameof(semester1Weeks));
        }

        AcademicYearId = academicYearId;
        GradeLevel = gradeLevel;
        Name = name.Trim();
        TotalWeeks = totalWeeks;
        Semester1Weeks = semester1Weeks;
        Semester2Weeks = semester2Weeks;
    }

    public Guid AcademicYearId { get; private set; }
    public AcademicYear AcademicYear { get; private set; } = null!;
    public int GradeLevel { get; private set; }
    public string Name { get; private set; } = null!;
    public int TotalWeeks { get; private set; }
    public int Semester1Weeks { get; private set; }
    public int Semester2Weeks { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<CurriculumSubjectQuota> SubjectQuotas { get; } = new List<CurriculumSubjectQuota>();
}
