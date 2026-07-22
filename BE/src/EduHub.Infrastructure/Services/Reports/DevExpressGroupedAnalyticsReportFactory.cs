using System.Drawing;
using System.Globalization;
using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using EduHub.Application.Contracts.Analytics;

namespace EduHub.Infrastructure.Services.Reports;

/// <summary>
/// Ghi chú: DevExpressGroupedAnalyticsReportFactory dựng hai báo cáo có GroupHeaderBand thật cho lớp học và lỗi dữ liệu.
/// </summary>
internal static class DevExpressGroupedAnalyticsReportFactory
{
    private const float ContentWidth = 740f;

    /// <summary>
    /// Ghi chú: CreateAcademicByGrade nhóm kết quả từng lớp theo khối 10, 11 và 12 của học kỳ đã chọn.
    /// </summary>
    public static XtraReport CreateAcademicByGrade(AdminAnalyticsReportData data)
    {
        var rows = data.Academic.ClassPerformance
            .OrderBy(item => item.GradeLevel)
            .ThenBy(item => item.ClassCode, StringComparer.OrdinalIgnoreCase)
            .GroupBy(item => item.GradeLevel)
            .SelectMany(group =>
            {
                var groupLabel = $"KHỐI {group.Key}  |  {group.Count()} lớp  |  {group.Sum(item => item.PublishedGradeCount)} điểm đã công bố";
                return group.Select(item => new AcademicByGradeRow(
                    group.Key,
                    groupLabel,
                    item.ClassCode,
                    item.ClassName,
                    FormatScore(item.AverageNormalizedScore),
                    FormatPercentage(item.PassRatePercentage),
                    FormatInteger(item.PublishedGradeCount)));
            })
            .ToList();

        if (rows.Count == 0)
        {
            rows.Add(new AcademicByGradeRow(99, "CHƯA CÓ DỮ LIỆU LỚP", "Chưa có", "Chưa có lớp phát sinh điểm công bố", "Chưa có", "Chưa có", "0"));
        }

        return CreateGroupedReport(
            "EduHub - Kết quả học tập theo khối",
            "KẾT QUẢ HỌC TẬP THEO KHỐI VÀ LỚP",
            $"{data.Academic.Semester.AcademicYearName} | {data.Academic.Semester.Name} | Chỉ gồm điểm đã công bố hoặc khóa",
            rows,
            nameof(AcademicByGradeRow.GroupOrder),
            nameof(AcademicByGradeRow.GroupLabel),
            [
                new ReportColumn("Mã lớp", nameof(AcademicByGradeRow.ClassCode), 1.1),
                new ReportColumn("Tên lớp", nameof(AcademicByGradeRow.ClassName), 2.4),
                new ReportColumn("Điểm TB", nameof(AcademicByGradeRow.AverageScore), 1),
                new ReportColumn("Tỷ lệ đạt", nameof(AcademicByGradeRow.PassRate), 1),
                new ReportColumn("Điểm công bố", nameof(AcademicByGradeRow.PublishedGrades), 1.2)
            ]);
    }

    /// <summary>
    /// Ghi chú: CreateDataQuality nhóm các lỗi dữ liệu học vụ theo Critical, Warning và các mức độ còn lại.
    /// </summary>
    public static XtraReport CreateDataQuality(AdminAnalyticsReportData data)
    {
        var rows = data.DataQuality.Issues
            .Where(item => item.Count > 0)
            .GroupBy(item => NormalizeSeverity(item.Severity), StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => SeverityOrder(group.Key))
            .SelectMany(group =>
            {
                var groupLabel = $"{SeverityLabel(group.Key)}  |  {group.Count()} loại vấn đề  |  {group.Sum(item => item.Count)} bản ghi ảnh hưởng";
                return group
                    .OrderByDescending(item => item.Count)
                    .ThenBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
                    .Select(item => new DataQualityRow(
                        SeverityOrder(group.Key),
                        groupLabel,
                        item.Code,
                        item.Title,
                        FormatInteger(item.Count)));
            })
            .ToList();

        if (rows.Count == 0)
        {
            rows.Add(new DataQualityRow(99, "KHÔNG CÓ PHÁT HIỆN", "Chưa có", "Không có lỗi chất lượng dữ liệu trong học kỳ", "0"));
        }

        return CreateGroupedReport(
            "EduHub - Chất lượng dữ liệu theo mức độ",
            "CHẤT LƯỢNG DỮ LIỆU THEO MỨC ĐỘ",
            $"{data.DataQuality.Semester.AcademicYearName} | {data.DataQuality.Semester.Name} | {data.DataQuality.TotalFindings} bản ghi cần xử lý",
            rows,
            nameof(DataQualityRow.GroupOrder),
            nameof(DataQualityRow.GroupLabel),
            [
                new ReportColumn("Mã kiểm tra", nameof(DataQualityRow.IssueCode), 1.3),
                new ReportColumn("Vấn đề dữ liệu", nameof(DataQualityRow.IssueTitle), 4.2),
                new ReportColumn("Ảnh hưởng", nameof(DataQualityRow.AffectedRecords), 1.1)
            ]);
    }

    /// <summary>
    /// Ghi chú: CreateGroupedReport tạo bố cục A4 dùng chung và cấu hình GroupHeaderBand theo field của từng đối tượng báo cáo.
    /// </summary>
    private static XtraReport CreateGroupedReport(
        string displayName,
        string title,
        string subtitle,
        object dataSource,
        string groupOrderField,
        string groupLabelField,
        IReadOnlyList<ReportColumn> columns)
    {
        var report = new XtraReport
        {
            DisplayName = displayName,
            PaperKind = DXPaperKind.A4,
            Landscape = false,
            Margins = new DXMargins(40, 40, 48, 42),
            RequestParameters = false,
            DataSource = dataSource
        };

        var reportHeader = new ReportHeaderBand { HeightF = 100f };
        var brand = CreateLabel("EDUHUB  |  DEVEXPRESS REPORTING", 0, 0, ContentWidth, 34, 15, true, Color.White);
        brand.BackColor = Color.FromArgb(25, 111, 91);
        brand.Padding = new PaddingInfo(12, 12, 0, 0);
        reportHeader.Controls.Add(brand);
        reportHeader.Controls.Add(CreateLabel(title, 0, 43, ContentWidth, 26, 13, true, Color.FromArgb(31, 41, 55)));
        reportHeader.Controls.Add(CreateLabel(subtitle, 0, 72, ContentWidth, 18, 8, false, Color.FromArgb(100, 116, 139)));

        var groupHeader = new GroupHeaderBand
        {
            HeightF = 62f,
            RepeatEveryPage = true,
            KeepTogether = true
        };
        groupHeader.GroupFields.Add(new GroupField(groupOrderField, XRColumnSortOrder.Ascending));

        var groupLabel = CreateLabel(string.Empty, 0, 4, ContentWidth, 28, 10, true, Color.FromArgb(22, 101, 84));
        groupLabel.BackColor = Color.FromArgb(232, 245, 241);
        groupLabel.Padding = new PaddingInfo(8, 8, 0, 0);
        groupLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", $"[{groupLabelField}]"));
        groupHeader.Controls.Add(groupLabel);
        groupHeader.Controls.Add(CreateTable(columns, true, 35f));

        var detail = new DetailBand { HeightF = 25f, KeepTogether = true };
        detail.Controls.Add(CreateTable(columns, false, 0));

        var footer = new PageFooterBand { HeightF = 28f };
        footer.Controls.Add(CreateLabel("EduHub | Báo cáo quản trị nội bộ", 0, 5, ContentWidth / 2, 18, 8, false, Color.FromArgb(100, 116, 139)));
        footer.Controls.Add(new XRPageInfo
        {
            BoundsF = new RectangleF(ContentWidth / 2, 5, ContentWidth / 2, 18),
            PageInfo = PageInfo.NumberOfTotal,
            TextAlignment = TextAlignment.MiddleRight,
            ForeColor = Color.FromArgb(100, 116, 139)
        });

        report.Bands.AddRange([reportHeader, groupHeader, detail, footer]);
        return report;
    }

    /// <summary>
    /// Ghi chú: CreateTable tạo header tĩnh hoặc detail row liên kết dữ liệu của đối tượng đang được DevExpress render.
    /// </summary>
    private static XRTable CreateTable(IReadOnlyList<ReportColumn> columns, bool header, float y)
    {
        var table = new XRTable
        {
            BoundsF = new RectangleF(0, y, ContentWidth, 25f),
            Borders = BorderSide.All,
            BorderColor = Color.FromArgb(226, 232, 240),
            Font = new DXFont("Arial", 8.5f),
            KeepTogether = true
        };
        var row = new XRTableRow { HeightF = 25f };

        foreach (var column in columns)
        {
            var cell = new XRTableCell
            {
                Text = header ? column.Caption : string.Empty,
                Weight = column.Weight,
                BackColor = header ? Color.FromArgb(241, 245, 249) : Color.White,
                ForeColor = header ? Color.FromArgb(51, 65, 85) : Color.FromArgb(31, 41, 55),
                Font = new DXFont("Arial", 8.5f, header ? DXFontStyle.Bold : DXFontStyle.Regular),
                Padding = new PaddingInfo(6, 6, 2, 2),
                TextAlignment = TextAlignment.MiddleLeft,
                CanGrow = false
            };

            if (!header)
            {
                cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", $"[{column.FieldName}]"));
            }

            row.Cells.Add(cell);
        }

        table.Rows.Add(row);
        return table;
    }

    /// <summary>
    /// Ghi chú: CreateLabel tạo nhãn văn bản dùng chung cho header, group và footer của hai báo cáo.
    /// </summary>
    private static XRLabel CreateLabel(string text, float x, float y, float width, float height, float fontSize, bool bold, Color color) =>
        new()
        {
            Text = text,
            BoundsF = new RectangleF(x, y, width, height),
            ForeColor = color,
            Font = new DXFont("Arial", fontSize, bold ? DXFontStyle.Bold : DXFontStyle.Regular),
            TextAlignment = TextAlignment.MiddleLeft
        };

    /// <summary>
    /// Ghi chú: NormalizeSeverity chuẩn hóa severity rỗng hoặc không đồng nhất trước khi chia group báo cáo dữ liệu.
    /// </summary>
    private static string NormalizeSeverity(string severity) =>
        string.IsNullOrWhiteSpace(severity) ? "OTHER" : severity.Trim().ToUpperInvariant();

    /// <summary>
    /// Ghi chú: SeverityOrder đặt Critical trước Warning rồi mới đến các mức độ dữ liệu còn lại.
    /// </summary>
    private static int SeverityOrder(string severity) => severity switch
    {
        "CRITICAL" => 0,
        "WARNING" => 1,
        "INFO" => 2,
        _ => 3
    };

    /// <summary>
    /// Ghi chú: SeverityLabel đổi mức độ kỹ thuật của vấn đề dữ liệu thành nhãn tiếng Việt trong tiêu đề nhóm báo cáo.
    /// </summary>
    private static string SeverityLabel(string severity) => severity switch
    {
        "CRITICAL" => "NGHIÊM TRỌNG",
        "WARNING" => "CẢNH BÁO",
        "INFO" => "THÔNG TIN",
        _ => "KHÁC"
    };

    /// <summary>
    /// Ghi chú: FormatScore định dạng điểm trung bình lớp theo thang 10 với hai chữ số thập phân.
    /// </summary>
    private static string FormatScore(decimal? value) => value?.ToString("0.00", CultureInfo.InvariantCulture) ?? "Chưa có";

    /// <summary>
    /// Ghi chú: FormatPercentage định dạng tỷ lệ đạt của lớp dưới dạng phần trăm.
    /// </summary>
    private static string FormatPercentage(decimal? value) =>
        value.HasValue ? $"{value.Value.ToString("0.00", CultureInfo.InvariantCulture)}%" : "Chưa có";

    /// <summary>
    /// Ghi chú: FormatInteger định dạng số lượng ổn định giữa local, Docker và CI.
    /// </summary>
    private static string FormatInteger(int value) => value.ToString(CultureInfo.InvariantCulture);

    private sealed record ReportColumn(string Caption, string FieldName, double Weight);
    private sealed record AcademicByGradeRow(int GroupOrder, string GroupLabel, string ClassCode, string ClassName, string AverageScore, string PassRate, string PublishedGrades);
    private sealed record DataQualityRow(int GroupOrder, string GroupLabel, string IssueCode, string IssueTitle, string AffectedRecords);
}
