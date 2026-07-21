namespace EduHub.Domain.Exceptions;

/// <summary>
/// Ghi chú: DomainException đại diện cho lỗi nghiệp vụ ở Domain trong hệ thống EduHub.
/// </summary>
public sealed class DomainException : Exception
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo lỗi nghiệp vụ ở Domain và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public DomainException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
