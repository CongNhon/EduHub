using EduHub.Application.Common.Models;

namespace EduHub.Application.Features.Reports.Common;

/// <summary>
/// Ghi chú: ReportErrors chứa mã lỗi nghiệp vụ cho request/status/download report PDF.
/// </summary>
public static class ReportErrors
{
    public static readonly Error Unauthorized = new("Report.Unauthorized", "Authentication is required.", ErrorType.Unauthorized);

    public static readonly Error Forbidden = new("Report.Forbidden", "Current user cannot access this report.", ErrorType.Forbidden);

    public static readonly Error StudentNotFound = new("Report.StudentNotFound", "Student was not found.", ErrorType.NotFound);

    public static readonly Error SemesterNotFound = new("Report.SemesterNotFound", "Semester was not found.", ErrorType.NotFound);

    public static readonly Error StudentNotEnrolled = new("Report.StudentNotEnrolled", "Student was not enrolled in the selected semester.", ErrorType.Validation);

    public static readonly Error NoPublishedGrades = new("Report.NoPublishedGrades", "The selected semester has no published grades for this student.", ErrorType.Conflict);

    public static readonly Error JobNotFound = new("Report.JobNotFound", "Report job was not found.", ErrorType.NotFound);

    public static readonly Error NotCompleted = new("Report.NotCompleted", "Report is not completed yet.", ErrorType.Conflict);

    public static readonly Error Expired = new("Report.Expired", "Report download link is expired.", ErrorType.Conflict);

    public static readonly Error RequestNotFound = new("Report.RequestNotFound", "Report request was not found.", ErrorType.NotFound);
    public static readonly Error RequestExists = new("Report.RequestExists", "An open report request already exists for this student and semester.", ErrorType.Conflict);
    public static readonly Error ParentRequired = new("Report.ParentRequired", "Parent role is required to create a report request.", ErrorType.Forbidden);
    public static readonly Error AcademicAdminRequired = new("Report.AcademicAdminRequired", "AcademicAdmin role is required to review report requests.", ErrorType.Forbidden);
    public static readonly Error ReviewReasonRequired = new("Report.ReviewReasonRequired", "A rejection reason is required.", ErrorType.Validation);
    public static readonly Error InvalidRequestState = new("Report.InvalidRequestState", "Report request state does not allow this operation.", ErrorType.Conflict);
    public static readonly Error IdempotencyPayloadMismatch = new("Report.IdempotencyPayloadMismatch", "The idempotency key was already used with a different student or semester.", ErrorType.Conflict);
}
