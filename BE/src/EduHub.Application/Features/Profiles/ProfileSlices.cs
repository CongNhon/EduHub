using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Profiles;
using EduHub.Application.Interfaces.Services.Profiles;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Profiles;

/// <summary>
/// Ghi chú: CreateEvidenceUploadGrantCommandValidator kiểm tra tên file, content type và kích thước ảnh bằng chứng.
/// </summary>
public sealed class CreateEvidenceUploadGrantCommandValidator : AbstractValidator<CreateEvidenceUploadGrantCommand>
{
    public CreateEvidenceUploadGrantCommandValidator()
    {
        RuleFor(command => command.FileName).NotEmpty().MaximumLength(255);
        RuleFor(command => command.ContentType).NotEmpty().MaximumLength(100);
        RuleFor(command => command.FileSize).InclusiveBetween(1, 5 * 1024 * 1024);
    }
}

/// <summary>
/// Ghi chú: CreateStudentProfileChangeRequestCommandValidator kiểm tra dữ liệu hồ sơ và lý do trước khi gửi duyệt.
/// </summary>
public sealed class CreateStudentProfileChangeRequestCommandValidator : AbstractValidator<CreateStudentProfileChangeRequestCommand>
{
    public CreateStudentProfileChangeRequestCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(256);
        RuleFor(command => command.DateOfBirth)
            .NotEmpty()
            .LessThanOrEqualTo(DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage("DateOfBirth must not be in the future.");
        RuleFor(command => command.Gender).MaximumLength(32);
        RuleFor(command => command.PhoneNumber).MaximumLength(32);
        RuleFor(command => command.Address).MaximumLength(500);
        RuleFor(command => command.Reason).NotEmpty().MinimumLength(10).MaximumLength(1000);
        RuleFor(command => command.EvidenceObjectKey).NotEmpty().MaximumLength(500);
    }
}

/// <summary>
/// Ghi chú: GetMyStudentProfileQueryHandler đọc hồ sơ cá nhân qua StudentProfileService.
/// </summary>
public sealed class GetMyStudentProfileQueryHandler(IStudentProfileService service)
    : IRequestHandler<GetMyStudentProfileQuery, Result<StudentSelfProfileResponse>>
{
    public Task<Result<StudentSelfProfileResponse>> Handle(GetMyStudentProfileQuery request, CancellationToken cancellationToken) => service.GetMyProfileAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: CreateEvidenceUploadGrantCommandHandler tạo URL upload bằng chứng qua StudentProfileService.
/// </summary>
public sealed class CreateEvidenceUploadGrantCommandHandler(IStudentProfileService service)
    : IRequestHandler<CreateEvidenceUploadGrantCommand, Result<EvidenceUploadGrantResponse>>
{
    public Task<Result<EvidenceUploadGrantResponse>> Handle(CreateEvidenceUploadGrantCommand request, CancellationToken cancellationToken) => service.CreateEvidenceUploadGrantAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: StoreLocalProfileEvidenceCommandHandler lưu ảnh fallback local qua StudentProfileService.
/// </summary>
public sealed class StoreLocalProfileEvidenceCommandHandler(IStudentProfileService service)
    : IRequestHandler<StoreLocalProfileEvidenceCommand, Result>
{
    public Task<Result> Handle(StoreLocalProfileEvidenceCommand request, CancellationToken cancellationToken) => service.StoreLocalEvidenceAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetLocalProfileEvidenceQueryHandler đọc ảnh fallback local sau khi service kiểm tra quyền.
/// </summary>
public sealed class GetLocalProfileEvidenceQueryHandler(IStudentProfileService service)
    : IRequestHandler<GetLocalProfileEvidenceQuery, Result<ProfileEvidenceContentResponse>>
{
    public Task<Result<ProfileEvidenceContentResponse>> Handle(GetLocalProfileEvidenceQuery request, CancellationToken cancellationToken) => service.GetLocalEvidenceAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: CreateStudentProfileChangeRequestCommandHandler gửi yêu cầu sửa hồ sơ qua StudentProfileService.
/// </summary>
public sealed class CreateStudentProfileChangeRequestCommandHandler(IStudentProfileService service)
    : IRequestHandler<CreateStudentProfileChangeRequestCommand, Result<StudentProfileChangeRequestResponse>>
{
    public Task<Result<StudentProfileChangeRequestResponse>> Handle(CreateStudentProfileChangeRequestCommand request, CancellationToken cancellationToken) => service.CreateRequestAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListMyStudentProfileChangeRequestsQueryHandler đọc lịch sử yêu cầu của học sinh hiện tại.
/// </summary>
public sealed class ListMyStudentProfileChangeRequestsQueryHandler(IStudentProfileService service)
    : IRequestHandler<ListMyStudentProfileChangeRequestsQuery, Result<IReadOnlyList<StudentProfileChangeRequestResponse>>>
{
    public Task<Result<IReadOnlyList<StudentProfileChangeRequestResponse>>> Handle(ListMyStudentProfileChangeRequestsQuery request, CancellationToken cancellationToken) => service.ListMyRequestsAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListStudentProfileChangeRequestsQueryHandler đọc queue yêu cầu cho quản trị học vụ.
/// </summary>
public sealed class ListStudentProfileChangeRequestsQueryHandler(IStudentProfileService service)
    : IRequestHandler<ListStudentProfileChangeRequestsQuery, Result<IReadOnlyList<StudentProfileChangeRequestResponse>>>
{
    public Task<Result<IReadOnlyList<StudentProfileChangeRequestResponse>>> Handle(ListStudentProfileChangeRequestsQuery request, CancellationToken cancellationToken) => service.ListRequestsAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ReviewStudentProfileChangeRequestCommandHandler duyệt hoặc từ chối yêu cầu qua StudentProfileService.
/// </summary>
public sealed class ReviewStudentProfileChangeRequestCommandHandler(IStudentProfileService service)
    : IRequestHandler<ReviewStudentProfileChangeRequestCommand, Result<StudentProfileChangeRequestResponse>>
{
    public Task<Result<StudentProfileChangeRequestResponse>> Handle(ReviewStudentProfileChangeRequestCommand request, CancellationToken cancellationToken) => service.ReviewRequestAsync(request, cancellationToken);
}
