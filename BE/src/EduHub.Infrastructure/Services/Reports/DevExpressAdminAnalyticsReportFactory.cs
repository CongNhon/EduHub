using System.Drawing;
using System.Globalization;
using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using EduHub.Application.Contracts.Analytics;

namespace EduHub.Infrastructure.Services.Reports;

/// <summary>
/// Ghi chú: DevExpressAdminAnalyticsReportFactory dựng XtraReport System Analytics từ ba dataset quản trị.
/// </summary>
public static class DevExpressAdminAnalyticsReportFactory
{
    private const float ContentWidth = 740f;

    /// <summary>
    /// Ghi chú: Create tạo báo cáo nhiều phần gồm KPI, môn học, lớp học và lỗi chất lượng dữ liệu.
    /// </summary>
    public static XtraReport Create(AdminAnalyticsReportData data)
    {
        var report = new XtraReport
        {
            DisplayName = "EduHub - Báo cáo điều hành toàn trường",
            PaperKind = DXPaperKind.A4,
            Landscape = false,
            Margins = new DXMargins(40, 40, 48, 42),
            RequestParameters = false
        };
        var header = new ReportHeaderBand { HeightF = 112f };
        var brandLabel = CreateLabel("EDUHUB  ·  BÁO CÁO ĐIỀU HÀNH", 0, 0, ContentWidth, 36, 16, true, Color.White);
        brandLabel.BackColor = Color.FromArgb(25, 111, 91);
        brandLabel.Padding = new PaddingInfo(12, 12, 0, 0);
        header.Controls.Add(brandLabel);
        header.Controls.Add(CreateLabel(
            $"{data.Overview.Semester.AcademicYearName} · {data.Overview.Semester.Name}",
            0,
            46,
            ContentWidth,
            24,
            12,
            true,
            Color.FromArgb(31, 41, 55)));
        header.Controls.Add(CreateLabel(
            $"Tạo lúc {data.Overview.GeneratedAtUtc:dd/MM/yyyy HH:mm:ss} UTC · Chỉ sử dụng điểm đã công bố hoặc khóa",
            0,
            76,
            ContentWidth,
            18,
            8,
            false,
            Color.FromArgb(100, 116, 139)));

        var detail = new DetailBand();
        var y = 0f;
        y = AddSectionTitle(detail, y, "1. Tổng quan nhà trường");
        y = AddTable(detail, y,
            ["Chỉ số", "Giá trị", "Chỉ số", "Giá trị"],
            [
                ["Học sinh đang học", FormatInteger(data.Overview.ActiveStudents), "Giáo viên hoạt động", FormatInteger(data.Overview.ActiveTeachers)],
                ["Phụ huynh hoạt động", FormatInteger(data.Overview.ActiveParents), "Lớp đang mở", FormatInteger(data.Overview.ActiveClasses)],
                ["Môn học đang mở", FormatInteger(data.Overview.ActiveSubjects), "Yêu cầu báo cáo", FormatInteger(data.Overview.OpenReportRequests)],
                ["Outbox chờ gửi", FormatInteger(data.Overview.PendingOutboxMessages), "Đồng bộ thất bại", FormatInteger(data.Overview.FailedExternalSyncs)]
            ],
            [2.3, 1, 2.3, 1]);

        y = AddSectionTitle(detail, y + 14, "2. Kết quả học tập");
        y = AddTable(detail, y,
            ["Điểm trung bình", "Tỷ lệ đạt", "Điểm công bố", "Tổng bản ghi điểm"],
            [[FormatScore(data.Academic.AverageNormalizedScore), FormatPercentage(data.Academic.PassRatePercentage), FormatInteger(data.Academic.PublishedGradeCount), FormatInteger(data.Academic.TotalGradeCount)]],
            [1, 1, 1, 1]);
        y = AddTable(detail, y + 10,
            ["Môn học", "Điểm trung bình", "Tỷ lệ đạt", "Điểm công bố"],
            data.Academic.SubjectPerformance.Select(subject => new[]
            {
                $"{subject.SubjectCode} · {subject.SubjectName}",
                FormatScore(subject.AverageNormalizedScore),
                FormatPercentage(subject.PassRatePercentage),
                FormatInteger(subject.PublishedGradeCount)
            }).ToList(),
            [2.8, 1, 1, 1]);

        y = AddSectionTitle(detail, y + 14, "3. Kết quả theo lớp");
        y = AddTable(detail, y,
            ["Lớp", "Khối", "Điểm trung bình", "Tỷ lệ đạt", "Điểm công bố"],
            data.Academic.ClassPerformance.Select(classRoom => new[]
            {
                $"{classRoom.ClassCode} · {classRoom.ClassName}",
                FormatInteger(classRoom.GradeLevel),
                FormatScore(classRoom.AverageNormalizedScore),
                FormatPercentage(classRoom.PassRatePercentage),
                FormatInteger(classRoom.PublishedGradeCount)
            }).ToList(),
            [2.4, .8, 1, 1, 1]);

        var qualityRows = data.DataQuality.Issues
            .Where(issue => issue.Count > 0)
            .Select(issue => new[] { SeverityLabel(issue.Severity), issue.Title, FormatInteger(issue.Count) })
            .ToList();
        if (qualityRows.Count == 0)
        {
            qualityRows.Add(["Ổn định", "Không có vấn đề dữ liệu cần xử lý", "0"]);
        }

        y = AddSectionTitle(detail, y + 14, "4. Chất lượng dữ liệu");
        y = AddTable(detail, y,
            ["Mức độ", "Vấn đề", "Số bản ghi"],
            qualityRows,
            [1, 4, 1]);
        detail.HeightF = y + 24;

        var footer = new PageFooterBand { HeightF = 30f };
        footer.Controls.Add(CreateLabel("EduHub · Báo cáo quản trị nội bộ", 0, 6, ContentWidth / 2, 18, 8, false, Color.FromArgb(100, 116, 139)));
        footer.Controls.Add(new XRPageInfo
        {
            BoundsF = new RectangleF(ContentWidth / 2, 6, ContentWidth / 2, 18),
            PageInfo = PageInfo.NumberOfTotal,
            TextAlignment = TextAlignment.MiddleRight,
            ForeColor = Color.FromArgb(100, 116, 139)
        });

        report.Bands.AddRange(new Band[] { header, detail, footer });
        return report;
    }

    /// <summary>
    /// Ghi chú: AddSectionTitle thêm tiêu đề cho một phần dữ liệu trong báo cáo.
    /// </summary>
    private static float AddSectionTitle(DetailBand band, float y, string title)
    {
        var label = CreateLabel(title, 0, y, ContentWidth, 26, 11, true, Color.FromArgb(31, 83, 72));
        label.BackColor = Color.FromArgb(238, 247, 244);
        label.Padding = new PaddingInfo(8, 8, 0, 0);
        band.Controls.Add(label);
        return y + 30;
    }

    /// <summary>
    /// Ghi chú: AddTable thêm bảng có header và các dòng dữ liệu tĩnh vào XtraReport.
    /// </summary>
    private static float AddTable(
        DetailBand band,
        float y,
        IReadOnlyList<string> headers,
        List<string[]> rows,
        IReadOnlyList<double> weights)
    {
        var table = new XRTable
        {
            BoundsF = new RectangleF(0, y, ContentWidth, 25f * (rows.Count + 1)),
            Borders = BorderSide.All,
            BorderColor = Color.FromArgb(226, 232, 240),
            Font = new DXFont("Arial", 9f),
            KeepTogether = true
        };
        table.Rows.Add(CreateRow(headers, weights, true));
        foreach (var row in rows)
        {
            table.Rows.Add(CreateRow(row, weights, false));
        }

        band.Controls.Add(table);
        return y + table.HeightF;
    }

    /// <summary>
    /// Ghi chú: CreateRow tạo một dòng XRTable với trọng số cột ổn định.
    /// </summary>
    private static XRTableRow CreateRow(IReadOnlyList<string> values, IReadOnlyList<double> weights, bool header)
    {
        var row = new XRTableRow { HeightF = 25f };
        for (var index = 0; index < values.Count; index++)
        {
            row.Cells.Add(new XRTableCell
            {
                Text = values[index],
                Weight = weights[index],
                BackColor = header ? Color.FromArgb(236, 246, 243) : Color.White,
                ForeColor = header ? Color.FromArgb(22, 101, 84) : Color.FromArgb(51, 65, 85),
                TextAlignment = TextAlignment.MiddleLeft,
                Padding = new PaddingInfo(6, 6, 2, 2),
                Font = new DXFont("Arial", 9f, header ? DXFontStyle.Bold : DXFontStyle.Regular),
                KeepTogether = true
            });
        }

        return row;
    }

    /// <summary>
    /// Ghi chú: CreateLabel tạo XRLabel có vị trí, màu và cỡ chữ cụ thể.
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
    /// Ghi chú: FormatScore định dạng điểm chuẩn hóa của report với hai chữ số thập phân.
    /// </summary>
    private static string FormatScore(decimal? value) => value?.ToString("0.00", CultureInfo.InvariantCulture) ?? "Chưa có";

    /// <summary>
    /// Ghi chú: FormatInteger định dạng số nguyên ổn định giữa máy phát triển, Docker và CI.
    /// </summary>
    private static string FormatInteger(int value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Ghi chú: FormatPercentage định dạng tỷ lệ report theo phần trăm.
    /// </summary>
    private static string FormatPercentage(decimal? value) =>
        value.HasValue ? $"{value.Value.ToString("0.00", CultureInfo.InvariantCulture)}%" : "Chưa có";

    /// <summary>
    /// Ghi chú: SeverityLabel đổi mức độ kỹ thuật của vấn đề dữ liệu thành nhãn tiếng Việt trong báo cáo tổng hợp.
    /// </summary>
    private static string SeverityLabel(string severity) => severity.ToUpperInvariant() switch
    {
        "CRITICAL" => "Nghiêm trọng",
        "WARNING" => "Cảnh báo",
        "INFO" => "Thông tin",
        _ => "Khác"
    };
}
