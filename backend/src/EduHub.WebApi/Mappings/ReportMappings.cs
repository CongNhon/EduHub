using EduHub.Application.Contracts.Reports;
using EduHub.WebApi.Dtos.Reports;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: ReportMappings map giữa Report DTO API và contract Application.
/// </summary>
public static class ReportMappings
{
    /// <summary>
    /// Ghi chú: ToCommand chuyển RequestReportCardRequest API thành RequestReportCardCommand application.
    /// </summary>
    public static RequestReportCardCommand ToCommand(this RequestReportCardRequest request) =>
        new(request.StudentId, request.SemesterId, request.IdempotencyKey);

    /// <summary>
    /// Ghi chú: ToQuery tạo GetReportJobQuery application từ route report job id.
    /// </summary>
    public static GetReportJobQuery ToStatusQuery(Guid id) => new(id);

    /// <summary>
    /// Ghi chú: ToQuery tạo GetReportDownloadQuery application từ route report job id.
    /// </summary>
    public static GetReportDownloadQuery ToDownloadQuery(Guid id) => new(id);

    /// <summary>
    /// Ghi chú: ToDto chuyển ReportJobResponse application thành ReportJobDto API.
    /// </summary>
    public static ReportJobDto ToDto(this ReportJobResponse response) =>
        new(
            response.Id,
            response.StudentId,
            response.SemesterId,
            response.Status,
            response.ChecksumSha256,
            response.PolicyVersion,
            response.GeneratedAtUtc,
            response.ExpiresAtUtc,
            response.FailureReason);

    public static CreateReportRequestCommand ToCommand(this CreateReportRequestRequest request) =>
        new(request.StudentId, request.SemesterId, request.Purpose);

    public static ListReportRequestsQuery ToQuery(this ListReportRequestsRequest request) =>
        new(request.Status, request.Page ?? 1, request.PageSize ?? 20);

    public static ReviewReportRequestCommand ToCommand(this ReviewReportRequestRequest request, Guid id) =>
        new(id, request.Approve, request.Note);

    public static ReportRequestDto ToDto(this ReportRequestResponse response) =>
        new(response.Id, response.StudentId, response.StudentCode, response.StudentName, response.SemesterId, response.SemesterName, response.RequesterUserId, response.RequesterName, response.ReviewerUserId, response.ReviewerName, response.ReportJobId, response.Purpose, response.ReviewNote, response.Status, response.JobStatus, response.RequestedAtUtc, response.ReviewedAtUtc);
}
