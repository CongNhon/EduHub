using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Reports;
using EduHub.Application.Interfaces.Services.Reports;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Reports.ReportCards;

/// <summary>
/// Ghi chú: RequestReportCardCommandValidator kiểm tra request tạo PDF bảng điểm.
/// </summary>
public sealed class RequestReportCardCommandValidator : AbstractValidator<RequestReportCardCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khai báo rule student/semester/idempotency key cho report request.
    /// </summary>
    public RequestReportCardCommandValidator()
    {
        RuleFor(command => command.StudentId).NotEmpty();
        RuleFor(command => command.SemesterId).NotEmpty();
        RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(128);
    }
}

/// <summary>
/// Ghi chú: RequestReportCardCommandHandler chuyển request PDF sang ReportService.
/// </summary>
public sealed class RequestReportCardCommandHandler(IReportService reportService)
    : IRequestHandler<RequestReportCardCommand, Result<ReportJobResponse>>
{
    /// <summary>
    /// Ghi chú: Handle tạo report job và enqueue worker sinh PDF.
    /// </summary>
    public Task<Result<ReportJobResponse>> Handle(RequestReportCardCommand request, CancellationToken cancellationToken) =>
        reportService.RequestReportCardAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetReportJobQueryValidator kiểm tra report job id trước khi đọc status.
/// </summary>
public sealed class GetReportJobQueryValidator : AbstractValidator<GetReportJobQuery>
{
    /// <summary>
    /// Ghi chú: Constructor khai báo rule bắt buộc cho report job id.
    /// </summary>
    public GetReportJobQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}

/// <summary>
/// Ghi chú: GetReportJobQueryHandler chuyển query status report sang ReportService.
/// </summary>
public sealed class GetReportJobQueryHandler(IReportService reportService)
    : IRequestHandler<GetReportJobQuery, Result<ReportJobResponse>>
{
    /// <summary>
    /// Ghi chú: Handle đọc trạng thái report job nếu user có quyền.
    /// </summary>
    public Task<Result<ReportJobResponse>> Handle(GetReportJobQuery request, CancellationToken cancellationToken) =>
        reportService.GetReportJobAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetReportDownloadQueryHandler chuyển query download PDF sang ReportService.
/// </summary>
public sealed class GetReportDownloadQueryHandler(IReportService reportService)
    : IRequestHandler<GetReportDownloadQuery, Result<ReportDownloadResponse>>
{
    /// <summary>
    /// Ghi chú: Handle tải file PDF nếu report completed/chưa expired và user có quyền.
    /// </summary>
    public Task<Result<ReportDownloadResponse>> Handle(
        GetReportDownloadQuery request,
        CancellationToken cancellationToken) =>
        reportService.GetReportDownloadAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetReportDownloadQueryValidator kiểm tra report job id trước khi tải PDF.
/// </summary>
public sealed class GetReportDownloadQueryValidator : AbstractValidator<GetReportDownloadQuery>
{
    /// <summary>
    /// Ghi chú: Constructor khai báo rule bắt buộc cho report job id khi download.
    /// </summary>
    public GetReportDownloadQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}

/// <summary>
/// Ghi chú: CreateReportRequestCommandValidator kiểm tra yêu cầu báo cáo phụ huynh trước khi gửi học vụ.
/// </summary>
public sealed class CreateReportRequestCommandValidator : AbstractValidator<CreateReportRequestCommand>
{
    public CreateReportRequestCommandValidator()
    {
        RuleFor(command => command.StudentId).NotEmpty();
        RuleFor(command => command.SemesterId).NotEmpty();
        RuleFor(command => command.Purpose).NotEmpty().MaximumLength(500);
    }
}

/// <summary>
/// Ghi chú: CreateReportRequestCommandHandler chuyển yêu cầu báo cáo sang ReportService.
/// </summary>
public sealed class CreateReportRequestCommandHandler(IReportService reportService) : IRequestHandler<CreateReportRequestCommand, Result<ReportRequestResponse>>
{
    public Task<Result<ReportRequestResponse>> Handle(CreateReportRequestCommand request, CancellationToken cancellationToken) => reportService.CreateReportRequestAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListReportRequestsQueryHandler đọc inbox hoặc lịch sử yêu cầu qua ReportService.
/// </summary>
public sealed class ListReportRequestsQueryHandler(IReportService reportService) : IRequestHandler<ListReportRequestsQuery, Result<PagedResult<ReportRequestResponse>>>
{
    public Task<Result<PagedResult<ReportRequestResponse>>> Handle(ListReportRequestsQuery request, CancellationToken cancellationToken) => reportService.ListReportRequestsAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ReviewReportRequestCommandHandler duyệt hoặc từ chối yêu cầu báo cáo qua ReportService.
/// </summary>
public sealed class ReviewReportRequestCommandHandler(IReportService reportService) : IRequestHandler<ReviewReportRequestCommand, Result<ReportRequestResponse>>
{
    public Task<Result<ReportRequestResponse>> Handle(ReviewReportRequestCommand request, CancellationToken cancellationToken) => reportService.ReviewReportRequestAsync(request, cancellationToken);
}
