using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Diagnostics;
using EduHub.Application.Interfaces.Services.Caching;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Diagnostics.Pipeline;

/// <summary>
/// Ghi chú: RunPipelineProbeCommandValidator kiểm tra dữ liệu đầu vào cho kiểm tra pipeline command trước khi handler chạy.
/// </summary>
public sealed class RunPipelineProbeCommandValidator : AbstractValidator<RunPipelineProbeCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo kiểm tra pipeline command và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public RunPipelineProbeCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(64);
    }
}

/// <summary>
/// Ghi chú: RunPipelineProbeCommandHandler xử lý kiểm tra pipeline command, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class RunPipelineProbeCommandHandler
    : IRequestHandler<RunPipelineProbeCommand, Result<PipelineProbeResponse>>
{
    /// <summary>
    /// Ghi chú: Handle xử lý kiểm tra pipeline command, gọi database/service cần thiết và trả kết quả.
    /// </summary>
    public Task<Result<PipelineProbeResponse>> Handle(
        RunPipelineProbeCommand request,
        CancellationToken cancellationToken) =>
        Task.FromResult(Result.Success(new PipelineProbeResponse(request.Name, "command", DateTime.UtcNow)));
}

/// <summary>
/// Ghi chú: GetPipelineProbeQueryHandler xử lý kiểm tra pipeline query, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class GetPipelineProbeQueryHandler : IRequestHandler<GetPipelineProbeQuery, PipelineProbeResponse>
{
    /// <summary>
    /// Ghi chú: Handle xử lý kiểm tra pipeline query, gọi database/service cần thiết và trả kết quả.
    /// </summary>
    public Task<PipelineProbeResponse> Handle(GetPipelineProbeQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(new PipelineProbeResponse(request.Name, "query", DateTime.UtcNow));
}

/// <summary>
/// Ghi chú: GetCacheMetricsQueryHandler đọc số liệu cache hit/miss/failure từ ICacheService.
/// </summary>
public sealed class GetCacheMetricsQueryHandler(ICacheService cacheService)
    : IRequestHandler<GetCacheMetricsQuery, CacheMetricsResponse>
{
    /// <summary>
    /// Ghi chú: Handle trả số liệu cache hiện tại để kiểm tra Redis hit/miss/fallback.
    /// </summary>
    public Task<CacheMetricsResponse> Handle(GetCacheMetricsQuery request, CancellationToken cancellationToken)
    {
        var metrics = cacheService.GetMetrics();
        return Task.FromResult(new CacheMetricsResponse(metrics.Hits, metrics.Misses, metrics.Failures));
    }
}
