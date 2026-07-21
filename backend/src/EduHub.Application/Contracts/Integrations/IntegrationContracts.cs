using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;

namespace EduHub.Application.Contracts.Integrations;

/// <summary>
/// Ghi chú: ExternalSyncRecordResponse là dữ liệu trạng thái sync Ministry API trả về admin.
/// </summary>
public sealed record ExternalSyncRecordResponse(
    Guid Id,
    string AggregateType,
    Guid AggregateId,
    int Version,
    string IdempotencyKey,
    string Status,
    int Attempts,
    string? ExternalId,
    string? ExternalVersion,
    string? LastError,
    DateTime? NextRetryAtUtc,
    DateTime? SucceededAtUtc);

/// <summary>
/// Ghi chú: RetryGradeSyncCommand là command admin retry sync Ministry cho một assignment bằng idempotency key cũ.
/// </summary>
public sealed record RetryGradeSyncCommand(Guid AssignmentId, string Reason) : ICommand<Result<ExternalSyncRecordResponse>>;

/// <summary>
/// Ghi chú: GetExternalSyncRecordQuery là query admin đọc trạng thái sync record.
/// </summary>
public sealed record GetExternalSyncRecordQuery(Guid Id) : IQuery<Result<ExternalSyncRecordResponse>>;
