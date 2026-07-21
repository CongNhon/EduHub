using EduHub.Domain.Common;
using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities.Academics;

/// <summary>
/// Ghi chú: CurriculumSubjectQuota lưu số tiết chính xác của một môn trong năm và từng học kỳ.
/// </summary>
public sealed class CurriculumSubjectQuota : AuditableEntity
{
    private CurriculumSubjectQuota()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo quota môn học và bảo đảm tổng số tiết hai học kỳ bằng số tiết cả năm.
    /// </summary>
    public CurriculumSubjectQuota(
        Guid curriculumPlanId,
        Guid subjectId,
        CurriculumSubjectKind kind,
        int annualPeriods,
        int semester1Periods,
        int semester2Periods,
        bool canDoublePeriod,
        int maxPeriodsPerDay,
        bool includesHomeroom,
        TimetableSession? preferredSession = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(annualPeriods, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(semester1Periods, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(semester2Periods, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxPeriodsPerDay, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxPeriodsPerDay, 2);
        if (semester1Periods + semester2Periods != annualPeriods)
        {
            throw new ArgumentException("Semester period quotas must equal annual periods.", nameof(semester1Periods));
        }

        CurriculumPlanId = curriculumPlanId;
        SubjectId = subjectId;
        Kind = kind;
        AnnualPeriods = annualPeriods;
        Semester1Periods = semester1Periods;
        Semester2Periods = semester2Periods;
        CanDoublePeriod = canDoublePeriod;
        MaxPeriodsPerDay = maxPeriodsPerDay;
        IncludesHomeroom = includesHomeroom;
        PreferredSession = preferredSession;
    }

    public Guid CurriculumPlanId { get; private set; }
    public CurriculumPlan CurriculumPlan { get; private set; } = null!;
    public Guid SubjectId { get; private set; }
    public Subject Subject { get; private set; } = null!;
    public CurriculumSubjectKind Kind { get; private set; }
    public int AnnualPeriods { get; private set; }
    public int Semester1Periods { get; private set; }
    public int Semester2Periods { get; private set; }
    public bool CanDoublePeriod { get; private set; }
    public int MaxPeriodsPerDay { get; private set; }
    public bool IncludesHomeroom { get; private set; }
    public TimetableSession? PreferredSession { get; private set; }
}
