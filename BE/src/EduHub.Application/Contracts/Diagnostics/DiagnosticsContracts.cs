using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;

namespace EduHub.Application.Contracts.Diagnostics;

/// <summary>
/// Ghi chú: PipelineProbeResponse là dữ liệu trả về cho endpoint kiểm tra MediatR pipeline.
/// </summary>
public sealed record PipelineProbeResponse(string Name, string RequestType, DateTime ProcessedAtUtc);

/// <summary>
/// Ghi chú: RunPipelineProbeCommand là command kiểm tra command pipeline.
/// </summary>
public sealed record RunPipelineProbeCommand(string Name) : ICommand<Result<PipelineProbeResponse>>;

/// <summary>
/// Ghi chú: GetPipelineProbeQuery là query kiểm tra query pipeline.
/// </summary>
public sealed record GetPipelineProbeQuery(string Name) : IQuery<PipelineProbeResponse>;

/// <summary>
/// Ghi chú: CacheMetricsResponse là dữ liệu số lần cache hit, miss và fallback failure.
/// </summary>
public sealed record CacheMetricsResponse(long Hits, long Misses, long Failures);

/// <summary>
/// Ghi chú: GetCacheMetricsQuery là query đọc số liệu cache hiện tại.
/// </summary>
public sealed record GetCacheMetricsQuery : IQuery<CacheMetricsResponse>;
