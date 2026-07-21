namespace EduHub.WebApi.Dtos.Diagnostics;

/// <summary>
/// Ghi chú: PipelineProbeRequest là DTO request API dùng cho endpoint kiểm tra MediatR pipeline.
/// </summary>
public sealed record PipelineProbeRequest(string Name);

/// <summary>
/// Ghi chú: PipelineProbeQueryRequest là DTO request API dùng để kiểm tra MediatR query pipeline bằng query string name.
/// </summary>
public sealed record PipelineProbeQueryRequest(string Name);

/// <summary>
/// Ghi chú: PipelineProbeDto là DTO response API chứa kết quả kiểm tra MediatR pipeline.
/// </summary>
public sealed record PipelineProbeDto(string Name, string RequestType, DateTime ProcessedAtUtc);

/// <summary>
/// Ghi chú: CacheMetricsDto là DTO response API chứa số lần cache hit, miss và failure.
/// </summary>
public sealed record CacheMetricsDto(long Hits, long Misses, long Failures);
