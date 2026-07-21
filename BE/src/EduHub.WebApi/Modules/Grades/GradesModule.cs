using Carter;
using EduHub.Application.Contracts.Grades;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Grades;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Grades;

/// <summary>
/// Ghi chú: GradesModule đăng ký endpoint API cho nhập điểm và state machine sổ điểm.
/// </summary>
public sealed class GradesModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký route HTTP cho sửa điểm, bulk, submit, publish, reopen và lock.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var grades = app.MapGroup("/api/v1/grades").WithTags("Grades");
        grades.MapPut("/", UpdateAsync)
            .RequireAuthorization(AuthPolicies.Teacher)
            .WithName("UpdateGrade");

        var assignments = app.MapGroup("/api/v1/assignments").WithTags("Grades");
        assignments.MapPut("/{assignmentId:guid}/grades/bulk", BulkUpdateAsync)
            .RequireAuthorization(AuthPolicies.Teacher)
            .WithName("BulkUpdateGrades");

        assignments.MapGet("/{assignmentId:guid}/gradebook", GetGradebookAsync)
            .WithName("GetGradebook");

        assignments.MapPut("/{assignmentId:guid}/students/{studentId:guid}/remark", UpdateRemarkAsync)
            .RequireAuthorization(AuthPolicies.Teacher)
            .WithName("UpdateStudentRemark");

        assignments.MapPost("/{assignmentId:guid}/grades/submit", SubmitAsync)
            .RequireAuthorization(AuthPolicies.Teacher)
            .WithName("SubmitGradebook");

        assignments.MapPost("/{assignmentId:guid}/grades/publish", PublishAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("PublishGradebook");

        assignments.MapPost("/{assignmentId:guid}/grades/reopen", ReopenAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("ReopenGradebook");

        assignments.MapPost("/{assignmentId:guid}/grades/lock", LockAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("LockGradebook");

        assignments.MapGet("/{assignmentId:guid}/students/{studentId:guid}/grades/published", GetPublishedAsync)
            .RequireAuthorization(AuthPolicies.Parent)
            .WithName("GetPublishedGradesForParent");
    }

    private static async Task<IResult> UpdateAsync(
        UpdateGradeRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(), cancellationToken)).ToHttpResult(GradeMappings.ToDto);

    private static async Task<IResult> BulkUpdateAsync(
        Guid assignmentId,
        BulkUpdateGradesRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(assignmentId), cancellationToken)).ToHttpResult(GradeMappings.ToDto);

    /// <summary>
    /// Ghi chú: GetGradebookAsync trả context, roster, components, điểm và nhận xét trong một response.
    /// </summary>
    private static async Task<IResult> GetGradebookAsync(Guid assignmentId, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetGradebookQuery(assignmentId), cancellationToken)).ToHttpResult(GradeMappings.ToDto);

    /// <summary>
    /// Ghi chú: UpdateRemarkAsync lưu nhận xét môn học của giáo viên cho học sinh trong assignment.
    /// </summary>
    private static async Task<IResult> UpdateRemarkAsync(Guid assignmentId, Guid studentId, UpdateStudentRemarkRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(assignmentId, studentId), cancellationToken)).ToHttpResult(GradeMappings.ToDto);

    private static async Task<IResult> SubmitAsync(
        Guid assignmentId,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(GradeMappings.ToSubmitCommand(assignmentId), cancellationToken)).ToHttpResult(GradeMappings.ToDto);

    private static async Task<IResult> PublishAsync(
        Guid assignmentId,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(GradeMappings.ToPublishCommand(assignmentId), cancellationToken)).ToHttpResult(GradeMappings.ToDto);

    private static async Task<IResult> ReopenAsync(
        Guid assignmentId,
        ReopenGradebookRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(assignmentId), cancellationToken)).ToHttpResult(GradeMappings.ToDto);

    private static async Task<IResult> LockAsync(
        Guid assignmentId,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(GradeMappings.ToLockCommand(assignmentId), cancellationToken)).ToHttpResult(GradeMappings.ToDto);

    private static async Task<IResult> GetPublishedAsync(
        Guid assignmentId,
        Guid studentId,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublishedGradesRequest(studentId, assignmentId).ToQuery(), cancellationToken)).ToHttpResult(GradeMappings.ToDto);
}
