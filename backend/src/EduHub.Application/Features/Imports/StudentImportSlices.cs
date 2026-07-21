using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.StudentImports;
using EduHub.Application.Interfaces.Services.StudentImports;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.StudentImports;

/// <summary>
/// Ghi chú: ImportStudentsWorkbookCommandValidator kiểm tra tên và kích thước file Excel trước khi đọc workbook.
/// </summary>
public sealed class ImportStudentsWorkbookCommandValidator : AbstractValidator<ImportStudentsWorkbookCommand>
{
    public ImportStudentsWorkbookCommandValidator()
    {
        RuleFor(command => command.FileName).NotEmpty().Must(fileName => fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase));
        RuleFor(command => command.Content).NotEmpty().Must(content => content.Length <= 10 * 1024 * 1024);
    }
}

/// <summary>
/// Ghi chú: ImportStudentsWorkbookCommandHandler chuyển workbook sang StudentImportService.
/// </summary>
public sealed class ImportStudentsWorkbookCommandHandler(IStudentImportService service)
    : IRequestHandler<ImportStudentsWorkbookCommand, Result<StudentImportResponse>>
{
    public Task<Result<StudentImportResponse>> Handle(ImportStudentsWorkbookCommand request, CancellationToken cancellationToken) => service.ImportAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: DownloadStudentImportTemplateQueryHandler tạo file mẫu Excel qua StudentImportService.
/// </summary>
public sealed class DownloadStudentImportTemplateQueryHandler(IStudentImportService service)
    : IRequestHandler<DownloadStudentImportTemplateQuery, Result<StudentImportTemplateResponse>>
{
    public Task<Result<StudentImportTemplateResponse>> Handle(DownloadStudentImportTemplateQuery request, CancellationToken cancellationToken) => service.DownloadTemplateAsync(request, cancellationToken);
}
