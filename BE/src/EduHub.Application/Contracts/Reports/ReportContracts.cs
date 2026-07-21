using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Domain.Enums;

namespace EduHub.Application.Contracts.Reports;

/// <summary>
/// Ghi chú: RequestReportCardCommand là command enqueue job sinh PDF bảng điểm cho một học sinh.
/// </summary>
public sealed record RequestReportCardCommand(Guid StudentId, Guid SemesterId, string IdempotencyKey)
    : ICommand<Result<ReportJobResponse>>;

/// <summary>
/// Ghi chú: GetReportJobQuery là query đọc trạng thái report job theo id.
/// </summary>
public sealed record GetReportJobQuery(Guid Id) : IQuery<Result<ReportJobResponse>>;

/// <summary>
/// Ghi chú: GetReportDownloadQuery là query lấy file PDF của report job đã completed và chưa expired.
/// </summary>
public sealed record GetReportDownloadQuery(Guid Id) : IQuery<Result<ReportDownloadResponse>>;

/// <summary>
/// Ghi chú: ReportJobResponse là dữ liệu trạng thái report job trả về API.
/// </summary>
public sealed record ReportJobResponse(
    Guid Id,
    Guid StudentId,
    Guid SemesterId,
    string Status,
    string? ChecksumSha256,
    string? PolicyVersion,
    DateTime? GeneratedAtUtc,
    DateTime? ExpiresAtUtc,
    string? FailureReason);

/// <summary>
/// Ghi chú: ReportDownloadResponse là dữ liệu file PDF đã được authorize để API trả về.
/// </summary>
public sealed record ReportDownloadResponse(string FileName, string ContentType, byte[] Content);

/// <summary>
/// Ghi chú: ReportRequestResponse trả yêu cầu báo cáo cùng học sinh, học kỳ, requester, reviewer và report job.
/// </summary>
public sealed record ReportRequestResponse(Guid Id, Guid StudentId, string StudentCode, string StudentName, Guid SemesterId, string SemesterName, Guid RequesterUserId, string RequesterName, Guid? ReviewerUserId, string? ReviewerName, Guid? ReportJobId, string Purpose, string? ReviewNote, string Status, string? JobStatus, DateTime RequestedAtUtc, DateTime? ReviewedAtUtc);

/// <summary>
/// Ghi chú: CreateReportRequestCommand tạo yêu cầu phụ huynh gửi quản trị học vụ duyệt.
/// </summary>
public sealed record CreateReportRequestCommand(Guid StudentId, Guid SemesterId, string Purpose) : ICommand<Result<ReportRequestResponse>>;

/// <summary>
/// Ghi chú: ListReportRequestsQuery đọc inbox của AcademicAdmin hoặc lịch sử của phụ huynh.
/// </summary>
public sealed record ListReportRequestsQuery(ReportRequestStatus? Status, int Page, int PageSize) : IQuery<Result<PagedResult<ReportRequestResponse>>>;

/// <summary>
/// Ghi chú: ReviewReportRequestCommand cho AcademicAdmin duyệt hoặc từ chối yêu cầu báo cáo.
/// </summary>
public sealed record ReviewReportRequestCommand(Guid Id, bool Approve, string? Note) : ICommand<Result<ReportRequestResponse>>;
