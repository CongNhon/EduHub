using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Integrations;
using EduHub.Application.Features.Integrations.Common;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.Integrations;
using EduHub.Application.Interfaces.Services.Integrations;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Enums;

namespace EduHub.Application.Services.Integrations;

/// <summary>
/// Ghi chú: ExternalSyncService xử lý admin đọc và manual retry Ministry sync records.
/// </summary>
public sealed class ExternalSyncService(
    IExternalSyncRepository externalSyncRepository,
    IExternalSyncJobScheduler externalSyncJobScheduler,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IExternalSyncService
{
    /// <summary>
    /// Ghi chú: RetryGradeSyncAsync reset sync record mới nhất của assignment và enqueue retry.
    /// </summary>
    public async Task<Result<ExternalSyncRecordResponse>> RetryGradeSyncAsync(
        RetryGradeSyncCommand request,
        CancellationToken cancellationToken)
    {
        var permission = ValidateAdmin();
        if (permission.IsFailure)
        {
            return Result.Failure<ExternalSyncRecordResponse>(permission.Error!);
        }

        var record = await externalSyncRepository.GetLatestForAssignmentAsync(request.AssignmentId, cancellationToken);
        if (record is null)
        {
            return Result.Failure<ExternalSyncRecordResponse>(IntegrationErrors.AssignmentNotSynced);
        }

        record.MarkManualRetry(currentUser.UserId!.Value, request.Reason, timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        _ = externalSyncJobScheduler.EnqueueSyncRecord(record.Id);
        return Result.Success(ToResponse(record));
    }

    /// <summary>
    /// Ghi chú: GetSyncRecordAsync cho admin đọc trạng thái một sync record.
    /// </summary>
    public async Task<Result<ExternalSyncRecordResponse>> GetSyncRecordAsync(
        GetExternalSyncRecordQuery request,
        CancellationToken cancellationToken)
    {
        var permission = ValidateAdmin();
        if (permission.IsFailure)
        {
            return Result.Failure<ExternalSyncRecordResponse>(permission.Error!);
        }

        var record = await externalSyncRepository.GetAsync(request.Id, cancellationToken);
        return record is null
            ? Result.Failure<ExternalSyncRecordResponse>(IntegrationErrors.SyncRecordNotFound)
            : Result.Success(ToResponse(record));
    }

    /// <summary>
    /// Ghi chú: ValidateAdmin kiểm tra current user có quyền AcademicAdmin/SystemAdmin để quản lý Ministry sync.
    /// </summary>
    private Result ValidateAdmin()
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure(IntegrationErrors.Unauthorized);
        }

        return currentUser.Role is UserRole.AcademicAdmin or UserRole.SystemAdmin
            ? Result.Success()
            : Result.Failure(IntegrationErrors.Forbidden);
    }

    /// <summary>
    /// Ghi chú: ToResponse chuyển ExternalSyncRecord entity thành response trạng thái Ministry sync cho admin.
    /// </summary>
    private static ExternalSyncRecordResponse ToResponse(ExternalSyncRecord record) =>
        new(
            record.Id,
            record.AggregateType,
            record.AggregateId,
            record.Version,
            record.IdempotencyKey,
            record.Status.ToString(),
            record.Attempts,
            record.ExternalId,
            record.ExternalVersion,
            record.LastError,
            record.NextRetryAtUtc,
            record.SucceededAtUtc);
}
