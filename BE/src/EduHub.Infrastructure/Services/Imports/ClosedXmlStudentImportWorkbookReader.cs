using System.Globalization;
using ClosedXML.Excel;
using EduHub.Application.Contracts.StudentImports;
using EduHub.Application.Interfaces.Services.StudentImports;

namespace EduHub.Infrastructure.Services.StudentImports;

/// <summary>
/// Ghi chú: ClosedXmlStudentImportWorkbookReader đọc workbook XLSX và tạo template import dễ dùng cho học vụ.
/// </summary>
public sealed class ClosedXmlStudentImportWorkbookReader : IStudentImportWorkbookReader
{
    private static readonly string[] Headers =
    [
        "StudentCode", "FullName", "DateOfBirth", "Gender", "Address", "StudentEmail",
        "ParentFullName", "ParentEmail", "ParentPhone", "Relationship", "ClassCode", "SemesterName"
    ];

    /// <summary>
    /// Ghi chú: Read kiểm tra header và chuyển từng dòng Excel thành StudentImportRow có số dòng gốc.
    /// </summary>
    public StudentImportWorkbookParseResult Read(byte[] content)
    {
        try
        {
            using var stream = new MemoryStream(content);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet is null) return new StudentImportWorkbookParseResult([], ["Workbook không có worksheet."]);
            var headerRow = worksheet.FirstRowUsed();
            if (headerRow is null) return new StudentImportWorkbookParseResult([], ["Worksheet không có header."]);

            var headerMap = headerRow.CellsUsed().ToDictionary(cell => cell.GetString().Trim(), cell => cell.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);
            var missing = Headers.Where(header => !headerMap.ContainsKey(header)).ToList();
            if (missing.Count > 0) return new StudentImportWorkbookParseResult([], [$"Thiếu cột: {string.Join(", ", missing)}."]);

            var rows = new List<StudentImportRow>();
            var errors = new List<string>();
            foreach (var row in worksheet.RowsUsed().Where(row => row.RowNumber() > headerRow.RowNumber()))
            {
                var studentCode = Cell(row, headerMap, "StudentCode");
                if (string.IsNullOrWhiteSpace(studentCode)) continue;
                var dateCell = row.Cell(headerMap["DateOfBirth"]);
                if (!TryDate(dateCell, out var dateOfBirth))
                {
                    errors.Add($"Dòng {row.RowNumber()}: DateOfBirth phải là ngày hợp lệ.");
                    continue;
                }

                var requiredValues = new Dictionary<string, string>
                {
                    ["FullName"] = Cell(row, headerMap, "FullName"),
                    ["StudentEmail"] = Cell(row, headerMap, "StudentEmail"),
                    ["ParentFullName"] = Cell(row, headerMap, "ParentFullName"),
                    ["ParentEmail"] = Cell(row, headerMap, "ParentEmail"),
                    ["Relationship"] = Cell(row, headerMap, "Relationship"),
                    ["ClassCode"] = Cell(row, headerMap, "ClassCode"),
                    ["SemesterName"] = Cell(row, headerMap, "SemesterName")
                };
                var emptyColumns = requiredValues.Where(pair => string.IsNullOrWhiteSpace(pair.Value)).Select(pair => pair.Key).ToList();
                if (emptyColumns.Count > 0)
                {
                    errors.Add($"Dòng {row.RowNumber()}: thiếu {string.Join(", ", emptyColumns)}.");
                    continue;
                }

                rows.Add(new StudentImportRow(
                    row.RowNumber(),
                    studentCode,
                    requiredValues["FullName"],
                    dateOfBirth,
                    NullIfEmpty(Cell(row, headerMap, "Gender")),
                    NullIfEmpty(Cell(row, headerMap, "Address")),
                    requiredValues["StudentEmail"],
                    requiredValues["ParentFullName"],
                    requiredValues["ParentEmail"],
                    NullIfEmpty(Cell(row, headerMap, "ParentPhone")),
                    requiredValues["Relationship"],
                    requiredValues["ClassCode"],
                    requiredValues["SemesterName"]));
            }

            if (rows.Count == 0 && errors.Count == 0) errors.Add("Workbook không có dòng dữ liệu học sinh.");
            return new StudentImportWorkbookParseResult(rows, errors);
        }
        catch (Exception exception) when (exception is InvalidDataException or FormatException or ArgumentException)
        {
            return new StudentImportWorkbookParseResult([], [$"Không đọc được workbook: {exception.Message}"]);
        }
    }

    /// <summary>
    /// Ghi chú: CreateTemplate tạo workbook mẫu có hai dòng ví dụ, màu header, filter và định dạng ngày.
    /// </summary>
    public byte[] CreateTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Students");
        for (var index = 0; index < Headers.Length; index++) worksheet.Cell(1, index + 1).Value = Headers[index];
        var samples = new[]
        {
            new[] { "STU10001", "Nguyễn Minh An", "20/05/2010", "Nam", "Quận 1, TP.HCM", "student10001@eduhub.local", "Nguyễn Văn Bình", "parent10001@eduhub.local", "0901000001", "Cha", "10A1", "HK1" },
            new[] { "STU10002", "Trần Gia Hân", "12/08/2010", "Nữ", "Quận 3, TP.HCM", "student10002@eduhub.local", "Trần Thu Hà", "parent10002@eduhub.local", "0901000002", "Mẹ", "10A1", "HK1" }
        };
        for (var row = 0; row < samples.Length; row++)
        {
            for (var column = 0; column < samples[row].Length; column++) worksheet.Cell(row + 2, column + 1).Value = samples[row][column];
        }

        var table = worksheet.Range(1, 1, samples.Length + 1, Headers.Length).CreateTable("StudentImport");
        table.Theme = XLTableTheme.TableStyleMedium2;
        worksheet.SheetView.FreezeRows(1);
        worksheet.Column(3).Style.DateFormat.Format = "dd/MM/yyyy";
        worksheet.Columns().AdjustToContents(12, 36);
        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Row(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        worksheet.Row(1).Height = 26;
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string Cell(IXLRow row, Dictionary<string, int> headerMap, string header) => row.Cell(headerMap[header]).GetString().Trim();

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

    /// <summary>
    /// Ghi chú: TryDate đọc ngày từ cell kiểu DateTime hoặc chuỗi dd/MM/yyyy, yyyy-MM-dd.
    /// </summary>
    private static bool TryDate(IXLCell cell, out DateOnly date)
    {
        if (cell.TryGetValue<DateTime>(out var dateTime))
        {
            date = DateOnly.FromDateTime(dateTime);
            return true;
        }

        var formats = new[] { "dd/MM/yyyy", "yyyy-MM-dd", "M/d/yyyy" };
        if (DateTime.TryParseExact(cell.GetString().Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
        {
            date = DateOnly.FromDateTime(dateTime);
            return true;
        }

        date = default;
        return false;
    }
}
