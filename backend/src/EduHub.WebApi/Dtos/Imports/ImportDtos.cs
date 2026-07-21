namespace EduHub.WebApi.Dtos.StudentImports;

/// <summary>
/// Ghi chú: ImportStudentsWorkbookRequest chứa tên và byte file XLSX đã đọc từ multipart upload.
/// </summary>
public sealed record ImportStudentsWorkbookRequest(string FileName, byte[] Content);

/// <summary>
/// Ghi chú: StudentImportCredentialDto trả mật khẩu tạm một lần cho tài khoản vừa tạo.
/// </summary>
public sealed record StudentImportCredentialDto(string Email, string Role, string TemporaryPassword);

/// <summary>
/// Ghi chú: StudentImportRowDto trả trạng thái xử lý của một dòng workbook.
/// </summary>
public sealed record StudentImportRowDto(int RowNumber, string StudentCode, bool Success, string? ErrorCode, string? ErrorMessage);

/// <summary>
/// Ghi chú: StudentImportDto tổng kết import, lỗi từng dòng và danh sách mật khẩu tạm.
/// </summary>
public sealed record StudentImportDto(int TotalRows, int SuccessCount, int ErrorCount, IReadOnlyList<StudentImportRowDto> Rows, IReadOnlyList<StudentImportCredentialDto> TemporaryCredentials);
