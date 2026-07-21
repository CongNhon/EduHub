using Carter;
using EduHub.Application.Contracts.Classes;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Classes;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Classes;

/// <summary>
/// Ghi chú: ClassesModule đăng ký các endpoint API cho lớp học, phân công giáo viên và ghi danh học sinh.
/// </summary>
public sealed class ClassesModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký route HTTP cho lớp học, phân công giáo viên và ghi danh học sinh.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/classes").WithTags("Classes");

        group.MapPost("/", CreateClassRoomAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("CreateClassRoom");

        group.MapGet("/", ListClassRoomsAsync)
            .WithName("ListClassRooms");

        group.MapPut("/{id:guid}", UpdateClassRoomAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("UpdateClassRoom");

        group.MapPost("/{classRoomId:guid}/assignments", AssignTeacherAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("AssignTeacherToClassRoom");

        app.MapGet("/api/v1/me/teaching-assignments", ListMyTeachingAssignmentsAsync)
            .RequireAuthorization(AuthPolicies.Teacher)
            .WithTags("Classes")
            .WithName("ListMyTeachingAssignments");

        app.MapGet("/api/v1/teaching-assignments", ListTeachingAssignmentsAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithTags("Classes")
            .WithName("ListTeachingAssignments");

        group.MapPost("/{classRoomId:guid}/enrollments", EnrollStudentAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("EnrollStudentToClassRoom");

        group.MapPost("/{classRoomId:guid}/enrollments/bulk", BulkEnrollStudentsAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("BulkEnrollStudentsToClassRoom");

        group.MapPost("/{fromClassRoomId:guid}/enrollments/{studentId:guid}/transfer", TransferEnrollmentAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("TransferStudentEnrollment");

        group.MapPost("/{classRoomId:guid}/enrollments/{studentId:guid}/withdraw", WithdrawEnrollmentAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("WithdrawStudentEnrollment");
    }

    /// <summary>
    /// Ghi chú: CreateClassRoomAsync nhận DTO tạo lớp học, map sang command và trả ClassRoomDto.
    /// </summary>
    private static async Task<IResult> CreateClassRoomAsync(
        CreateClassRoomRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result.ToCreatedHttpResult(response => $"/api/v1/classes/{response.Id}", ClassMappings.ToDto);
    }

    /// <summary>
    /// Ghi chú: ListClassRoomsAsync nhận query string lớp học, gửi query và trả PagedResponse ClassRoomDto.
    /// </summary>
    private static async Task<IResult> ListClassRoomsAsync(
        [AsParameters] ListClassRoomsRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken)).ToHttpResult(result => result.ToPagedResponse(ClassMappings.ToDto));

    /// <summary>
    /// Ghi chú: UpdateClassRoomAsync nhận DTO cập nhật lớp học, map sang command và trả ClassRoomDto.
    /// </summary>
    private static async Task<IResult> UpdateClassRoomAsync(
        Guid id,
        UpdateClassRoomRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(id), cancellationToken)).ToHttpResult(ClassMappings.ToDto);

    /// <summary>
    /// Ghi chú: AssignTeacherAsync nhận DTO phân công giáo viên, map sang command và trả TeachingAssignmentDto.
    /// </summary>
    private static async Task<IResult> AssignTeacherAsync(
        Guid classRoomId,
        AssignTeacherRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(classRoomId), cancellationToken);
        return result.ToCreatedHttpResult(
            response => $"/api/v1/classes/{response.ClassRoomId}/assignments/{response.Id}",
            ClassMappings.ToDto);
    }

    /// <summary>
    /// Ghi chú: ListMyTeachingAssignmentsAsync trả các lớp-môn thuộc giáo viên đang đăng nhập.
    /// </summary>
    private static async Task<IResult> ListMyTeachingAssignmentsAsync(Guid? semesterId, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ListMyTeachingAssignmentsQuery(semesterId), cancellationToken)).ToHttpResult(items => items.Select(ClassMappings.ToDto).ToList());

    /// <summary>
    /// Ghi chú: ListTeachingAssignmentsAsync trả phân công giáo viên-lớp-môn để quản trị học vụ kiểm soát.
    /// </summary>
    private static async Task<IResult> ListTeachingAssignmentsAsync([AsParameters] ListTeachingAssignmentsRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken)).ToHttpResult(items => items.Select(ClassMappings.ToDto).ToList());

    /// <summary>
    /// Ghi chú: EnrollStudentAsync nhận DTO ghi danh học sinh, map sang command và trả EnrollmentDto.
    /// </summary>
    private static async Task<IResult> EnrollStudentAsync(
        Guid classRoomId,
        EnrollStudentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(classRoomId), cancellationToken);
        return result.ToCreatedHttpResult(
            response => $"/api/v1/classes/{response.ClassRoomId}/enrollments/{response.Id}",
            ClassMappings.ToDto);
    }

    /// <summary>
    /// Ghi chú: BulkEnrollStudentsAsync nhận DTO ghi danh hàng loạt và trả BulkEnrollmentDto.
    /// </summary>
    private static async Task<IResult> BulkEnrollStudentsAsync(
        Guid classRoomId,
        BulkEnrollStudentsRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(classRoomId), cancellationToken)).ToHttpResult(ClassMappings.ToDto);

    /// <summary>
    /// Ghi chú: TransferEnrollmentAsync nhận DTO chuyển lớp, map sang command và trả EnrollmentDto mới.
    /// </summary>
    private static async Task<IResult> TransferEnrollmentAsync(
        Guid fromClassRoomId,
        Guid studentId,
        TransferEnrollmentRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(fromClassRoomId, studentId), cancellationToken)).ToHttpResult(ClassMappings.ToDto);

    /// <summary>
    /// Ghi chú: WithdrawEnrollmentAsync nhận DTO rút học sinh, map sang command và trả 204 khi thành công.
    /// </summary>
    private static async Task<IResult> WithdrawEnrollmentAsync(
        Guid classRoomId,
        Guid studentId,
        WithdrawEnrollmentRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(classRoomId, studentId), cancellationToken)).ToHttpResult();
}
