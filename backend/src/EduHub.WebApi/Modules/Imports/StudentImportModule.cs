using Carter;
using EduHub.Application.Contracts.StudentImports;
using EduHub.Application.Interfaces.Authentication;
using EduHub.WebApi.Dtos.StudentImports;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using MediatR;

namespace EduHub.WebApi.Modules.StudentImports;

/// <summary>
/// Ghi chú: StudentImportModule đăng ký API tải template và import workbook học sinh-phụ huynh.
/// </summary>
public sealed class StudentImportModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes chỉ cho AcademicAdmin/SystemAdmin tải mẫu và thực hiện import Excel.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/imports/students").WithTags("Imports").RequireAuthorization(AuthPolicies.AcademicAdmin);
        group.MapGet("/template", DownloadTemplateAsync).WithName("DownloadStudentImportTemplate");
        group.MapPost("/", ImportAsync).DisableAntiforgery().WithName("ImportStudentsWorkbook");
    }

    private static async Task<IResult> DownloadTemplateAsync(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DownloadStudentImportTemplateQuery(), cancellationToken);
        return result.IsSuccess
            ? Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName)
            : result.ToHttpResult(_ => new { });
    }

    /// <summary>
    /// Ghi chú: ImportAsync đọc file multipart tối đa 10 MB rồi chuyển qua DTO mapping và command.
    /// </summary>
    private static async Task<IResult> ImportAsync(IFormFile file, ISender sender, CancellationToken cancellationToken)
    {
        if (file.Length is 0 or > 10 * 1024 * 1024) return Results.BadRequest(new { errorCode = "Import.FileInvalid" });
        await using var stream = new MemoryStream((int)file.Length);
        await file.CopyToAsync(stream, cancellationToken);
        var request = new ImportStudentsWorkbookRequest(file.FileName, stream.ToArray());
        return (await sender.Send(request.ToCommand(), cancellationToken)).ToHttpResult(ImportMappings.ToDto);
    }
}
