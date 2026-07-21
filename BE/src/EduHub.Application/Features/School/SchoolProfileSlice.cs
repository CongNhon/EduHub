using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.School;
using EduHub.Application.Interfaces.Services.School;
using MediatR;

namespace EduHub.Application.Features.School;

/// <summary>
/// Ghi chú: GetSchoolProfileQueryHandler đọc nhận diện trường qua SchoolProfileService.
/// </summary>
public sealed class GetSchoolProfileQueryHandler(ISchoolProfileService service) : IRequestHandler<GetSchoolProfileQuery, Result<SchoolProfileResponse>>
{
    public Task<Result<SchoolProfileResponse>> Handle(GetSchoolProfileQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(service.GetProfile());
}
