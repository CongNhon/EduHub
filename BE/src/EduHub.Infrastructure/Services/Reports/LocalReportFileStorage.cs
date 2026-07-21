using EduHub.Application.Interfaces.Services.Reports;
using Microsoft.Extensions.Configuration;

namespace EduHub.Infrastructure.Services.Reports;

/// <summary>
/// Ghi chú: LocalReportFileStorage lưu PDF report vào thư mục local nội bộ của API.
/// </summary>
public sealed class LocalReportFileStorage(IConfiguration configuration) : IReportFileStorage
{
    private readonly string rootPath = configuration["Reports:StoragePath"] ??
        Path.Combine(AppContext.BaseDirectory, "reports");

    /// <summary>
    /// Ghi chú: SaveAsync lưu file PDF report và trả storage key tương đối.
    /// </summary>
    public async Task<string> SaveAsync(Guid reportJobId, byte[] content, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(rootPath);
        var storageKey = $"{reportJobId:N}.pdf";
        var fullPath = GetFullPath(storageKey);
        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);
        return storageKey;
    }

    /// <summary>
    /// Ghi chú: ReadAsync đọc file PDF theo storage key, không nhận path tùy ý từ client.
    /// </summary>
    public Task<byte[]> ReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        var fullPath = GetFullPath(storageKey);
        return File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    /// <summary>
    /// Ghi chú: GetFullPath tạo đường dẫn local từ storage key nội bộ đã sanitize bằng file name.
    /// </summary>
    private string GetFullPath(string storageKey)
    {
        var fileName = Path.GetFileName(storageKey);
        return Path.Combine(rootPath, fileName);
    }
}
