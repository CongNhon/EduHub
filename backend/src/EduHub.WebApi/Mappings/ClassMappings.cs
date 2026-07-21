using EduHub.Application.Contracts.Classes;
using EduHub.WebApi.Dtos.Classes;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: ClassMappings chứa mapping giữa Class DTO của API và command/query/response của Application.
/// </summary>
public static class ClassMappings
{
    /// <summary>
    /// Ghi chú: ToCommand chuyển CreateClassRoomRequest API thành CreateClassRoomCommand application.
    /// </summary>
    public static CreateClassRoomCommand ToCommand(this CreateClassRoomRequest request) =>
        new(request.ClassCode, request.Name, request.AcademicYearId, request.GradeLevel, request.Capacity);

    /// <summary>
    /// Ghi chú: ToCommand chuyển UpdateClassRoomRequest API thành UpdateClassRoomCommand application.
    /// </summary>
    public static UpdateClassRoomCommand ToCommand(this UpdateClassRoomRequest request, Guid id) =>
        new(id, request.Name, request.GradeLevel, request.Capacity);

    /// <summary>
    /// Ghi chú: ToQuery chuyển ListClassRoomsRequest API thành ListClassRoomsQuery application.
    /// </summary>
    public static ListClassRoomsQuery ToQuery(this ListClassRoomsRequest request) =>
        new(request.AcademicYearId, request.Page ?? 1, request.PageSize ?? 20, request.Search);

    /// <summary>
    /// Ghi chú: ToDto chuyển ClassRoomResponse application thành ClassRoomDto API.
    /// </summary>
    public static ClassRoomDto ToDto(this ClassRoomResponse response) =>
        new(
            response.Id,
            response.ClassCode,
            response.Name,
            response.AcademicYearId,
            response.GradeLevel,
            response.Capacity,
            response.ActiveEnrollmentCount,
            response.IsActive);

    /// <summary>
    /// Ghi chú: ToCommand chuyển AssignTeacherRequest API thành AssignTeacherCommand application.
    /// </summary>
    public static AssignTeacherCommand ToCommand(this AssignTeacherRequest request, Guid classRoomId) =>
        new(classRoomId, request.SubjectId, request.TeacherId, request.SemesterId);

    /// <summary>
    /// Ghi chú: ToDto chuyển TeachingAssignmentResponse application thành TeachingAssignmentDto API.
    /// </summary>
    public static TeachingAssignmentDto ToDto(this TeachingAssignmentResponse response) =>
        new(response.Id, response.ClassRoomId, response.SubjectId, response.TeacherId, response.SemesterId, response.IsActive);

    public static TeachingAssignmentSummaryDto ToDto(this TeachingAssignmentSummaryResponse response) =>
        new(response.Id, response.ClassRoomId, response.ClassCode, response.ClassName, response.SubjectId, response.SubjectCode, response.SubjectName, response.SemesterId, response.SemesterName, response.TeacherId, response.TeacherName, response.StudentCount, response.GradebookStatus, response.IsActive);

    /// <summary>
    /// Ghi chú: ToQuery chuyển bộ lọc API phân công giáo viên sang query Application.
    /// </summary>
    public static ListTeachingAssignmentsQuery ToQuery(this ListTeachingAssignmentsRequest request) =>
        new(request.ClassRoomId, request.TeacherId, request.SemesterId);

    /// <summary>
    /// Ghi chú: ToCommand chuyển EnrollStudentRequest API thành EnrollStudentCommand application.
    /// </summary>
    public static EnrollStudentCommand ToCommand(this EnrollStudentRequest request, Guid classRoomId) =>
        new(classRoomId, request.StudentId, request.SemesterId);

    /// <summary>
    /// Ghi chú: ToCommand chuyển BulkEnrollStudentsRequest API thành BulkEnrollStudentsCommand application.
    /// </summary>
    public static BulkEnrollStudentsCommand ToCommand(this BulkEnrollStudentsRequest request, Guid classRoomId) =>
        new(classRoomId, request.SemesterId, request.StudentIds);

    /// <summary>
    /// Ghi chú: ToCommand chuyển TransferEnrollmentRequest API thành TransferEnrollmentCommand application.
    /// </summary>
    public static TransferEnrollmentCommand ToCommand(
        this TransferEnrollmentRequest request,
        Guid fromClassRoomId,
        Guid studentId) =>
        new(studentId, fromClassRoomId, request.ToClassRoomId, request.SemesterId, request.Reason);

    /// <summary>
    /// Ghi chú: ToCommand chuyển WithdrawEnrollmentRequest API thành WithdrawEnrollmentCommand application.
    /// </summary>
    public static WithdrawEnrollmentCommand ToCommand(
        this WithdrawEnrollmentRequest request,
        Guid classRoomId,
        Guid studentId) =>
        new(studentId, classRoomId, request.SemesterId, request.Reason);

    /// <summary>
    /// Ghi chú: ToDto chuyển EnrollmentResponse application thành EnrollmentDto API.
    /// </summary>
    public static EnrollmentDto ToDto(this EnrollmentResponse response) =>
        new(
            response.Id,
            response.StudentId,
            response.ClassRoomId,
            response.SemesterId,
            response.Status,
            response.EnrolledAtUtc,
            response.EndedAtUtc);

    /// <summary>
    /// Ghi chú: ToDto chuyển BulkEnrollmentResponse application thành BulkEnrollmentDto API.
    /// </summary>
    public static BulkEnrollmentDto ToDto(this BulkEnrollmentResponse response) =>
        new(response.Items.Select(ToDto).ToList(), response.SuccessCount, response.ErrorCount);

    /// <summary>
    /// Ghi chú: ToDto chuyển BulkEnrollmentItemResult application thành BulkEnrollmentItemDto API.
    /// </summary>
    private static BulkEnrollmentItemDto ToDto(BulkEnrollmentItemResult item) =>
        new(item.StudentId, item.Success, item.EnrollmentId, item.ErrorCode, item.ErrorMessage);
}
