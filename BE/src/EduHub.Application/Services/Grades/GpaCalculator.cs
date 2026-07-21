using EduHub.Application.Contracts.Grades;
using EduHub.Application.Interfaces.Services.Grades;

namespace EduHub.Application.Services.Grades;

/// <summary>
/// Ghi chú: GpaCalculator tính trung bình môn, GPA học kỳ và xếp loại học lực theo policy version.
/// </summary>
public sealed class GpaCalculator : IGpaCalculator
{
    private const int DecimalPlaces = 2;

    /// <summary>
    /// Ghi chú: CalculateSubjectAverage tính trung bình môn từ các thành phần điểm có weight tổng đúng 1.00.
    /// </summary>
    public SubjectAverageResult CalculateSubjectAverage(IReadOnlyList<GradeComponentScoreInput> components)
    {
        var included = components.Where(component => component.IncludeInGpa).ToList();
        if (included.Count == 0)
        {
            return new(false, null, "GPA.NoIncludedComponents");
        }

        var totalWeight = included.Sum(component => component.Weight);
        if (totalWeight != 1.00m)
        {
            return new(false, null, "GPA.InvalidWeightTotal");
        }

        if (included.Any(component => component.IsRequired && component.Score is null))
        {
            return new(false, null, "GPA.MissingRequiredComponent");
        }

        var available = included.Where(component => component.Score.HasValue).ToList();
        if (available.Count != included.Count)
        {
            return new(false, null, "GPA.MissingComponent");
        }

        var average = available.Sum(component => component.Score!.Value * component.Weight);
        return new(true, Round(average), null);
    }

    /// <summary>
    /// Ghi chú: CalculateSemesterGpa tính GPA học kỳ từ trung bình môn và credits, bỏ môn exclude khỏi GPA.
    /// </summary>
    public SemesterGpaResult CalculateSemesterGpa(IReadOnlyList<SubjectGradeForGpaInput> subjects)
    {
        var included = subjects.Where(subject => subject.IncludeInGpa).ToList();
        if (included.Any(subject => subject.SubjectAverage is null))
        {
            return new(false, null, "GPA.MissingSubjectAverage");
        }

        var totalCredits = included.Sum(subject => subject.Credits);
        if (totalCredits <= 0)
        {
            return new(false, null, "GPA.NoCredits");
        }

        var gpa = included.Sum(subject => subject.SubjectAverage!.Value * subject.Credits) / totalCredits;
        return new(true, Round(gpa), null);
    }

    /// <summary>
    /// Ghi chú: Classify xếp loại học lực theo ngưỡng GPA của policy version được truyền vào.
    /// </summary>
    public ClassificationResult Classify(decimal gpa, ClassificationPolicy policy)
    {
        var threshold = policy.Thresholds
            .OrderByDescending(item => item.MinimumGpa)
            .First(item => gpa >= item.MinimumGpa);

        return new(threshold.Name, policy.Version, policy.EffectiveFrom);
    }

    private static decimal Round(decimal value) =>
        Math.Round(value, DecimalPlaces, MidpointRounding.AwayFromZero);
}
