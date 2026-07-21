using EduHub.Application.Contracts.Grades;

namespace EduHub.Application.Interfaces.Services.Grades;

/// <summary>
/// Ghi chú: IGpaCalculator là interface tính trung bình môn, GPA học kỳ và xếp loại học lực.
/// </summary>
public interface IGpaCalculator
{
    /// <summary>
    /// Ghi chú: CalculateSubjectAverage tính SubjectAverage từ các component score và weight.
    /// </summary>
    SubjectAverageResult CalculateSubjectAverage(IReadOnlyList<GradeComponentScoreInput> components);

    /// <summary>
    /// Ghi chú: CalculateSemesterGpa tính SemesterGPA từ trung bình môn và credits.
    /// </summary>
    SemesterGpaResult CalculateSemesterGpa(IReadOnlyList<SubjectGradeForGpaInput> subjects);

    /// <summary>
    /// Ghi chú: Classify xếp loại học lực theo GPA và policy version.
    /// </summary>
    ClassificationResult Classify(decimal gpa, ClassificationPolicy policy);
}

