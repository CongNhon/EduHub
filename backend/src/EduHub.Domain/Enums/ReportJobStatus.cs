namespace EduHub.Domain.Enums;

/// <summary>
/// Ghi chú: ReportJobStatus liệt kê trạng thái xử lý PDF report job.
/// </summary>
public enum ReportJobStatus
{
    Queued,
    Processing,
    Completed,
    Failed,
    Expired
}
