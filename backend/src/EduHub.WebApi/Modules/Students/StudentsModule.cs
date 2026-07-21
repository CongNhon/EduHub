using Carter;
using EduHub.Application.Contracts.Students;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.Students;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.Students;

/// <summary>
/// Ghi chú: StudentsModule đăng ký các endpoint API cho học sinh.
/// </summary>
public sealed class StudentsModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký route HTTP cho học sinh.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/students").WithTags("Students");

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("CreateStudent");

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetStudentById");

        group.MapGet("/{id:guid}/detail", GetDetailAsync)
            .WithName("GetStudentDetail");

        group.MapGet("/", ListAsync)
            .WithName("ListStudents");

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("UpdateStudent");

        group.MapPost("/{id:guid}/parents/{parentUserId:guid}", LinkParentAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("LinkParentStudent");

        group.MapDelete("/{id:guid}/parents/{parentUserId:guid}", UnlinkParentAsync)
            .RequireAuthorization(AuthPolicies.AcademicAdmin)
            .WithName("UnlinkParentStudent");

        group.MapPut("/{id:guid}/account", LinkStudentUserAsync)
            .RequireAuthorization(AuthPolicies.SystemAdmin)
            .WithName("LinkStudentUserAccount");

        app.MapGet("/api/v1/me/children", ListMyChildrenAsync)
            .RequireAuthorization(AuthPolicies.Parent)
            .WithTags("Students")
            .WithName("ListMyChildren");
    }

    /// <summary>
    /// Ghi chú: CreateAsync nhận request tạo học sinh từ API và gửi command tương ứng.
    /// </summary>
    private static async Task<IResult> CreateAsync(
        CreateStudentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);

        return result.ToCreatedHttpResult(response => $"/api/v1/students/{response.Id}", StudentMappings.ToDto);
    }

    /// <summary>
    /// Ghi chú: GetByIdAsync lấy chi tiết students theo id trên URL.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(
        [AsParameters] GetStudentByIdRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken)).ToHttpResult(StudentMappings.ToDto);

    /// <summary>
    /// Ghi chú: GetDetailAsync trả hồ sơ, lớp và phụ huynh của học sinh theo quyền truy cập.
    /// </summary>
    private static async Task<IResult> GetDetailAsync(Guid id, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetStudentDetailQuery(id), cancellationToken)).ToHttpResult(StudentMappings.ToDto);

    /// <summary>
    /// Ghi chú: ListMyChildrenAsync trả danh sách con active của phụ huynh đang đăng nhập.
    /// </summary>
    private static async Task<IResult> ListMyChildrenAsync(ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ListMyChildrenQuery(), cancellationToken)).ToHttpResult(items => items.Select(StudentMappings.ToDto).ToList());

    /// <summary>
    /// Ghi chú: LinkStudentUserAsync gắn tài khoản Student với hồ sơ học sinh do SystemAdmin chọn.
    /// </summary>
    private static async Task<IResult> LinkStudentUserAsync(Guid id, LinkStudentUserRequest request, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(id), cancellationToken)).ToHttpResult(StudentMappings.ToDto);

    /// <summary>
    /// Ghi chú: ListAsync đọc query string để trả danh sách học sinh.
    /// </summary>
    private static async Task<IResult> ListAsync(
        [AsParameters] ListStudentsRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToQuery(), cancellationToken))
        .ToHttpResult(result => result.ToPagedResponse(StudentMappings.ToDto));

    /// <summary>
    /// Ghi chú: UpdateAsync nhận request cập nhật học sinh và gửi command tương ứng.
    /// </summary>
    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateStudentRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(id), cancellationToken)).ToHttpResult(StudentMappings.ToDto);

    /// <summary>
    /// Ghi chú: LinkParentAsync gắn phụ huynh với học sinh theo studentId và parentUserId.
    /// </summary>
    private static async Task<IResult> LinkParentAsync(
        Guid id,
        Guid parentUserId,
        LinkParentStudentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(id, parentUserId), cancellationToken);

        return result.ToCreatedHttpResult(
            response => $"/api/v1/students/{response.StudentId}/parents/{response.ParentUserId}",
            StudentMappings.ToDto);
    }

    /// <summary>
    /// Ghi chú: UnlinkParentAsync deactivate liên kết phụ huynh-học sinh, không xóa lịch sử.
    /// </summary>
    private static async Task<IResult> UnlinkParentAsync(
        [AsParameters] UnlinkParentStudentRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(request.ToCommand(), cancellationToken)).ToHttpResult();
}
