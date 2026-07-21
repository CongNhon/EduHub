using EduHub.Application.Contracts.Students;
using EduHub.WebApi.Dtos.Students;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: StudentMappings chứa mapping giữa Student DTO của API và command/query/response của Application.
/// </summary>
public static class StudentMappings
{
    /// <summary>
    /// Ghi chú: ToCommand chuyển CreateStudentRequest API thành CreateStudentCommand application.
    /// </summary>
    public static CreateStudentCommand ToCommand(this CreateStudentRequest request) =>
        new(request.StudentCode, request.FullName, request.DateOfBirth);

    /// <summary>
    /// Ghi chú: ToCommand chuyển UpdateStudentRequest API thành UpdateStudentCommand application.
    /// </summary>
    public static UpdateStudentCommand ToCommand(this UpdateStudentRequest request, Guid id) =>
        new(id, request.FullName, request.DateOfBirth, request.Status, request.Version);

    /// <summary>
    /// Ghi chú: ToQuery chuyển GetStudentByIdRequest API thành GetStudentByIdQuery application.
    /// </summary>
    public static GetStudentByIdQuery ToQuery(this GetStudentByIdRequest request) =>
        new(request.Id);

    /// <summary>
    /// Ghi chú: ToQuery chuyển ListStudentsRequest API thành ListStudentsQuery application.
    /// </summary>
    public static ListStudentsQuery ToQuery(this ListStudentsRequest request) =>
        new(request.Status, request.Page ?? 1, request.PageSize ?? 20, request.Search, request.ClassRoomId);

    /// <summary>
    /// Ghi chú: ToDto chuyển StudentResponse application thành StudentDto API.
    /// </summary>
    public static StudentDto ToDto(this StudentResponse response) =>
        new(
            response.Id,
            response.StudentCode,
            response.FullName,
            response.DateOfBirth,
            response.Status,
            response.Version,
            response.CurrentClassId,
            response.CurrentClassCode,
            response.CurrentClassName,
            response.GuardianCount,
            response.AccountEmail,
            response.AccountIsActive);

    /// <summary>
    /// Ghi chú: ToDto chuyển detail hồ sơ học sinh gồm lớp và phụ huynh sang DTO API.
    /// </summary>
    public static StudentDetailDto ToDto(this StudentDetailResponse response) =>
        new(response.Student.ToDto(), response.Enrollments.Select(ToDto).ToList(), response.Guardians.Select(ToDto).ToList());

    public static ChildSummaryDto ToDto(this ChildSummaryResponse response) =>
        new(response.Id, response.StudentCode, response.FullName, response.DateOfBirth, response.Relationship, response.CurrentClassId, response.CurrentClassCode, response.CurrentClassName, response.CurrentSemesterId, response.CurrentSemesterName);

    public static LinkStudentUserCommand ToCommand(this LinkStudentUserRequest request, Guid studentId) => new(studentId, request.UserId);

    private static StudentEnrollmentSummaryDto ToDto(StudentEnrollmentSummaryResponse response) =>
        new(response.Id, response.ClassRoomId, response.ClassCode, response.ClassName, response.SemesterId, response.SemesterName, response.Status, response.EnrolledAtUtc, response.EndedAtUtc);

    private static StudentGuardianDto ToDto(StudentGuardianResponse response) =>
        new(response.LinkId, response.ParentUserId, response.FullName, response.Email, response.PhoneNumber, response.Relationship, response.IsActive);

    /// <summary>
    /// Ghi chú: ToCommand chuyển LinkParentStudentRequest API thành LinkParentStudentCommand application.
    /// </summary>
    public static LinkParentStudentCommand ToCommand(
        this LinkParentStudentRequest request,
        Guid studentId,
        Guid parentUserId) =>
        new(studentId, parentUserId, request.Relationship);

    /// <summary>
    /// Ghi chú: ToCommand chuyển UnlinkParentStudentRequest API thành UnlinkParentStudentCommand application.
    /// </summary>
    public static UnlinkParentStudentCommand ToCommand(this UnlinkParentStudentRequest request) =>
        new(request.Id, request.ParentUserId);

    /// <summary>
    /// Ghi chú: ToDto chuyển ParentStudentResponse application thành ParentStudentDto API.
    /// </summary>
    public static ParentStudentDto ToDto(this ParentStudentResponse response) =>
        new(response.Id, response.StudentId, response.ParentUserId, response.Relationship, response.IsActive);
}
