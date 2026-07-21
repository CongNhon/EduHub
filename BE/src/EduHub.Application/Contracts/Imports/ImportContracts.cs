using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;

namespace EduHub.Application.Contracts.StudentImports;

/// <summary>
/// Ghi chú: StudentImportRow chứa dữ liệu một học sinh, tài khoản, phụ huynh và lớp đọc từ Excel.
/// </summary>
public sealed record StudentImportRow(
    int RowNumber,
    string StudentCode,
    string FullName,
    DateOnly DateOfBirth,
    string? Gender,
    string? Address,
    string StudentEmail,
    string ParentFullName,
    string ParentEmail,
    string? ParentPhone,
    string Relationship,
    string ClassCode,
    string SemesterName);

/// <summary>
/// Ghi chú: StudentImportWorkbookParseResult trả các dòng hợp lệ và lỗi cấu trúc/ô dữ liệu của workbook.
/// </summary>
public sealed record StudentImportWorkbookParseResult(IReadOnlyList<StudentImportRow> Rows, IReadOnlyList<string> Errors);

/// <summary>
/// Ghi chú: StudentImportCredentialResponse trả mật khẩu tạm một lần cho tài khoản mới được tạo từ Excel.
/// </summary>
public sealed record StudentImportCredentialResponse(string Email, string Role, string TemporaryPassword);

/// <summary>
/// Ghi chú: StudentImportRowResponse trả kết quả thành công hoặc lỗi của từng dòng Excel.
/// </summary>
public sealed record StudentImportRowResponse(int RowNumber, string StudentCode, bool Success, string? ErrorCode, string? ErrorMessage);

/// <summary>
/// Ghi chú: StudentImportResponse tổng kết số học sinh import và các tài khoản mới cần đổi mật khẩu.
/// </summary>
public sealed record StudentImportResponse(
    int TotalRows,
    int SuccessCount,
    int ErrorCount,
    IReadOnlyList<StudentImportRowResponse> Rows,
    IReadOnlyList<StudentImportCredentialResponse> TemporaryCredentials);

/// <summary>
/// Ghi chú: ImportStudentsWorkbookCommand import file Excel học sinh-phụ huynh do quản trị học vụ tải lên.
/// </summary>
public sealed record ImportStudentsWorkbookCommand(string FileName, byte[] Content)
    : ICommand<Result<StudentImportResponse>>;

/// <summary>
/// Ghi chú: StudentImportTemplateResponse trả file mẫu Excel và tên file tải xuống.
/// </summary>
public sealed record StudentImportTemplateResponse(string FileName, byte[] Content, string ContentType);

/// <summary>
/// Ghi chú: DownloadStudentImportTemplateQuery tạo file mẫu có header và dòng ví dụ đúng định dạng.
/// </summary>
public sealed record DownloadStudentImportTemplateQuery : IQuery<Result<StudentImportTemplateResponse>>;
