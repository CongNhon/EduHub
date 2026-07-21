using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Integrations;
using EduHub.Application.Interfaces.Services.Integrations;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Integrations.ExternalSync;

/// <summary>
/// Ghi chú: RetryGradeSyncCommandValidator kiểm tra assignment và reason khi admin retry Ministry sync.
/// </summary>
public sealed class RetryGradeSyncCommandValidator : AbstractValidator<RetryGradeSyncCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khai báo rule bắt buộc cho assignment id và reason manual retry.
    /// </summary>
    public RetryGradeSyncCommandValidator()
    {
        RuleFor(command => command.AssignmentId).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(512);
    }
}

/// <summary>
/// Ghi chú: RetryGradeSyncCommandHandler chuyển retry command sang ExternalSyncService.
/// </summary>
public sealed class RetryGradeSyncCommandHandler(IExternalSyncService externalSyncService)
    : IRequestHandler<RetryGradeSyncCommand, Result<ExternalSyncRecordResponse>>
{
    /// <summary>
    /// Ghi chú: Handle retry sync Ministry cho assignment đã có sync record.
    /// </summary>
    public Task<Result<ExternalSyncRecordResponse>> Handle(
        RetryGradeSyncCommand request,
        CancellationToken cancellationToken) =>
        externalSyncService.RetryGradeSyncAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetExternalSyncRecordQueryValidator kiểm tra sync record id.
/// </summary>
public sealed class GetExternalSyncRecordQueryValidator : AbstractValidator<GetExternalSyncRecordQuery>
{
    /// <summary>
    /// Ghi chú: Constructor khai báo rule bắt buộc cho sync record id.
    /// </summary>
    public GetExternalSyncRecordQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}

/// <summary>
/// Ghi chú: GetExternalSyncRecordQueryHandler chuyển query status sync sang ExternalSyncService.
/// </summary>
public sealed class GetExternalSyncRecordQueryHandler(IExternalSyncService externalSyncService)
    : IRequestHandler<GetExternalSyncRecordQuery, Result<ExternalSyncRecordResponse>>
{
    /// <summary>
    /// Ghi chú: Handle đọc trạng thái sync record cho admin.
    /// </summary>
    public Task<Result<ExternalSyncRecordResponse>> Handle(
        GetExternalSyncRecordQuery request,
        CancellationToken cancellationToken) =>
        externalSyncService.GetSyncRecordAsync(request, cancellationToken);
}
