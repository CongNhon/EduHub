using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Classes;
using EduHub.Application.Interfaces.Services.Classes;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Classes.Enrollments;

/// <summary>
/// Ghi chú: EnrollStudentCommandValidator kiểm tra dữ liệu đầu vào cho ghi danh một học sinh vào lớp.
/// </summary>
public sealed class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo validator cho ghi danh một học sinh vào lớp.
    /// </summary>
    public EnrollStudentCommandValidator()
    {
        RuleFor(command => command.ClassRoomId).NotEmpty();
        RuleFor(command => command.StudentId).NotEmpty();
        RuleFor(command => command.SemesterId).NotEmpty();
    }
}

/// <summary>
/// Ghi chú: EnrollStudentCommandHandler xử lý ghi danh một học sinh vào lớp và giữ sĩ số không vượt capacity.
/// </summary>
public sealed class EnrollStudentCommandHandler(IClassService classService)
    : IRequestHandler<EnrollStudentCommand, Result<EnrollmentResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command ghi danh một học sinh sang ClassService.
    /// </summary>
    public Task<Result<EnrollmentResponse>> Handle(EnrollStudentCommand request, CancellationToken cancellationToken) =>
        classService.EnrollStudentAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: BulkEnrollStudentsCommandValidator kiểm tra dữ liệu đầu vào cho ghi danh hàng loạt học sinh.
/// </summary>
public sealed class BulkEnrollStudentsCommandValidator : AbstractValidator<BulkEnrollStudentsCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo validator cho ghi danh hàng loạt học sinh vào lớp.
    /// </summary>
    public BulkEnrollStudentsCommandValidator()
    {
        RuleFor(command => command.ClassRoomId).NotEmpty();
        RuleFor(command => command.SemesterId).NotEmpty();
        RuleFor(command => command.StudentIds).NotEmpty();
        RuleForEach(command => command.StudentIds).NotEmpty();
    }
}

/// <summary>
/// Ghi chú: BulkEnrollStudentsCommandHandler xử lý ghi danh hàng loạt học sinh theo partial-success.
/// </summary>
public sealed class BulkEnrollStudentsCommandHandler(IClassService classService)
    : IRequestHandler<BulkEnrollStudentsCommand, Result<BulkEnrollmentResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command ghi danh hàng loạt sang ClassService.
    /// </summary>
    public Task<Result<BulkEnrollmentResponse>> Handle(BulkEnrollStudentsCommand request, CancellationToken cancellationToken) =>
        classService.BulkEnrollStudentsAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: TransferEnrollmentCommandValidator kiểm tra dữ liệu đầu vào cho chuyển lớp của học sinh.
/// </summary>
public sealed class TransferEnrollmentCommandValidator : AbstractValidator<TransferEnrollmentCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo validator cho chuyển học sinh giữa hai lớp.
    /// </summary>
    public TransferEnrollmentCommandValidator()
    {
        RuleFor(command => command.StudentId).NotEmpty();
        RuleFor(command => command.FromClassRoomId).NotEmpty();
        RuleFor(command => command.ToClassRoomId).NotEmpty();
        RuleFor(command => command.SemesterId).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(512);
        RuleFor(command => command.ToClassRoomId).NotEqual(command => command.FromClassRoomId);
    }
}

/// <summary>
/// Ghi chú: TransferEnrollmentCommandHandler xử lý chuyển học sinh sang lớp mới bằng một transaction.
/// </summary>
public sealed class TransferEnrollmentCommandHandler(IClassService classService)
    : IRequestHandler<TransferEnrollmentCommand, Result<EnrollmentResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command chuyển lớp sang ClassService.
    /// </summary>
    public Task<Result<EnrollmentResponse>> Handle(TransferEnrollmentCommand request, CancellationToken cancellationToken) =>
        classService.TransferEnrollmentAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: WithdrawEnrollmentCommandValidator kiểm tra dữ liệu đầu vào cho rút học sinh khỏi lớp.
/// </summary>
public sealed class WithdrawEnrollmentCommandValidator : AbstractValidator<WithdrawEnrollmentCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo validator cho rút học sinh khỏi lớp.
    /// </summary>
    public WithdrawEnrollmentCommandValidator()
    {
        RuleFor(command => command.StudentId).NotEmpty();
        RuleFor(command => command.ClassRoomId).NotEmpty();
        RuleFor(command => command.SemesterId).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(512);
    }
}

/// <summary>
/// Ghi chú: WithdrawEnrollmentCommandHandler xử lý rút học sinh khỏi lớp và giảm sĩ số active.
/// </summary>
public sealed class WithdrawEnrollmentCommandHandler(IClassService classService)
    : IRequestHandler<WithdrawEnrollmentCommand, Result>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command rút học sinh khỏi lớp sang ClassService.
    /// </summary>
    public Task<Result> Handle(WithdrawEnrollmentCommand request, CancellationToken cancellationToken) =>
        classService.WithdrawEnrollmentAsync(request, cancellationToken);
}
