using Carter;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Academics;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Academics;

/// <summary>
/// Ghi chú: AcademicYearsModule đăng ký các endpoint API cho năm học.
/// </summary>
public sealed class AcademicYearsModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký route HTTP cho năm học.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/academic-years").WithTags("Academic Years");

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("CreateAcademicYear");

        group.MapGet("/", ListAsync)
            .WithName("ListAcademicYears");
    }

    /// <summary>
    /// Ghi chú: CreateAsync nhận request tạo năm học từ API và gửi command tương ứng.
    /// </summary>
    private static async Task<IResult> CreateAsync(
        CreateAcademicYearRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);

        return result.ToCreatedHttpResult(response => $"/api/v1/academic-years/{response.Id}", AcademicMappings.ToDto);
    }

    /// <summary>
    /// Ghi chú: ListAsync đọc query string để trả danh sách năm học.
    /// </summary>
    private static async Task<IResult> ListAsync(
        [AsParameters] ListAcademicYearsRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken))
        .ToHttpResult(result => result.ToPagedResponse(AcademicMappings.ToDto));
}
