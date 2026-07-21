using EduHub.Application.Common.Models;

namespace EduHub.Application.Features.StudentImports.Common;

/// <summary>
/// Ghi chú: ImportErrors tập trung lỗi file Excel, quyền học vụ và cấu trúc dữ liệu import.
/// </summary>
public static class ImportErrors
{
    public static readonly Error AdminRequired = new("Import.AdminRequired", "Academic administrator role is required.", ErrorType.Forbidden);
    public static readonly Error FileInvalid = new("Import.FileInvalid", "The workbook must be a valid XLSX file no larger than 10 MB.", ErrorType.Validation);
    public static readonly Error WorkbookInvalid = new("Import.WorkbookInvalid", "The workbook contains invalid headers or row values.", ErrorType.Validation);
}
