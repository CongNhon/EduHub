using EduHub.Application.Contracts.Analytics;
using EduHub.WebApi.Dtos.Analytics;

namespace EduHub.WebApi.Mappings.Analytics;

/// <summary>
/// Ghi chú: AdminAdvancedAnalyticsMappings thực hiện mapping giữa Application Contract và WebApi DTO.
/// </summary>
public static class AdminAdvancedAnalyticsMappings
{
    public static AdminAdvancedAnalyticsFilter ToFilter(this AdminAdvancedAnalyticsRequest request)
    {
        return new AdminAdvancedAnalyticsFilter(
            request.SemesterId,
            request.PreviousSemesterId,
            request.GradeLevels,
            request.ClassIds,
            request.SubjectIds,
            request.TeacherIds,
            request.RiskLevel,
            request.Skip,
            request.Take);
    }

    public static AdminAdvancedSummaryDto ToDto(this AdminAdvancedSummaryResponse response)
    {
        return new AdminAdvancedSummaryDto(
            response.Metadata.ToDto(),
            response.AverageScore.ToDto(),
            response.PassRate.ToDto(),
            response.ExcellentRate.ToDto(),
            response.MissingGradeRate.ToDto(),
            response.Growth.ToDto(),
            response.DataQuality.ToDto());
    }

    public static AdvancedMetricMetadataDto ToDto(this AdvancedMetricMetadata metadata)
    {
        return new AdvancedMetricMetadataDto(
            metadata.MetricVersion,
            metadata.RiskModelVersion,
            metadata.QualityModelVersion,
            metadata.GeneratedAt);
    }

    public static CommonDecimalMetricDto ToDto(this CommonDecimalMetric metric)
    {
        return new CommonDecimalMetricDto(
            metric.Value,
            metric.PreviousValue,
            metric.AbsoluteChange,
            metric.PercentageChange,
            metric.Trend);
    }

    public static GrowthSummaryDto ToDto(this GrowthSummary summary)
    {
        return new GrowthSummaryDto(
            summary.TotalCount,
            summary.ImprovedCount,
            summary.StableCount,
            summary.DeclinedCount,
            summary.MeanGrowth,
            summary.MedianGrowth);
    }

    public static DataQualityScoreSummaryDto ToDto(this DataQualityScoreSummary quality)
    {
        return new DataQualityScoreSummaryDto(
            quality.OverallScore,
            quality.Completeness,
            quality.Validity,
            quality.Consistency,
            quality.Integrity,
            quality.Uniqueness,
            quality.Freshness);
    }

    public static AcademicDistributionDto ToDto(this AcademicDistributionResponse response)
    {
        return new AcademicDistributionDto(
            response.Metadata.ToDto(),
            response.Overall.ToDto(),
            response.Buckets.Select(b => b.ToDto()).ToList(),
            response.Grouped.Select(g => g.ToDto()).ToList());
    }

    public static ScoreDistributionMetricsDto ToDto(this ScoreDistributionMetrics metrics)
    {
        return new ScoreDistributionMetricsDto(
            metrics.SampleSize,
            metrics.Mean,
            metrics.Median,
            metrics.Min,
            metrics.Max,
            metrics.StandardDeviation,
            metrics.Variance,
            metrics.P10,
            metrics.Q1,
            metrics.Q3,
            metrics.P90,
            metrics.InterquartileRange);
    }

    public static ScoreBucketMetricDto ToDto(this ScoreBucketMetric bucket)
    {
        return new ScoreBucketMetricDto(
            bucket.Code,
            bucket.Name,
            bucket.Count,
            bucket.Percentage);
    }

    public static GroupedDistributionItemDto ToDto(this GroupedDistributionItem item)
    {
        return new GroupedDistributionItemDto(
            item.GroupKey,
            item.GroupName,
            item.Metrics.ToDto());
    }

    public static AcademicTrendDto ToDto(this AcademicTrendResponse response)
    {
        return new AcademicTrendDto(
            response.Metadata.ToDto(),
            response.Points.Select(p => p.ToDto()).ToList());
    }

    public static AcademicTrendPointDto ToDto(this AcademicTrendPoint point)
    {
        return new AcademicTrendPointDto(
            point.SemesterId,
            point.SemesterName,
            point.AcademicYearStart,
            point.AcademicYearEnd,
            point.Mean,
            point.Median,
            point.StandardDeviation,
            point.PassRate,
            point.FailureRate,
            point.MissingRate,
            point.ValidScoreCount,
            point.StudentCount);
    }

    public static StudentRiskDto ToDto(this StudentRiskResponse response)
    {
        return new StudentRiskDto(
            response.Metadata.ToDto(),
            response.Summary.ToDto(),
            response.Items.Select(i => i.ToDto()).ToList());
    }

    public static StudentRiskSummaryDto ToDto(this StudentRiskSummary summary)
    {
        return new StudentRiskSummaryDto(
            summary.Total,
            summary.Low,
            summary.Medium,
            summary.High,
            summary.Critical);
    }

    public static StudentRiskItemDto ToDto(this StudentRiskItem item)
    {
        return new StudentRiskItemDto(
            item.StudentId,
            item.StudentCode,
            item.StudentName,
            item.ClassId,
            item.ClassCode,
            item.ClassName,
            item.GradeLevel,
            item.RiskScore,
            item.RiskLevel,
            item.CurrentAverage,
            item.PreviousAverage,
            item.Growth,
            item.FailedSubjectCount,
            item.TotalSubjectCount,
            item.MissingGradeRate,
            item.CurrentPercentileInGrade,
            item.Reasons.Select(r => r.ToDto()).ToList());
    }

    public static StudentRiskReasonDto ToDto(this StudentRiskReason reason)
    {
        return new StudentRiskReasonDto(
            reason.Code,
            reason.Message);
    }
}
