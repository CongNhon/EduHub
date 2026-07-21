using EduHub.Application.Common.Models;

namespace EduHub.Application.Features.Integrations.Common;

/// <summary>
/// Ghi chú: IntegrationErrors chứa lỗi nghiệp vụ cho Ministry sync.
/// </summary>
public static class IntegrationErrors
{
    public static readonly Error Unauthorized = new("Integration.Unauthorized", "Authentication is required.", ErrorType.Unauthorized);

    public static readonly Error Forbidden = new("Integration.Forbidden", "AcademicAdmin role is required.", ErrorType.Forbidden);

    public static readonly Error SyncRecordNotFound = new("Integration.SyncRecordNotFound", "External sync record was not found.", ErrorType.NotFound);

    public static readonly Error AssignmentNotSynced = new("Integration.AssignmentNotSynced", "Assignment has no sync record to retry.", ErrorType.Conflict);
}
