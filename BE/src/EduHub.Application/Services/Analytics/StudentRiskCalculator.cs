using EduHub.Application.Common.Options;
using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Services.Analytics;
using Microsoft.Extensions.Options;

namespace EduHub.Application.Services.Analytics;

/// <summary>
/// Ghi chú: StudentRiskCalculator thực hiện tính toán rủi ro học sinh dựa trên trọng số cấu hình.
/// </summary>
public sealed class StudentRiskCalculator : IStudentRiskCalculator
{
    private readonly AdvancedAnalyticsOptions _options;

    public StudentRiskCalculator(IOptions<AdvancedAnalyticsOptions> options)
    {
        _options = options.Value;
    }

    public StudentRiskItem Calculate(StudentRiskInput input)
    {
        var reasons = new List<StudentRiskReason>();
        var weights = _options.RiskWeights;

        // 1. Risk from GPA (35%)
        decimal riskGpa = 0;
        if (input.CurrentAverage.HasValue)
        {
            riskGpa = Math.Max(0, 100 - (input.CurrentAverage.Value * 10));
            if (input.CurrentAverage < 5.0m)
            {
                reasons.Add(new StudentRiskReason("ACAD_LOW", $"Điểm trung bình thấp ({input.CurrentAverage:F2})."));
            }
        }
        else
        {
            riskGpa = 100;
            reasons.Add(new StudentRiskReason("ACAD_MISSING", "Chưa có dữ liệu điểm học kỳ này."));
        }

        // 2. Risk from Trend (25%)
        decimal riskTrend = 0;
        decimal? growth = null;
        if (input.CurrentAverage.HasValue && input.PreviousAverage.HasValue)
        {
            growth = input.CurrentAverage.Value - input.PreviousAverage.Value;
            if (growth < 0)
            {
                riskTrend = Math.Min(100, -growth.Value * 20);
                if (growth < -0.5m)
                {
                    reasons.Add(new StudentRiskReason("TREND_DECLINE", $"Kết quả giảm sút ({growth:F2} điểm)."));
                }
            }
        }

        // 3. Risk from Missing Grades (20%)
        decimal riskMissing = Math.Min(100m, (input.MissingGradeRate ?? 0) * 2);
        if (input.MissingGradeRate > 10)
        {
            reasons.Add(new StudentRiskReason("DATA_MISSING", $"Thiếu {input.MissingGradeRate:F1}% số điểm."));
        }

        // 4. Risk from Failed Subjects (20%)
        decimal riskFailed = 0;
        if (input.TotalSubjectCount > 0)
        {
            riskFailed = Math.Min(100, ((decimal)input.FailedSubjectCount / input.TotalSubjectCount) * 100);
            if (input.FailedSubjectCount > 0)
            {
                reasons.Add(new StudentRiskReason("FAIL_SUBJECTS", $"Trượt {input.FailedSubjectCount}/{input.TotalSubjectCount} môn."));
            }
        }

        // Final weighted score
        decimal rawScore = (riskGpa * weights.LowPerformance) +
                          (riskTrend * weights.Decline) +
                          (riskMissing * weights.MissingGrades) +
                          (riskFailed * weights.FailedSubjects);

        decimal riskScore = Math.Ceiling(Math.Min(100, rawScore));

        string riskLevel = riskScore switch
        {
            >= 75 => "CRITICAL",
            >= 50 => "HIGH",
            >= 25 => "MEDIUM",
            _ => "LOW"
        };

        return new StudentRiskItem(
            input.StudentId,
            input.StudentCode,
            input.StudentName,
            input.ClassId,
            input.ClassCode,
            input.ClassName,
            input.GradeLevel,
            riskScore,
            riskLevel,
            input.CurrentAverage,
            input.PreviousAverage,
            growth,
            input.FailedSubjectCount,
            input.TotalSubjectCount,
            input.MissingGradeRate,
            input.CurrentPercentileInGrade,
            reasons);
    }
}
