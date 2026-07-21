namespace EduHub.Application.Interfaces.Services.Reports;

/// <summary>
/// Ghi chú: IReportFileStorage lưu và đọc file PDF report qua storage key, không lộ local path ra API.
/// </summary>
public interface IReportFileStorage
{
    /// <summary>
    /// Ghi chú: SaveAsync lưu nội dung PDF và trả storage key nội bộ.
    /// </summary>
    Task<string> SaveAsync(Guid reportJobId, byte[] content, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: ReadAsync đọc nội dung PDF từ storage key nội bộ.
    /// </summary>
    Task<byte[]> ReadAsync(string storageKey, CancellationToken cancellationToken);
}
