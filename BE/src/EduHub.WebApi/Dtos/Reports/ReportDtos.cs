using EduHub.Domain.Enums;

namespace EduHub.WebApi.Dtos.Reports;

/// <summary>
/// Ghi chú: RequestReportCardRequest là DTO API để requester tạo PDF bảng điểm.
/// </summary>
public sealed record RequestReportCardRequest(Guid StudentId, Guid SemesterId, string IdempotencyKey);

/// <summary>
/// Ghi chú: ReportJobDto là DTO API trả trạng thái report job.
/// </summary>
public sealed record ReportJobDto(
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
/// Ghi chú: CreateReportRequestRequest chứa học sinh, học kỳ và mục đích phụ huynh cần báo cáo.
/// </summary>
public sealed record CreateReportRequestRequest(Guid StudentId, Guid SemesterId, string Purpose);

/// <summary>
/// Ghi chú: ListReportRequestsRequest chứa bộ lọc inbox/lịch sử yêu cầu báo cáo.
/// </summary>
public sealed record ListReportRequestsRequest(ReportRequestStatus? Status, int? Page, int? PageSize);

/// <summary>
/// Ghi chú: ReviewReportRequestRequest chứa quyết định duyệt/từ chối và ghi chú của quản trị học vụ.
/// </summary>
public sealed record ReviewReportRequestRequest(bool Approve, string? Note);

/// <summary>
/// Ghi chú: ReportRequestDto trả toàn bộ context nghiệp vụ của một yêu cầu báo cáo.
/// </summary>
public sealed record ReportRequestDto(Guid Id, Guid StudentId, string StudentCode, string StudentName, Guid SemesterId, string SemesterName, Guid RequesterUserId, string RequesterName, Guid? ReviewerUserId, string? ReviewerName, Guid? ReportJobId, string Purpose, string? ReviewNote, string Status, string? JobStatus, DateTime RequestedAtUtc, DateTime? ReviewedAtUtc);
