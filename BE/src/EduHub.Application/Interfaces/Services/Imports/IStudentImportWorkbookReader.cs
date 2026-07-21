using EduHub.Application.Contracts.StudentImports;

namespace EduHub.Application.Interfaces.Services.StudentImports;

/// <summary>
/// Ghi chú: IStudentImportWorkbookReader là interface đọc và tạo workbook Excel học sinh-phụ huynh.
/// </summary>
public interface IStudentImportWorkbookReader
{
    StudentImportWorkbookParseResult Read(byte[] content);
    byte[] CreateTemplate();
}
