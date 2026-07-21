using System.Text.Json;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Reports;
using EduHub.Application.Features.Reports.Common;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.Reports;
using EduHub.Application.Interfaces.Services.Reports;
using EduHub.Domain.Entities.Reports;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Enums;

namespace EduHub.Application.Services.Reports;

/// <summary>
/// Ghi chú: ReportService xử lý tạo job PDF, đọc trạng thái và tải file report theo quyền requester.
/// </summary>
public sealed class ReportService(
    IReportJobRepository reportJobRepository,
    IReportFileStorage reportFileStorage,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IReportService
{
    /// <summary>
    /// Ghi chú: RequestReportCardAsync authorize requester, tạo report job idempotent và enqueue Hangfire.
    /// </summary>
    public async Task<Result<ReportJobResponse>> RequestReportCardAsync(
        RequestReportCardCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role is not (UserRole.AcademicAdmin or UserRole.SystemAdmin))
        {
            return Result.Failure<ReportJobResponse>(ReportErrors.AcademicAdminRequired);
        }

        if (currentUser.UserId is null)
        {
            return Result.Failure<ReportJobResponse>(ReportErrors.Unauthorized);
        }

        var permission = await ValidateReportAccessAsync(currentUser.UserId.Value, request.StudentId, cancellationToken);
        if (permission.IsFailure)
        {
            return Result.Failure<ReportJobResponse>(permission.Error!);
        }

        if (!await reportJobRepository.StudentExistsAsync(request.StudentId, cancellationToken))
        {
            return Result.Failure<ReportJobResponse>(ReportErrors.StudentNotFound);
        }

        if (!await reportJobRepository.SemesterExistsAsync(request.SemesterId, cancellationToken))
        {
            return Result.Failure<ReportJobResponse>(ReportErrors.SemesterNotFound);
        }

        var existing = await reportJobRepository.GetByRequesterAndIdempotencyKeyAsync(
            currentUser.UserId.Value,
            request.IdempotencyKey,
            cancellationToken);
        if (existing is not null)
        {
            if (existing.StudentId != request.StudentId || existing.SemesterId != request.SemesterId)
            {
                return Result.Failure<ReportJobResponse>(ReportErrors.IdempotencyPayloadMismatch);
            }

            return Result.Success(ToResponse(existing));
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var reportJob = new ReportJob(
            currentUser.UserId.Value,
            request.StudentId,
            request.SemesterId,
            request.IdempotencyKey,
            now);

        reportJobRepository.Add(reportJob);
        reportJobRepository.AddOutboxMessage(new OutboxMessage(
            "ReportJobRequested",
            JsonSerializer.Serialize(new { reportJobId = reportJob.Id }),
            now));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(reportJob));
    }

    /// <summary>
    /// Ghi chú: CreateReportRequestAsync tạo yêu cầu Pending nếu phụ huynh có quyền với học sinh và chưa có yêu cầu mở trùng.
    /// </summary>
    public async Task<Result<ReportRequestResponse>> CreateReportRequestAsync(CreateReportRequestCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != UserRole.Parent || currentUser.UserId is null)
            return Result.Failure<ReportRequestResponse>(ReportErrors.ParentRequired);
        var requesterId = currentUser.UserId.Value;
        if (!await reportJobRepository.ParentCanReadStudentAsync(requesterId, request.StudentId, cancellationToken))
            return Result.Failure<ReportRequestResponse>(ReportErrors.Forbidden);
        if (!await reportJobRepository.SemesterExistsAsync(request.SemesterId, cancellationToken))
            return Result.Failure<ReportRequestResponse>(ReportErrors.SemesterNotFound);
        if (!await reportJobRepository.StudentWasEnrolledAsync(request.StudentId, request.SemesterId, cancellationToken))
            return Result.Failure<ReportRequestResponse>(ReportErrors.StudentNotEnrolled);
        if (!await reportJobRepository.StudentHasPublishedGradesAsync(request.StudentId, request.SemesterId, cancellationToken))
            return Result.Failure<ReportRequestResponse>(ReportErrors.NoPublishedGrades);
        if (await reportJobRepository.HasOpenRequestAsync(requesterId, request.StudentId, request.SemesterId, cancellationToken))
            return Result.Failure<ReportRequestResponse>(ReportErrors.RequestExists);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var reportRequest = new ReportRequest(requesterId, request.StudentId, request.SemesterId, request.Purpose, now);
        reportJobRepository.AddRequest(reportRequest);
        reportJobRepository.AddOutboxMessage(new OutboxMessage("ReportRequested", JsonSerializer.Serialize(new { reportRequestId = reportRequest.Id, requesterUserId = requesterId, studentId = request.StudentId }), now));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success((await reportJobRepository.GetRequestResponseAsync(reportRequest.Id, cancellationToken))!);
    }

    /// <summary>
    /// Ghi chú: ListReportRequestsAsync trả lịch sử của Parent hoặc inbox toàn trường cho AcademicAdmin/SystemAdmin.
    /// </summary>
    public async Task<Result<PagedResult<ReportRequestResponse>>> ListReportRequestsAsync(ListReportRequestsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null || currentUser.Role is not (UserRole.Parent or UserRole.AcademicAdmin or UserRole.SystemAdmin))
            return Result.Failure<PagedResult<ReportRequestResponse>>(ReportErrors.Forbidden);
        var page = PageRequest.Create(request.Page, request.PageSize);
        if (page.IsFailure) return Result.Failure<PagedResult<ReportRequestResponse>>(page.Error!);
        return Result.Success(await reportJobRepository.ListRequestsAsync(currentUser.UserId.Value, currentUser.Role.Value, request.Status, page.Value, cancellationToken));
    }

    /// <summary>
    /// Ghi chú: ReviewReportRequestAsync duyệt yêu cầu, tạo Hangfire job hoặc từ chối kèm lý do.
    /// </summary>
    public async Task<Result<ReportRequestResponse>> ReviewReportRequestAsync(ReviewReportRequestCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Role is not (UserRole.AcademicAdmin or UserRole.SystemAdmin) || currentUser.UserId is null)
            return Result.Failure<ReportRequestResponse>(ReportErrors.AcademicAdminRequired);
        if (!request.Approve && string.IsNullOrWhiteSpace(request.Note))
            return Result.Failure<ReportRequestResponse>(ReportErrors.ReviewReasonRequired);
        var reportRequest = await reportJobRepository.GetRequestAsync(request.Id, cancellationToken);
        if (reportRequest is null) return Result.Failure<ReportRequestResponse>(ReportErrors.RequestNotFound);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        try
        {
            if (request.Approve)
            {
                reportRequest.Approve(currentUser.UserId.Value, request.Note, now);
                var reportJob = new ReportJob(reportRequest.RequesterUserId, reportRequest.StudentId, reportRequest.SemesterId, $"report-request:{reportRequest.Id:N}", now);
                reportJobRepository.Add(reportJob);
                reportRequest.AttachJob(reportJob.Id, now);
                reportJobRepository.AddOutboxMessage(new OutboxMessage("ReportJobRequested", JsonSerializer.Serialize(new { reportJobId = reportJob.Id }), now));
                reportJobRepository.AddOutboxMessage(new OutboxMessage("ReportRequestApproved", JsonSerializer.Serialize(new { reportRequestId = reportRequest.Id, recipientUserId = reportRequest.RequesterUserId, studentId = reportRequest.StudentId }), now));
            }
            else
            {
                reportRequest.Reject(currentUser.UserId.Value, request.Note!, now);
                reportJobRepository.AddOutboxMessage(new OutboxMessage("ReportRequestRejected", JsonSerializer.Serialize(new { reportRequestId = reportRequest.Id, recipientUserId = reportRequest.RequesterUserId, studentId = reportRequest.StudentId, reason = request.Note }), now));
            }
        }
        catch (InvalidOperationException)
        {
            return Result.Failure<ReportRequestResponse>(ReportErrors.InvalidRequestState);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success((await reportJobRepository.GetRequestResponseAsync(reportRequest.Id, cancellationToken))!);
    }

    /// <summary>
    /// Ghi chú: GetReportJobAsync đọc trạng thái report job nếu current user là requester hoặc admin.
    /// </summary>
    public async Task<Result<ReportJobResponse>> GetReportJobAsync(GetReportJobQuery request, CancellationToken cancellationToken)
    {
        var reportJob = await GetAuthorizedReportJobAsync(request.Id, cancellationToken);
        return reportJob.IsFailure ? Result.Failure<ReportJobResponse>(reportJob.Error!) : Result.Success(ToResponse(reportJob.Value));
    }

    /// <summary>
    /// Ghi chú: GetReportDownloadAsync đọc file PDF nếu report completed và chưa hết hạn.
    /// </summary>
    public async Task<Result<ReportDownloadResponse>> GetReportDownloadAsync(
        GetReportDownloadQuery request,
        CancellationToken cancellationToken)
    {
        var reportJobResult = await GetAuthorizedReportJobAsync(request.Id, cancellationToken);
        if (reportJobResult.IsFailure)
        {
            return Result.Failure<ReportDownloadResponse>(reportJobResult.Error!);
        }

        var reportJob = reportJobResult.Value;
        if (reportJob.Status != ReportJobStatus.Completed || string.IsNullOrWhiteSpace(reportJob.StorageKey))
        {
            return Result.Failure<ReportDownloadResponse>(ReportErrors.NotCompleted);
        }

        if (reportJob.ExpiresAtUtc <= timeProvider.GetUtcNow().UtcDateTime)
        {
            reportJob.MarkExpired(timeProvider.GetUtcNow().UtcDateTime);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<ReportDownloadResponse>(ReportErrors.Expired);
        }

        var content = await reportFileStorage.ReadAsync(reportJob.StorageKey, cancellationToken);
        return Result.Success(new ReportDownloadResponse($"report-{reportJob.Id:N}.pdf", "application/pdf", content));
    }

    /// <summary>
    /// Ghi chú: GetAuthorizedReportJobAsync lấy report job và kiểm tra current user có quyền xem/tải.
    /// </summary>
    private async Task<Result<ReportJob>> GetAuthorizedReportJobAsync(Guid reportJobId, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<ReportJob>(ReportErrors.Unauthorized);
        }

        var reportJob = await reportJobRepository.GetAsync(reportJobId, cancellationToken);
        if (reportJob is null)
        {
            return Result.Failure<ReportJob>(ReportErrors.JobNotFound);
        }

        if (currentUser.Role is UserRole.AcademicAdmin or UserRole.SystemAdmin || reportJob.RequesterUserId == currentUser.UserId.Value)
        {
            return Result.Success(reportJob);
        }

        return Result.Failure<ReportJob>(ReportErrors.Forbidden);
    }

    /// <summary>
    /// Ghi chú: ValidateReportAccessAsync kiểm tra requester có quyền tạo report cho học sinh.
    /// </summary>
    private async Task<Result> ValidateReportAccessAsync(Guid requesterUserId, Guid studentId, CancellationToken cancellationToken)
    {
        if (currentUser.Role is UserRole.AcademicAdmin or UserRole.SystemAdmin)
        {
            return Result.Success();
        }

        if (currentUser.Role == UserRole.Parent &&
            await reportJobRepository.ParentCanReadStudentAsync(requesterUserId, studentId, cancellationToken))
        {
            return Result.Success();
        }

        return Result.Failure(ReportErrors.Forbidden);
    }

    /// <summary>
    /// Ghi chú: ToResponse chuyển ReportJob entity thành ReportJobResponse trả về API.
    /// </summary>
    private static ReportJobResponse ToResponse(ReportJob reportJob) =>
        new(
            reportJob.Id,
            reportJob.StudentId,
            reportJob.SemesterId,
            reportJob.Status.ToString(),
            reportJob.ChecksumSha256,
            reportJob.PolicyVersion,
            reportJob.GeneratedAtUtc,
            reportJob.ExpiresAtUtc,
            reportJob.FailureReason);
}
