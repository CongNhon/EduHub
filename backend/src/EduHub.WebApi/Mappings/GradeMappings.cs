using EduHub.Application.Contracts.Grades;
using EduHub.WebApi.Dtos.Grades;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: GradeMappings chứa mapping giữa Grade DTO của API và command/query/response của Application.
/// </summary>
public static class GradeMappings
{
    /// <summary>
    /// Ghi chú: ToCommand chuyển CreateGradeConfigurationRequest API thành CreateGradeConfigurationCommand application.
    /// </summary>
    public static CreateGradeConfigurationCommand ToCommand(this CreateGradeConfigurationRequest request) =>
        new(
            request.SubjectId,
            request.SemesterId,
            request.Components.Select(ToContract).ToList());

    /// <summary>
    /// Ghi chú: ToQuery chuyển ListGradeConfigurationsRequest API thành ListGradeConfigurationsQuery application.
    /// </summary>
    public static ListGradeConfigurationsQuery ToQuery(this ListGradeConfigurationsRequest request) =>
        new(request.SubjectId, request.SemesterId, request.IsActive, request.Page ?? 1, request.PageSize ?? 20);

    /// <summary>
    /// Ghi chú: ToDto chuyển GradeConfigurationResponse application thành GradeConfigurationDto API.
    /// </summary>
    public static GradeConfigurationDto ToDto(this GradeConfigurationResponse response) =>
        new(
            response.SubjectId,
            response.SemesterId,
            response.Version,
            response.IsActive,
            response.TotalWeight,
            response.Components.Select(ToDto).ToList());

    private static CreateGradeComponentItem ToContract(CreateGradeComponentItemRequest request) =>
        new(
            request.Name,
            request.Weight,
            request.MaxScore,
            request.DisplayOrder,
            request.IsRequired,
            request.IncludeInGpa);

    private static GradeComponentDto ToDto(GradeComponentResponse response) =>
        new(
            response.Id,
            response.SubjectId,
            response.SemesterId,
            response.Name,
            response.Weight,
            response.MaxScore,
            response.DisplayOrder,
            response.IsRequired,
            response.IncludeInGpa,
            response.Version,
            response.IsActive);

    /// <summary>
    /// Ghi chú: ToCommand chuyển UpdateGradeRequest API thành UpdateGradeCommand application.
    /// </summary>
    public static UpdateGradeCommand ToCommand(this UpdateGradeRequest request) =>
        new(request.StudentId, request.AssignmentId, request.ComponentId, request.Score, request.Version, request.Reason);

    /// <summary>
    /// Ghi chú: ToCommand chuyển BulkUpdateGradesRequest API thành BulkUpdateGradesCommand application.
    /// </summary>
    public static BulkUpdateGradesCommand ToCommand(this BulkUpdateGradesRequest request, Guid assignmentId) =>
        new(assignmentId, request.Atomic, request.Items.Select(ToContract).ToList());

    /// <summary>
    /// Ghi chú: ToCommand chuyển assignment id route thành SubmitGradebookCommand application.
    /// </summary>
    public static SubmitGradebookCommand ToSubmitCommand(Guid assignmentId) => new(assignmentId);

    /// <summary>
    /// Ghi chú: ToCommand chuyển assignment id route thành PublishGradebookCommand application.
    /// </summary>
    public static PublishGradebookCommand ToPublishCommand(Guid assignmentId) => new(assignmentId);

    /// <summary>
    /// Ghi chú: ToCommand chuyển ReopenGradebookRequest API thành ReopenGradebookCommand application.
    /// </summary>
    public static ReopenGradebookCommand ToCommand(this ReopenGradebookRequest request, Guid assignmentId) =>
        new(assignmentId, request.Reason);

    /// <summary>
    /// Ghi chú: ToCommand chuyển assignment id route thành LockGradebookCommand application.
    /// </summary>
    public static LockGradebookCommand ToLockCommand(Guid assignmentId) => new(assignmentId);

    /// <summary>
    /// Ghi chú: ToDto chuyển GradeEntryResponse application thành GradeEntryDto API.
    /// </summary>
    public static GradeEntryDto ToDto(this GradeEntryResponse response) =>
        new(
            response.Id,
            response.StudentId,
            response.AssignmentId,
            response.ComponentId,
            response.Score,
            response.Status,
            response.Version,
            response.PublicationVersion);

    /// <summary>
    /// Ghi chú: ToDto chuyển BulkUpdateGradesResponse application thành BulkUpdateGradesDto API.
    /// </summary>
    public static BulkUpdateGradesDto ToDto(this BulkUpdateGradesResponse response) =>
        new(response.Items.Select(ToDto).ToList(), response.SuccessCount, response.ErrorCount);

    /// <summary>
    /// Ghi chú: ToDto chuyển GradebookStateResponse application thành GradebookStateDto API.
    /// </summary>
    public static GradebookStateDto ToDto(this GradebookStateResponse response) =>
        new(response.AssignmentId, response.Status, response.AffectedCount, response.PublicationVersion);

    /// <summary>
    /// Ghi chú: ToQuery chuyển GetPublishedGradesRequest API thành GetPublishedGradesForParentQuery application.
    /// </summary>
    public static GetPublishedGradesForParentQuery ToQuery(this GetPublishedGradesRequest request) =>
        new(request.StudentId, request.AssignmentId);

    /// <summary>
    /// Ghi chú: ToDto chuyển PublishedGradebookResponse application thành PublishedGradebookDto API.
    /// </summary>
    public static PublishedGradebookDto ToDto(this PublishedGradebookResponse response) =>
        new(response.StudentId, response.StudentCode, response.StudentName, response.AssignmentId, response.ClassCode, response.ClassName, response.SubjectCode, response.SubjectName, response.SemesterName, response.TeacherName, response.PublishedAtUtc, response.Remark, response.Grades.Select(ToDto).ToList());

    public static GradebookDto ToDto(this GradebookResponse response) =>
        new(response.AssignmentId, response.ClassRoomId, response.ClassCode, response.ClassName, response.SubjectId, response.SubjectCode, response.SubjectName, response.SemesterId, response.SemesterName, response.TeacherId, response.TeacherName, response.Status, response.Components.Select(component => new GradebookComponentDto(component.Id, component.Name, component.Weight, component.MaxScore, component.DisplayOrder, component.IsRequired)).ToList(), response.Students.Select(student => new GradebookStudentDto(student.StudentId, student.StudentCode, student.FullName, student.Remark, student.RemarkVersion, student.Grades.Select(grade => new GradebookCellDto(grade.ComponentId, grade.Score, grade.Status, grade.Version, grade.PublicationVersion)).ToList())).ToList());

    public static UpdateStudentRemarkCommand ToCommand(this UpdateStudentRemarkRequest request, Guid assignmentId, Guid studentId) =>
        new(assignmentId, studentId, request.Content, request.Version);

    public static StudentRemarkDto ToDto(this StudentRemarkResponse response) =>
        new(response.Id, response.AssignmentId, response.StudentId, response.Content, response.Version, response.IsPublished);

    private static BulkUpdateGradeItem ToContract(BulkUpdateGradeItemRequest request) =>
        new(request.StudentId, request.ComponentId, request.Score, request.Version, request.Reason);

    private static BulkUpdateGradeItemDto ToDto(BulkUpdateGradeItemResponse response) =>
        new(
            response.StudentId,
            response.ComponentId,
            response.Success,
            response.Grade?.ToDto(),
            response.ErrorCode,
            response.ErrorMessage);

    private static PublishedGradeEntryDto ToDto(PublishedGradeEntryResponse response) =>
        new(response.ComponentId, response.ComponentName, response.Weight, response.MaxScore, response.Score, response.PublicationVersion);
}
