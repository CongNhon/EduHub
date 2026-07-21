using Carter;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Academics;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Academics;

/// <summary>
/// Ghi chú: SemestersModule đăng ký các endpoint API cho học kỳ.
/// </summary>
public sealed class SemestersModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký route HTTP cho học kỳ.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/semesters").WithTags("Semesters");

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("CreateSemester");

        group.MapGet("/", ListAsync)
            .WithName("ListSemesters");
    }

    /// <summary>
    /// Ghi chú: CreateAsync nhận request tạo học kỳ từ API và gửi command tương ứng.
    /// </summary>
    private static async Task<IResult> CreateAsync(
        CreateSemesterRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);

        return result.ToCreatedHttpResult(response => $"/api/v1/semesters/{response.Id}", AcademicMappings.ToDto);
    }

    /// <summary>
    /// Ghi chú: ListAsync đọc query string để trả danh sách học kỳ.
    /// </summary>
    private static async Task<IResult> ListAsync(
        [AsParameters] ListSemestersRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken))
        .ToHttpResult(result => result.ToPagedResponse(AcademicMappings.ToDto));
}
