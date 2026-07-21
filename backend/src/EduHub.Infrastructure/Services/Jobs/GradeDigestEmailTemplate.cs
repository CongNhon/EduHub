using System.Net;

namespace EduHub.Infrastructure.Services.Jobs;

/// <summary>
/// Ghi chú: GradeDigestEmailTemplate tạo nội dung email HTML/text chứa danh sách điểm đã công bố của học sinh.
/// </summary>
public static class GradeDigestEmailTemplate
{
    /// <summary>
    /// Ghi chú: Render tạo subject, HTML body và text body cho email digest điểm theo kỳ gửi.
    /// </summary>
    public static GradeDigestEmailContent Render(
        string periodName,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        IReadOnlyList<GradeDigestEmailItem> items)
    {
        var subject = $"EduHub {periodName} grade digest";
        var textLines = items.Select(item =>
            $"- {item.StudentName}: {item.ComponentName} = {item.Score:0.##}/{item.MaxScore:0.##} ({item.Status})");
        var textBody = string.Join(
            Environment.NewLine,
            new[]
            {
                subject,
                $"Period UTC: {periodStartUtc:yyyy-MM-dd} -> {periodEndUtc:yyyy-MM-dd}",
                "Published grades:"
            }.Concat(textLines));

        var rows = string.Join(Environment.NewLine, items.Select(item => $"""
            <tr>
              <td>{Html(item.StudentName)}</td>
              <td>{Html(item.ComponentName)}</td>
              <td>{item.Score:0.##}/{item.MaxScore:0.##}</td>
              <td>{Html(item.Status)}</td>
              <td>{item.PublishedAtUtc:yyyy-MM-dd HH:mm} UTC</td>
            </tr>
            """));

        var htmlBody = $$"""
            <!doctype html>
            <html>
            <body style="font-family:Arial,sans-serif;color:#17202a">
              <h2 style="margin:0 0 12px">EduHub {{Html(periodName)}} grade digest</h2>
              <p style="margin:0 0 16px">Period UTC: {{periodStartUtc:yyyy-MM-dd}} -> {{periodEndUtc:yyyy-MM-dd}}</p>
              <table cellpadding="8" cellspacing="0" style="border-collapse:collapse;border:1px solid #d6dbdf">
                <thead>
                  <tr style="background:#f4f6f7">
                    <th align="left">Student</th>
                    <th align="left">Grade component</th>
                    <th align="left">Score</th>
                    <th align="left">Status</th>
                    <th align="left">Published at</th>
                  </tr>
                </thead>
                <tbody>
                  {{rows}}
                </tbody>
              </table>
            </body>
            </html>
            """;

        return new GradeDigestEmailContent(subject, htmlBody, textBody);
    }

    /// <summary>
    /// Ghi chú: Html encode chuỗi hiển thị trong email HTML để tránh chèn markup ngoài ý muốn.
    /// </summary>
    private static string Html(string value) => WebUtility.HtmlEncode(value);
}

/// <summary>
/// Ghi chú: GradeDigestEmailItem chứa một dòng điểm công bố dùng để render email phụ huynh.
/// </summary>
public sealed record GradeDigestEmailItem(
    string StudentName,
    string ComponentName,
    decimal Score,
    decimal MaxScore,
    string Status,
    DateTime PublishedAtUtc);

/// <summary>
/// Ghi chú: GradeDigestEmailContent chứa subject, HTML body và text body sau khi render email.
/// </summary>
public sealed record GradeDigestEmailContent(string Subject, string HtmlBody, string TextBody);
