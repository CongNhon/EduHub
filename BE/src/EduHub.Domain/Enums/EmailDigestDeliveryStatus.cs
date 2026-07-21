namespace EduHub.Domain.Enums;

/// <summary>
/// Ghi chu: EmailDigestDeliveryStatus mo ta trang thai gui mot email tong hop diem cho phu huynh.
/// </summary>
public enum EmailDigestDeliveryStatus
{
    Pending = 1,
    Sending = 2,
    Sent = 3,
    Failed = 4
}
