using System.Drawing;
using System.Globalization;
using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using EduHub.Application.Contracts.Analytics;

namespace EduHub.Infrastructure.Services.Reports;

/// <summary>
/// Ghi chú: DevExpressAdvancedAnalyticsReportFactory dựng 3 mẫu báo cáo nâng cao: Phân bổ điểm, Xu hướng và Rủi ro học sinh.
/// </summary>
internal static class DevExpressAdvancedAnalyticsReportFactory
{
    private const float ContentWidth = 740f;

    public static XtraReport CreateScoreDistribution(AdminAnalyticsReportData data)
    {
        var report = CreateBaseReport("EduHub - Phân bổ điểm số");
        var metrics = data.AdvancedDistribution?.Overall;
        
        var detail = new DetailBand();
        var y = 0f;

        y = AddHeader(report, "PHÂN BỐ ĐIỂM SỐ", data.Overview.Semester);
        
        y = AddSectionTitle(detail, y, "1. Chỉ số thống kê mô tả");
        if (metrics != null)
        {
            y = AddTable(detail, y, 
                ["Chỉ số", "Giá trị", "Chỉ số", "Giá trị"],
                [
                    ["Mẫu điểm (Sample Size)", FormatInteger(metrics.SampleSize), "Điểm trung bình (Mean)", FormatScore(metrics.Mean)],
                    ["Trung vị (Median)", FormatScore(metrics.Median), "Độ lệch chuẩn (Std Dev)", FormatScore(metrics.StandardDeviation)],
                    ["Thấp nhất (Min)", FormatScore(metrics.Min), "Cao nhất (Max)", FormatScore(metrics.Max)],
                    ["Phương sai (Variance)", FormatScore(metrics.Variance), "Khoảng tứ phân vị (IQR)", FormatScore(metrics.InterquartileRange)],
                    ["Q1 (25th)", FormatScore(metrics.Q1), "Q3 (75th)", FormatScore(metrics.Q3)],
                    ["P10 (10th)", FormatScore(metrics.P10), "P90 (90th)", FormatScore(metrics.P90)]
                ],
                [1.5, 1, 1.5, 1]);
        }
        else
        {
            y = AddNoDataMessage(detail, y);
        }

        y = AddSectionTitle(detail, y + 14, "2. Phân nhóm kết quả");
        if (data.AdvancedDistribution?.Buckets != null)
        {
            y = AddTable(detail, y,
                ["Nhóm điểm", "Số lượng", "Tỷ lệ (%)"],
                data.AdvancedDistribution.Buckets.Select(b => new[] { b.Name, FormatInteger(b.Count), FormatPercentage(b.Percentage) }).ToList(),
                [2, 1, 1]);
        }

        y = AddSectionTitle(detail, y + 14, "3. Chi tiết theo nhóm");
        if (data.AdvancedDistribution?.Grouped != null)
        {
            y = AddTable(detail, y,
                ["Tên nhóm", "Mẫu", "TB", "Trung vị", "Độ lệch"],
                data.AdvancedDistribution.Grouped.Select(g => new[] { 
                    g.GroupName, 
                    FormatInteger(g.Metrics.SampleSize), 
                    FormatScore(g.Metrics.Mean), 
                    FormatScore(g.Metrics.Median), 
                    FormatScore(g.Metrics.StandardDeviation) 
                }).ToList(),
                [2.5, 0.8, 1, 1, 1]);
        }

        detail.HeightF = y;
        report.Bands.Add(detail);
        AddFooter(report);
        return report;
    }

    public static XtraReport CreateAcademicTrend(AdminAnalyticsReportData data)
    {
        var report = CreateBaseReport("EduHub - Xu hướng học tập");
        var detail = new DetailBand();
        var y = 0f;

        y = AddHeader(report, "XU HƯỚNG HỌC TẬP QUA CÁC HỌC KỲ", data.Overview.Semester);

        y = AddSectionTitle(detail, y, "Dữ liệu xu hướng học kỳ");
        if (data.AdvancedTrend?.Points != null && data.AdvancedTrend.Points.Count > 0)
        {
            y = AddTable(detail, y,
                ["Học kỳ", "Số HS", "Điểm TB", "Trung vị", "Tỷ lệ đạt"],
                data.AdvancedTrend.Points.Select(p => new[] { 
                    p.SemesterName, 
                    FormatInteger(p.StudentCount), 
                    FormatScore(p.Mean), 
                    FormatScore(p.Median), 
                    FormatPercentage(p.PassRate) 
                }).ToList(),
                [2, 0.8, 1, 1, 1]);
        }
        else
        {
            y = AddNoDataMessage(detail, y);
        }

        detail.HeightF = y;
        report.Bands.Add(detail);
        AddFooter(report);
        return report;
    }

    public static XtraReport CreateStudentRisk(AdminAnalyticsReportData data)
    {
        var report = CreateBaseReport("EduHub - Danh sách học sinh rủi ro");
        var detail = new DetailBand();
        var y = 0f;

        y = AddHeader(report, "CẢNH BÁO SỚM RỦI RO HỌC TẬP", data.Overview.Semester);

        y = AddSectionTitle(detail, y, "1. Tóm tắt mức độ rủi ro");
        if (data.AdvancedRisk?.Summary != null)
        {
            var s = data.AdvancedRisk.Summary;
            y = AddTable(detail, y,
                ["Mức độ", "Số lượng", "Mức độ", "Số lượng"],
                [
                    ["Nguy cơ cao (Critical)", FormatInteger(s.Critical), "Rủi ro (High)", FormatInteger(s.High)],
                    ["Trung bình (Medium)", FormatInteger(s.Medium), "Thấp (Low)", FormatInteger(s.Low)],
                    ["Tổng số học sinh", FormatInteger(s.Total), "", ""]
                ],
                [1.5, 1, 1.5, 1]);
        }

        y = AddSectionTitle(detail, y + 14, "2. Danh sách học sinh cần hỗ trợ");
        if (data.AdvancedRisk?.Items != null && data.AdvancedRisk.Items.Count > 0)
        {
            // Sort by risk level and score as per requirement
            var sortedItems = data.AdvancedRisk.Items
                .OrderBy(i => RiskLevelOrder(i.RiskLevel))
                .ThenByDescending(i => i.RiskScore)
                .ToList();

            y = AddTable(detail, y,
                ["Học sinh", "Lớp", "Điểm rủi ro", "Mức độ", "Lý do"],
                sortedItems.Select(i => new[] { 
                    $"{i.StudentCode} - {i.StudentName}", 
                    i.ClassCode, 
                    i.RiskScore.ToString("0", CultureInfo.InvariantCulture),
                    RiskLevelLabel(i.RiskLevel),
                    string.Join(", ", i.Reasons.Select(r => r.Message))
                }).ToList(),
                [2.2, 0.8, 0.8, 1, 2.5]);
        }
        else
        {
            y = AddNoDataMessage(detail, y);
        }

        detail.HeightF = y;
        report.Bands.Add(detail);
        AddFooter(report);
        return report;
    }

    private static XtraReport CreateBaseReport(string displayName)
    {
        return new XtraReport
        {
            DisplayName = displayName,
            PaperKind = DXPaperKind.A4,
            Landscape = false,
            Margins = new DXMargins(40, 40, 48, 42),
            RequestParameters = false
        };
    }

    private static float AddHeader(XtraReport report, string title, AnalyticsSemesterResponse semester)
    {
        var header = new ReportHeaderBand { HeightF = 110f };
        var brandLabel = CreateLabel("EDUHUB  ·  ADVANCED ANALYTICS", 0, 0, ContentWidth, 36, 16, true, Color.White);
        brandLabel.BackColor = Color.FromArgb(25, 111, 91);
        brandLabel.Padding = new PaddingInfo(12, 12, 0, 0);
        header.Controls.Add(brandLabel);
        
        header.Controls.Add(CreateLabel(title, 0, 46, ContentWidth, 24, 13, true, Color.FromArgb(31, 41, 55)));
        
        header.Controls.Add(CreateLabel(
            $"{semester.AcademicYearName} · {semester.Name} · Xuất lúc {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC",
            0, 76, ContentWidth, 18, 8, false, Color.FromArgb(100, 116, 139)));
            
        report.Bands.Add(header);
        return 0;
    }

    private static float AddSectionTitle(DetailBand band, float y, string title)
    {
        var label = CreateLabel(title, 0, y, ContentWidth, 26, 11, true, Color.FromArgb(31, 83, 72));
        label.BackColor = Color.FromArgb(238, 247, 244);
        label.Padding = new PaddingInfo(8, 8, 0, 0);
        band.Controls.Add(label);
        return y + 30;
    }

    private static float AddTable(DetailBand band, float y, string[] headers, List<string[]> rows, double[] weights)
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

    private static XRTableRow CreateRow(string[] values, double[] weights, bool header)
    {
        var row = new XRTableRow { HeightF = 25f };
        for (var i = 0; i < values.Length; i++)
        {
            row.Cells.Add(new XRTableCell
            {
                Text = values[i],
                Weight = weights[i],
                BackColor = header ? Color.FromArgb(236, 246, 243) : Color.White,
                ForeColor = header ? Color.FromArgb(22, 101, 84) : Color.FromArgb(51, 65, 85),
                TextAlignment = TextAlignment.MiddleLeft,
                Padding = new PaddingInfo(6, 6, 2, 2),
                Font = new DXFont("Arial", 9f, header ? DXFontStyle.Bold : DXFontStyle.Regular),
                KeepTogether = true,
                CanGrow = true,
                Multiline = true
            });
        }
        return row;
    }

    private static void AddFooter(XtraReport report)
    {
        var footer = new PageFooterBand { HeightF = 30f };
        footer.Controls.Add(CreateLabel("EduHub · Báo cáo phân tích chuyên sâu", 0, 6, ContentWidth / 2, 18, 8, false, Color.FromArgb(100, 116, 139)));
        footer.Controls.Add(new XRPageInfo
        {
            BoundsF = new RectangleF(ContentWidth / 2, 6, ContentWidth / 2, 18),
            PageInfo = PageInfo.NumberOfTotal,
            TextAlignment = TextAlignment.MiddleRight,
            ForeColor = Color.FromArgb(100, 116, 139)
        });
        report.Bands.Add(footer);
    }

    private static float AddNoDataMessage(DetailBand band, float y)
    {
        var label = CreateLabel("Không có dữ liệu cho phần này.", 0, y, ContentWidth, 25, 9, false, Color.Gray);
        label.TextAlignment = TextAlignment.MiddleCenter;
        band.Controls.Add(label);
        return y + 30;
    }

    private static XRLabel CreateLabel(string text, float x, float y, float width, float height, float fontSize, bool bold, Color color) =>
        new()
        {
            Text = text,
            BoundsF = new RectangleF(x, y, width, height),
            ForeColor = color,
            Font = new DXFont("Arial", fontSize, bold ? DXFontStyle.Bold : DXFontStyle.Regular),
            TextAlignment = TextAlignment.MiddleLeft
        };

    private static string FormatScore(decimal? value) => value?.ToString("0.00", CultureInfo.InvariantCulture) ?? "—";
    private static string FormatInteger(int value) => value.ToString(CultureInfo.InvariantCulture);
    private static string FormatPercentage(decimal? value) => value.HasValue ? $"{value.Value.ToString("0.1", CultureInfo.InvariantCulture)}%" : "—";

    private static int RiskLevelOrder(string level) => level?.ToUpperInvariant() switch
    {
        "CRITICAL" => 0,
        "HIGH" => 1,
        "MEDIUM" => 2,
        "LOW" => 3,
        _ => 4
    };

    private static string RiskLevelLabel(string level) => level?.ToUpperInvariant() switch
    {
        "CRITICAL" => "Nguy cơ cao",
        "HIGH" => "Rủi ro",
        "MEDIUM" => "Trung bình",
        "LOW" => "Thấp",
        _ => "Không xác định"
    };
}
