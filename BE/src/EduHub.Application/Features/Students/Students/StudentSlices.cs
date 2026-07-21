using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Students;
using EduHub.Application.Interfaces.Services.Students;
using EduHub.Domain.Enums;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Students.Students;

/// <summary>
/// Ghi chú: CreateStudentCommandValidator kiểm tra dữ liệu đầu vào cho hồ sơ học sinh mới trước khi handler chạy.
/// </summary>
public sealed class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo hồ sơ học sinh mới và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public CreateStudentCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.StudentCode).NotEmpty().MaximumLength(64);
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(256);
        RuleFor(command => command.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage("DateOfBirth must not be in the future.");
    }
}

/// <summary>
/// Ghi chú: CreateStudentCommandHandler xử lý hồ sơ học sinh mới, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class CreateStudentCommandHandler(IStudentService studentService)
    : IRequestHandler<CreateStudentCommand, Result<StudentResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command tạo hồ sơ học sinh sang StudentService.
    /// </summary>
    public Task<Result<StudentResponse>> Handle(CreateStudentCommand request, CancellationToken cancellationToken) =>
        studentService.CreateStudentAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: UpdateStudentCommandValidator kiểm tra dữ liệu đầu vào cho hồ sơ học sinh hiện có trước khi handler chạy.
/// </summary>
public sealed class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo hồ sơ học sinh hiện có và kiểm tra dữ liệu bắt buộc ban đầu.
    /// </summary>
    public UpdateStudentCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(256);
        RuleFor(command => command.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage("DateOfBirth must not be in the future.");
        RuleFor(command => command.Version).GreaterThan(0);
    }
}

/// <summary>
/// Ghi chú: UpdateStudentCommandHandler xử lý hồ sơ học sinh hiện có, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class UpdateStudentCommandHandler(IStudentService studentService)
    : IRequestHandler<UpdateStudentCommand, Result<StudentResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command cập nhật hồ sơ học sinh sang StudentService.
    /// </summary>
    public Task<Result<StudentResponse>> Handle(UpdateStudentCommand request, CancellationToken cancellationToken) =>
        studentService.UpdateStudentAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetStudentByIdQueryHandler xử lý chi tiết học sinh theo id và quyền truy cập, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class GetStudentByIdQueryHandler(IStudentService studentService)
    : IRequestHandler<GetStudentByIdQuery, Result<StudentResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển query chi tiết học sinh sang StudentService.
    /// </summary>
    public Task<Result<StudentResponse>> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken) =>
        studentService.GetStudentByIdAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListStudentsQueryHandler xử lý danh sách học sinh theo phân trang/quyền truy cập, gọi database/service và trả Result/DTO.
/// </summary>
public sealed class ListStudentsQueryHandler(IStudentService studentService)
    : IRequestHandler<ListStudentsQuery, Result<PagedResult<StudentResponse>>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển query danh sách học sinh sang StudentService.
    /// </summary>
    public Task<Result<PagedResult<StudentResponse>>> Handle(
        ListStudentsQuery request,
        CancellationToken cancellationToken) =>
        studentService.ListStudentsAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetStudentDetailQueryHandler đọc detail hồ sơ, lớp và phụ huynh của học sinh qua StudentService.
/// </summary>
public sealed class GetStudentDetailQueryHandler(IStudentService studentService)
    : IRequestHandler<GetStudentDetailQuery, Result<StudentDetailResponse>>
{
    public Task<Result<StudentDetailResponse>> Handle(GetStudentDetailQuery request, CancellationToken cancellationToken) =>
        studentService.GetStudentDetailAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListMyChildrenQueryHandler đọc danh sách con của phụ huynh đang đăng nhập qua StudentService.
/// </summary>
public sealed class ListMyChildrenQueryHandler(IStudentService studentService)
    : IRequestHandler<ListMyChildrenQuery, Result<IReadOnlyList<ChildSummaryResponse>>>
{
    public Task<Result<IReadOnlyList<ChildSummaryResponse>>> Handle(ListMyChildrenQuery request, CancellationToken cancellationToken) =>
        studentService.ListMyChildrenAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: LinkStudentUserCommandHandler gắn tài khoản role Student vào hồ sơ học sinh qua StudentService.
/// </summary>
public sealed class LinkStudentUserCommandHandler(IStudentService studentService)
    : IRequestHandler<LinkStudentUserCommand, Result<StudentResponse>>
{
    public Task<Result<StudentResponse>> Handle(LinkStudentUserCommand request, CancellationToken cancellationToken) =>
        studentService.LinkStudentUserAsync(request, cancellationToken);
}
