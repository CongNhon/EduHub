namespace EduHub.Application.Common.Caching;

/// <summary>
/// Ghi chú: CacheEntryOptions mô tả thời gian sống của một dữ liệu được lưu trong cache.
/// </summary>
public sealed record CacheEntryOptions(TimeSpan AbsoluteExpirationRelativeToNow);
