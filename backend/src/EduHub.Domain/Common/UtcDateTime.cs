using EduHub.Domain.Exceptions;

namespace EduHub.Domain.Common;

internal static class UtcDateTime
{
    /// <summary>
    /// Ghi chú: Require kiểm tra DateTime phải là UTC trước khi lưu/xử lý.
    /// </summary>
    public static DateTime Require(DateTime value, string parameterName)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : throw new DomainException("timestamp.not_utc", $"{parameterName} must be UTC.");
    }
}
