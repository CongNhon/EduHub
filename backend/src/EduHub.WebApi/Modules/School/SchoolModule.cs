using Carter;
using EduHub.Application.Contracts.School;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.School;

/// <summary>
/// Ghi chú: SchoolModule đăng ký API đọc nhận diện trường single-school cho người dùng đã đăng nhập.
/// </summary>
public sealed class SchoolModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapGet("/api/v1/school-profile", async (ISender sender, CancellationToken cancellationToken) =>
            (await sender.Send(new GetSchoolProfileQuery(), cancellationToken)).ToHttpResult(SchoolMappings.ToDto))
            .WithTags("School")
            .WithName("GetSchoolProfile");
}
