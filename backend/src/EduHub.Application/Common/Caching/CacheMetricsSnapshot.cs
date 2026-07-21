namespace EduHub.Application.Common.Caching;

/// <summary>
/// Ghi chú: CacheMetricsSnapshot chứa số lần cache hit, miss và lỗi fallback về database.
/// </summary>
public sealed record CacheMetricsSnapshot(long Hits, long Misses, long Failures);
