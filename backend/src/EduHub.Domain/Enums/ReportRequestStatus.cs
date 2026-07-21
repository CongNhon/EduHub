namespace EduHub.Domain.Enums;

/// <summary>
/// Ghi chú: ReportRequestStatus mô tả trạng thái yêu cầu báo cáo từ phụ huynh tới quản trị học vụ.
/// </summary>
public enum ReportRequestStatus
{
    Pending = 0,
    Reviewing = 1,
    Approved = 2,
    Rejected = 3,
    Generating = 4,
    Completed = 5,
    Failed = 6
}
