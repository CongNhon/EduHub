using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.StudentImports;

namespace EduHub.Application.Interfaces.Services.StudentImports;

/// <summary>
/// Ghi chú: IStudentImportService là interface nghiệp vụ import học sinh, tài khoản, phụ huynh và ghi danh lớp.
/// </summary>
public interface IStudentImportService
{
    Task<Result<StudentImportResponse>> ImportAsync(ImportStudentsWorkbookCommand request, CancellationToken cancellationToken);
    Task<Result<StudentImportTemplateResponse>> DownloadTemplateAsync(DownloadStudentImportTemplateQuery request, CancellationToken cancellationToken);
}
