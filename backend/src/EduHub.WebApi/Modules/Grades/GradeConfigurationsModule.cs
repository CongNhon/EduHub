using Carter;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Grades;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Grades;

/// <summary>
/// Ghi chú: GradeConfigurationsModule đăng ký endpoint API cho cấu hình thành phần điểm.
/// </summary>
public sealed class GradeConfigurationsModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký route HTTP cho cấu hình thành phần điểm.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/grade-configurations").WithTags("Grade Configurations");

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("CreateGradeConfiguration");

        group.MapGet("/", ListAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("ListGradeConfigurations");
    }

    /// <summary>
    /// Ghi chú: CreateAsync nhận DTO cấu hình điểm, map sang command và trả GradeConfigurationDto.
    /// </summary>
    private static async Task<IResult> CreateAsync(
        CreateGradeConfigurationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);

        return result.ToCreatedHttpResult(
            response => $"/api/v1/grade-configurations?subjectId={response.SubjectId}&semesterId={response.SemesterId}&isActive=true",
            GradeMappings.ToDto);
    }

    /// <summary>
    /// Ghi chú: ListAsync đọc query string để trả danh sách version cấu hình điểm.
    /// </summary>
    private static async Task<IResult> ListAsync(
        [AsParameters] ListGradeConfigurationsRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken))
        .ToHttpResult(result => result.ToPagedResponse(GradeMappings.ToDto));
}
