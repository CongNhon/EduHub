using EduHub.Application.Contracts.StudentImports;
using EduHub.WebApi.Dtos.StudentImports;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: ImportMappings chuyển file upload và kết quả Application sang DTO API import.
/// </summary>
public static class ImportMappings
{
    public static ImportStudentsWorkbookCommand ToCommand(this ImportStudentsWorkbookRequest request) => new(request.FileName, request.Content);

    public static StudentImportDto ToDto(StudentImportResponse response) =>
        new(
            response.TotalRows,
            response.SuccessCount,
            response.ErrorCount,
            response.Rows.Select(row => new StudentImportRowDto(row.RowNumber, row.StudentCode, row.Success, row.ErrorCode, row.ErrorMessage)).ToList(),
            response.TemporaryCredentials.Select(credential => new StudentImportCredentialDto(credential.Email, credential.Role, credential.TemporaryPassword)).ToList());
}
