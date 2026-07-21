namespace EduHub.Domain.Common;

/// <summary>
/// Ghi chú: AuditableEntity đại diện cho entity có CreatedAtUtc/UpdatedAtUtc trong hệ thống EduHub.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Ghi chú: MarkUpdated thực hiện phần xử lý của entity có CreatedAtUtc/UpdatedAtUtc.
    /// </summary>
    protected void MarkUpdated(DateTime updatedAtUtc)
    {
        UpdatedAtUtc = UtcDateTime.Require(updatedAtUtc, nameof(updatedAtUtc));
    }
}
