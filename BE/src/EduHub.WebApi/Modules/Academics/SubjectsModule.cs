using Carter;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Academics;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Academics;

/// <summary>
/// Ghi chú: SubjectsModule đăng ký các endpoint API cho môn học.
/// </summary>
public sealed class SubjectsModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký route HTTP cho môn học.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/subjects").WithTags("Subjects");

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("CreateSubject");

        group.MapGet("/", ListAsync)
            .WithName("ListSubjects");

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("UpdateSubject");

        group.MapPut("/{id:guid}/disable", DisableAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("DisableSubject");
    }

    /// <summary>
    /// Ghi chú: CreateAsync nhận request tạo môn học từ API và gửi command tương ứng.
    /// </summary>
    private static async Task<IResult> CreateAsync(
        CreateSubjectRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);

        return result.ToCreatedHttpResult(response => $"/api/v1/subjects/{response.Id}", AcademicMappings.ToDto);
    }

    /// <summary>
    /// Ghi chú: ListAsync đọc query string để trả danh sách môn học.
    /// </summary>
    private static async Task<IResult> ListAsync(
        [AsParameters] ListSubjectsRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken))
        .ToHttpResult(result => result.ToPagedResponse(AcademicMappings.ToDto));

    /// <summary>
    /// Ghi chú: UpdateAsync nhận request cập nhật môn học và gửi command tương ứng.
    /// </summary>
    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateSubjectRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(id), cancellationToken)).ToHttpResult(AcademicMappings.ToDto);

    /// <summary>
    /// Ghi chú: DisableAsync vô hiệu hóa môn học nhưng giữ lịch sử dữ liệu.
    /// </summary>
    private static async Task<IResult> DisableAsync(
        [AsParameters] DisableSubjectRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(), cancellationToken)).ToHttpResult(AcademicMappings.ToDto);
}
