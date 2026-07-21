using Carter;
using EduHub.Application.Contracts.Profiles;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Profiles;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Profiles;

/// <summary>
/// Ghi chú: StudentProfileModule đăng ký API hồ sơ cá nhân, upload bằng chứng và học vụ duyệt yêu cầu.
/// </summary>
public sealed class StudentProfileModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes phân quyền Student tự gửi yêu cầu và AcademicAdmin xem, duyệt hoặc từ chối.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/student-profile").WithTags("Student Profile");
        group.MapGet("/me", GetMyProfileAsync).RequireAuthorization(AuthPolicies.Student).WithName("GetMyStudentProfile");
        group.MapPost("/evidence/upload-grant", CreateEvidenceUploadGrantAsync).RequireAuthorization(AuthPolicies.Student).WithName("CreateProfileEvidenceUploadGrant");
        group.MapPut("/evidence/local", StoreLocalEvidenceAsync).RequireAuthorization(AuthPolicies.Student).WithName("StoreLocalProfileEvidence");
        group.MapGet("/evidence/local", GetLocalEvidenceAsync).RequireAuthorization().WithName("GetLocalProfileEvidence");
        group.MapPost("/requests", CreateRequestAsync).RequireAuthorization(AuthPolicies.Student).WithName("CreateStudentProfileChangeRequest");
        group.MapGet("/requests/me", ListMyRequestsAsync).RequireAuthorization(AuthPolicies.Student).WithName("ListMyStudentProfileChangeRequests");
        group.MapGet("/requests", ListRequestsAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("ListStudentProfileChangeRequests");
        group.MapPut("/requests/{requestId:guid}/review", ReviewRequestAsync).RequireAuthorization(AuthPolicies.AcademicAdmin).WithName("ReviewStudentProfileChangeRequest");
    }

    private static async Task<IResult> GetMyProfileAsync(ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetMyStudentProfileQuery(), cancellationToken)).ToHttpResult(ProfileMappings.ToDto);

    private static async Task<IResult> CreateEvidenceUploadGrantAsync(CreateEvidenceUploadGrantRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(), cancellationToken)).ToHttpResult(ProfileMappings.ToDto);

    /// <summary>
    /// Ghi chú: StoreLocalEvidenceAsync đọc tối đa 5 MB từ body rồi chuyển dữ liệu qua mapping và command.
    /// </summary>
    private static async Task<IResult> StoreLocalEvidenceAsync(string objectKey, HttpRequest httpRequest, ISender sender, CancellationToken cancellationToken)
    {
        const int maxEvidenceBytes = 5 * 1024 * 1024;
        if (httpRequest.ContentLength is > maxEvidenceBytes)
        {
            return Results.BadRequest(new { errorCode = "Profile.EvidenceInvalid", message = "Evidence image must not exceed 5 MB." });
        }

        await using var stream = new MemoryStream();
        var buffer = new byte[64 * 1024];
        while (true)
        {
            var read = await httpRequest.Body.ReadAsync(buffer, cancellationToken);
            if (read == 0) break;
            if (stream.Length + read > maxEvidenceBytes)
            {
                return Results.BadRequest(new { errorCode = "Profile.EvidenceInvalid", message = "Evidence image must not exceed 5 MB." });
            }

            await stream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        var request = new StoreLocalEvidenceRequest(objectKey, httpRequest.ContentType ?? "application/octet-stream", stream.ToArray());
        return (await sender.Send(request.ToCommand(), cancellationToken)).ToHttpResult();
    }

    private static async Task<IResult> GetLocalEvidenceAsync([AsParameters] GetLocalEvidenceRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToQuery(), cancellationToken);
        return result.IsSuccess
            ? Results.File(result.Value.Content, result.Value.ContentType)
            : result.ToHttpResult(_ => new { });
    }

    private static async Task<IResult> CreateRequestAsync(CreateStudentProfileChangeRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result.ToCreatedHttpResult(response => $"/api/v1/student-profile/requests/{response.Id}", ProfileMappings.ToDto);
    }

    private static async Task<IResult> ListMyRequestsAsync(ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ListMyStudentProfileChangeRequestsQuery(), cancellationToken)).ToHttpResult(items => items.Select(ProfileMappings.ToDto).ToList());

    private static async Task<IResult> ListRequestsAsync([AsParameters] ListStudentProfileChangeRequestsRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken)).ToHttpResult(items => items.Select(ProfileMappings.ToDto).ToList());

    private static async Task<IResult> ReviewRequestAsync(Guid requestId, ReviewStudentProfileChangeRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(requestId), cancellationToken)).ToHttpResult(ProfileMappings.ToDto);
}
