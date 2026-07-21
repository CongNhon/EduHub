using EduHub.Domain.Common;

namespace EduHub.Domain.Entities.Integration;

/// <summary>
/// Ghi chú: OutboxMessage lưu event nghiệp vụ sau commit để worker Phase 12/13 xử lý bất đồng bộ.
/// </summary>
public sealed class OutboxMessage : BaseEntity
{
    private OutboxMessage()
    {
    }

    /// <summary>
    /// Ghi chú: Constructor tạo outbox message cho sự kiện nghiệp vụ cần phát sau transaction.
    /// </summary>
    public OutboxMessage(string type, string payload, DateTime occurredAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        Type = type.Trim();
        Payload = payload;
        OccurredAtUtc = UtcDateTime.Require(occurredAtUtc, nameof(occurredAtUtc));
    }

    public string Type { get; private set; } = null!;

    public string Payload { get; private set; } = null!;

    public DateTime OccurredAtUtc { get; private set; }

    public DateTime? ProcessedAtUtc { get; private set; }

    public int RetryCount { get; private set; }

    /// <summary>
    /// Ghi chú: MarkProcessed đánh dấu outbox message đã được worker xử lý xong.
    /// </summary>
    public void MarkProcessed(DateTime processedAtUtc)
    {
        ProcessedAtUtc = UtcDateTime.Require(processedAtUtc, nameof(processedAtUtc));
    }

    /// <summary>
    /// Ghi chú: MarkFailed tăng số lần retry khi worker xử lý outbox message lỗi.
    /// </summary>
    public void MarkFailed()
    {
        RetryCount++;
    }
}
