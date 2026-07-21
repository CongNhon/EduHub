using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Reports;

namespace EduHub.Application.Interfaces.Services.Reports;

/// <summary>
/// Ghi chú: IReportService xử lý request/status/download PDF report.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Ghi chú: RequestReportCardAsync tạo report job và enqueue Hangfire job sinh PDF.
    /// </summary>
    Task<Result<ReportJobResponse>> RequestReportCardAsync(
        RequestReportCardCommand request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetReportJobAsync đọc trạng thái report job nếu user có quyền.
    /// </summary>
    Task<Result<ReportJobResponse>> GetReportJobAsync(GetReportJobQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetReportDownloadAsync tải file PDF nếu job completed, chưa expired và user có quyền.
    /// </summary>
    Task<Result<ReportDownloadResponse>> GetReportDownloadAsync(
        GetReportDownloadQuery request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: CreateReportRequestAsync tạo yêu cầu phụ huynh gửi quản trị học vụ.
    /// </summary>
    Task<Result<ReportRequestResponse>> CreateReportRequestAsync(CreateReportRequestCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ListReportRequestsAsync đọc inbox duyệt hoặc lịch sử yêu cầu theo role.
    /// </summary>
    Task<Result<PagedResult<ReportRequestResponse>>> ListReportRequestsAsync(ListReportRequestsQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ReviewReportRequestAsync duyệt/từ chối và enqueue PDF khi được duyệt.
    /// </summary>
    Task<Result<ReportRequestResponse>> ReviewReportRequestAsync(ReviewReportRequestCommand request, CancellationToken cancellationToken);
}
