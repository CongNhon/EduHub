using EduHub.Application.Contracts.Integrations;
using EduHub.WebApi.Dtos.Integrations;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: IntegrationMappings map giữa Integration DTO API và contract Application.
/// </summary>
public static class IntegrationMappings
{
    /// <summary>
    /// Ghi chú: ToCommand chuyển RetryGradeSyncRequest API thành RetryGradeSyncCommand cho assignment.
    /// </summary>
    public static RetryGradeSyncCommand ToCommand(this RetryGradeSyncRequest request, Guid assignmentId) =>
        new(assignmentId, request.Reason);

    /// <summary>
    /// Ghi chú: ToStatusQuery tạo GetExternalSyncRecordQuery từ sync record id.
    /// </summary>
    public static GetExternalSyncRecordQuery ToStatusQuery(Guid id) => new(id);

    /// <summary>
    /// Ghi chú: ToDto chuyển ExternalSyncRecordResponse application thành ExternalSyncRecordDto API.
    /// </summary>
    public static ExternalSyncRecordDto ToDto(this ExternalSyncRecordResponse response) =>
        new(
            response.Id,
            response.AggregateType,
            response.AggregateId,
            response.Version,
            response.IdempotencyKey,
            response.Status,
            response.Attempts,
            response.ExternalId,
            response.ExternalVersion,
            response.LastError,
            response.NextRetryAtUtc,
            response.SucceededAtUtc);
}
