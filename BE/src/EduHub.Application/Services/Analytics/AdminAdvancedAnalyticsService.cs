using System.Globalization;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Repositories.Analytics;
using EduHub.Application.Interfaces.Services.Analytics;

namespace EduHub.Application.Services.Analytics;

/// <summary>
/// Ghi chú: AdminAdvancedAnalyticsService điều phối việc lấy dữ liệu và tính toán các chỉ số phân tích.
/// </summary>
public sealed class AdminAdvancedAnalyticsService : IAdminAdvancedAnalyticsService
{
    private readonly IAdminAdvancedAnalyticsRepository _repository;
    private readonly IAcademicStatisticsCalculator _statsCalculator;
    private readonly IComparativeAnalyticsCalculator _compCalculator;
    private readonly IDataQualityScoreCalculator _dqCalculator;
    private readonly IStudentRiskCalculator _riskCalculator;

    public AdminAdvancedAnalyticsService(
        IAdminAdvancedAnalyticsRepository repository,
        IAcademicStatisticsCalculator statsCalculator,
        IComparativeAnalyticsCalculator compCalculator,
        IDataQualityScoreCalculator dqCalculator,
        IStudentRiskCalculator riskCalculator)
    {
        _repository = repository;
        _statsCalculator = statsCalculator;
        _compCalculator = compCalculator;
        _dqCalculator = dqCalculator;
        _riskCalculator = riskCalculator;
    }

    public async Task<Result<AdminAdvancedSummaryResponse>> ReadSummaryAsync(
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken)
    {
        var currentId = filter.SemesterId ?? await _repository.GetLatestSemesterIdAsync(cancellationToken);
        if (currentId == null) return Result.Failure<AdminAdvancedSummaryResponse>(new Error("SEMESTER_NOT_FOUND", "Không tìm thấy học kỳ hiện tại.", ErrorType.NotFound));

        var previousId = filter.PreviousSemesterId ?? await _repository.GetPreviousSemesterIdAsync(currentId.Value, cancellationToken);

        var semesterIds = previousId.HasValue ? new[] { currentId.Value, previousId.Value } : new[] { currentId.Value };
        var scores = await _repository.ReadAcademicScoresAsync(semesterIds, filter, cancellationToken);
        
        var currentScores = scores.Where(s => s.SemesterId == currentId).Select(s => s.Score).ToList();
        var prevScores = previousId.HasValue ? scores.Where(s => s.SemesterId == previousId).Select(s => s.Score).ToList() : new List<decimal>();

        var metadata = new AdvancedMetricMetadata("2.0.0", "2.0.0", "2.0.0", DateTime.UtcNow);

        // Calculate Average Score Metric
        decimal? currentAvg = currentScores.Count > 0 ? Math.Round(currentScores.Average(), 2) : null;
        decimal? prevAvg = prevScores.Count > 0 ? Math.Round(prevScores.Average(), 2) : null;
        var avgComp = _compCalculator.Compare(currentAvg, prevAvg);
        var avgMetric = new CommonDecimalMetric(currentAvg, prevAvg, avgComp.AbsoluteChange, avgComp.PercentageChange, avgComp.Trend);

        // Calculate Pass Rate Metric
        var currentPass = _statsCalculator.CalculateThresholdRate(currentScores, x => x >= 5.0m);
        var prevPass = _statsCalculator.CalculateThresholdRate(prevScores, x => x >= 5.0m);
        var passComp = _compCalculator.Compare(currentPass.Percentage, prevPass.Percentage);
        var passMetric = new CommonDecimalMetric(currentPass.Percentage, prevPass.Percentage, passComp.AbsoluteChange, passComp.PercentageChange, passComp.Trend);

        // Calculate Excellent Rate Metric
        var currentExc = _statsCalculator.CalculateThresholdRate(currentScores, x => x >= 8.0m);
        var prevExc = _statsCalculator.CalculateThresholdRate(prevScores, x => x >= 8.0m);
        var excComp = _compCalculator.Compare(currentExc.Percentage, prevExc.Percentage);
        var excMetric = new CommonDecimalMetric(currentExc.Percentage, prevExc.Percentage, excComp.AbsoluteChange, excComp.PercentageChange, excComp.Trend);

        // Calculate Missing Grade Rate Metric
        var expectedGrades = await _repository.ReadExpectedGradesAsync(currentId.Value, filter, cancellationToken);
        var totalExpected = expectedGrades.Sum(x => x.ExpectedCount);
        decimal? currentMissingRate = totalExpected > 0 
            ? Math.Round((decimal)expectedGrades.Sum(x => x.ExpectedCount - x.RecordedCount) / totalExpected * 100, 1)
            : null;
        var missingMetric = new CommonDecimalMetric(currentMissingRate, null, null, null, "STABLE");

        // Calculate Growth Summary
        var studentGrowth = scores
            .GroupBy(s => s.StudentId)
            .Select(g => {
                var cur = g.Where(x => x.SemesterId == currentId).Select(x => x.Score).Cast<decimal?>().FirstOrDefault();
                var prev = g.Where(x => x.SemesterId == previousId).Select(x => x.Score).Cast<decimal?>().FirstOrDefault();
                return (cur, prev);
            })
            .Where(x => x.cur.HasValue && x.prev.HasValue)
            .Select(x => x.cur!.Value - x.prev!.Value)
            .ToList();
        var growthSummary = _compCalculator.CalculateGrowth(studentGrowth);

        // Data Quality
        var dqSnapshot = await _repository.ReadDataQualitySnapshotAsync(currentId.Value, filter, cancellationToken);
        var dqSummary = _dqCalculator.Calculate(dqSnapshot);

        return Result<AdminAdvancedSummaryResponse>.Success(new AdminAdvancedSummaryResponse(
            metadata, avgMetric, passMetric, excMetric, missingMetric, growthSummary, dqSummary));
    }

    public async Task<Result<AcademicDistributionResponse>> ReadDistributionAsync(
        AdminAdvancedAnalyticsFilter filter, 
        string groupBy, 
        CancellationToken cancellationToken)
    {
        var semesterId = filter.SemesterId ?? await _repository.GetLatestSemesterIdAsync(cancellationToken);
        if (semesterId == null) return Result.Failure<AcademicDistributionResponse>(new Error("SEMESTER_NOT_FOUND", "Không tìm thấy học kỳ.", ErrorType.NotFound));

        var scores = await _repository.ReadAcademicScoresAsync(new[] { semesterId.Value }, filter, cancellationToken);
        var currentScores = scores.Select(s => s.Score).ToList();

        var metadata = new AdvancedMetricMetadata("2.0.0", "2.0.0", "2.0.0", DateTime.UtcNow);
        var overallDist = _statsCalculator.CalculateDistribution(currentScores);
        var buckets = _statsCalculator.CalculateBuckets(currentScores);

        var groupedItems = scores
            .GroupBy(s => groupBy.ToLower(CultureInfo.InvariantCulture) switch {
                "grade" => s.GradeLevel?.ToString(CultureInfo.InvariantCulture) ?? "Unknown",
                _ => s.ClassId?.ToString() ?? "Unknown"
            })
            .Select(g => new GroupedDistributionItem(
                g.Key,
                g.FirstOrDefault()?.ClassName ?? g.Key,
                _statsCalculator.CalculateDistribution(g.Select(x => x.Score).ToList())
            ))
            .ToList();
        
        return Result<AcademicDistributionResponse>.Success(new AcademicDistributionResponse(
            metadata, overallDist, buckets, groupedItems));
    }

    public async Task<Result<AcademicTrendResponse>> ReadTrendsAsync(
        AdminAdvancedAnalyticsFilter filter, 
        int maxSemesters, 
        CancellationToken cancellationToken)
    {
        var currentId = filter.SemesterId ?? await _repository.GetLatestSemesterIdAsync(cancellationToken);
        if (currentId == null) return Result.Failure<AcademicTrendResponse>(new Error("SEMESTER_NOT_FOUND", "Không tìm thấy học kỳ hiện tại.", ErrorType.NotFound));

        var semesters = await _repository.ReadTrendSemestersAsync(currentId.Value, maxSemesters, cancellationToken);
        var semesterIds = semesters.Select(s => s.SemesterId).ToList();
        var allScores = await _repository.ReadAcademicScoresAsync(semesterIds, filter, cancellationToken);

        var trendPoints = new List<AcademicTrendPoint>();
        foreach (var sem in semesters)
        {
            var semScores = allScores.Where(s => s.SemesterId == sem.SemesterId).Select(s => s.Score).ToList();
            var dist = _statsCalculator.CalculateDistribution(semScores);
            var pass = _statsCalculator.CalculateThresholdRate(semScores, x => x >= 5.0m);

            trendPoints.Add(new AcademicTrendPoint(
                sem.SemesterId,
                sem.Name,
                sem.AcademicYearStart,
                sem.AcademicYearEnd,
                dist.Mean,
                dist.Median,
                dist.StandardDeviation,
                pass.Percentage,
                (100 - (pass.Percentage ?? 0)),
                0,
                semScores.Count,
                semScores.Count // Approximation
            ));
        }

        return Result<AcademicTrendResponse>.Success(new AcademicTrendResponse(
            new AdvancedMetricMetadata("2.0.0", "2.0.0", "2.0.0", DateTime.UtcNow),
            trendPoints));
    }

    public async Task<Result<StudentRiskResponse>> ReadStudentRiskAsync(
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken)
    {
        var currentId = filter.SemesterId ?? await _repository.GetLatestSemesterIdAsync(cancellationToken);
        if (currentId == null) return Result.Failure<StudentRiskResponse>(new Error("SEMESTER_NOT_FOUND", "Không tìm thấy học kỳ hiện tại.", ErrorType.NotFound));

        var previousId = filter.PreviousSemesterId ?? await _repository.GetPreviousSemesterIdAsync(currentId.Value, cancellationToken);
        
        var riskInputs = await _repository.ReadStudentRiskInputsAsync(currentId.Value, previousId, filter, cancellationToken);
        var studentRiskItems = riskInputs.Select(input => _riskCalculator.Calculate(input)).ToList();

        if (!string.IsNullOrEmpty(filter.RiskLevel))
        {
            studentRiskItems = studentRiskItems.Where(x => x.RiskLevel.Equals(filter.RiskLevel, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var summary = new StudentRiskSummary(
            studentRiskItems.Count,
            studentRiskItems.Count(x => x.RiskLevel == "LOW"),
            studentRiskItems.Count(x => x.RiskLevel == "MEDIUM"),
            studentRiskItems.Count(x => x.RiskLevel == "HIGH"),
            studentRiskItems.Count(x => x.RiskLevel == "CRITICAL"));

        return Result<StudentRiskResponse>.Success(new StudentRiskResponse(
            new AdvancedMetricMetadata("2.0.0", "2.0.0", "2.0.0", DateTime.UtcNow),
            summary,
            studentRiskItems.Skip(filter.Skip).Take(filter.Take).ToList()));
    }
}
