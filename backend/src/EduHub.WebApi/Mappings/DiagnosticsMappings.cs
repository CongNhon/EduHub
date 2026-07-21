using EduHub.Application.Contracts.Diagnostics;
using EduHub.WebApi.Dtos.Diagnostics;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: DiagnosticsMappings chứa mapping giữa Diagnostics DTO của API và command/query/response của Application.
/// </summary>
public static class DiagnosticsMappings
{
    /// <summary>
    /// Ghi chú: ToCommand chuyển PipelineProbeRequest API thành RunPipelineProbeCommand application.
    /// </summary>
    public static RunPipelineProbeCommand ToCommand(this PipelineProbeRequest request) =>
        new(request.Name);

    /// <summary>
    /// Ghi chú: ToQuery chuyển PipelineProbeQueryRequest API thành GetPipelineProbeQuery application.
    /// </summary>
    public static GetPipelineProbeQuery ToQuery(this PipelineProbeQueryRequest request) =>
        new(request.Name);

    /// <summary>
    /// Ghi chú: ToDto chuyển PipelineProbeResponse application thành PipelineProbeDto API.
    /// </summary>
    public static PipelineProbeDto ToDto(this PipelineProbeResponse response) =>
        new(response.Name, response.RequestType, response.ProcessedAtUtc);

    /// <summary>
    /// Ghi chú: ToQuery tạo GetCacheMetricsQuery application cho endpoint đo cache.
    /// </summary>
    public static GetCacheMetricsQuery ToCacheMetricsQuery() => new();

    /// <summary>
    /// Ghi chú: ToDto chuyển CacheMetricsResponse application thành CacheMetricsDto API.
    /// </summary>
    public static CacheMetricsDto ToDto(this CacheMetricsResponse response) =>
        new(response.Hits, response.Misses, response.Failures);
}
