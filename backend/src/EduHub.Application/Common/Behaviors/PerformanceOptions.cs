namespace EduHub.Application.Common.Behaviors;

/// <summary>
/// Ghi chú: PerformanceOptions chứa cấu hình cho performance.
/// </summary>
public sealed class PerformanceOptions
{
    public int WarningThresholdMilliseconds { get; init; } = 500;
}
