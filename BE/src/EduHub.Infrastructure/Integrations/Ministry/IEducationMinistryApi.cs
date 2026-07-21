using Refit;

namespace EduHub.Infrastructure.Integrations.Ministry;

/// <summary>
/// Ghi chú: IEducationMinistryApi là Refit interface gọi Ministry API version v1.
/// </summary>
public interface IEducationMinistryApi
{
    /// <summary>
    /// Ghi chú: SyncGradebookAsync gửi sổ điểm với Idempotency-Key cố định theo aggregate/version.
    /// </summary>
    [Post("/api/v1/gradebooks")]
    Task<MinistryGradebookResponse> SyncGradebookAsync(
        [Header("Idempotency-Key")] string idempotencyKey,
        [Body] MinistryGradebookRequest request,
        CancellationToken cancellationToken);
}
