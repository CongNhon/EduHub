using System.Text.Json;
using EduHub.Application.Interfaces.Services.Integrations;
using EduHub.Domain.Entities.Integration;

namespace EduHub.Infrastructure.Integrations.Ministry;

/// <summary>
/// Ghi chú: EducationMinistryGateway map ExternalSyncRecord payload và gọi Ministry API qua Refit.
/// </summary>
public sealed class EducationMinistryGateway(IEducationMinistryApi ministryApi) : IEducationMinistryGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Ghi chú: SyncGradebookAsync gửi payload sổ điểm trong ExternalSyncRecord sang Ministry API.
    /// </summary>
    public async Task<MinistrySyncResult> SyncGradebookAsync(
        ExternalSyncRecord record,
        CancellationToken cancellationToken)
    {
        var request = JsonSerializer.Deserialize<MinistryGradebookRequest>(record.Payload, JsonOptions)
            ?? throw new InvalidOperationException("Ministry gradebook payload is invalid.");

        var response = await ministryApi.SyncGradebookAsync(
            record.IdempotencyKey,
            request,
            cancellationToken);

        return new MinistrySyncResult(response.ExternalId, response.ExternalVersion);
    }
}
