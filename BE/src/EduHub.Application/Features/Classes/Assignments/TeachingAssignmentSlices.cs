using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Classes;
using EduHub.Application.Interfaces.Services.Classes;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Classes.Assignments;

/// <summary>
/// Ghi chú: AssignTeacherCommandValidator kiểm tra dữ liệu đầu vào cho phân công giáo viên trước khi handler chạy.
/// </summary>
public sealed class AssignTeacherCommandValidator : AbstractValidator<AssignTeacherCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo validator cho phân công giáo viên.
    /// </summary>
    public AssignTeacherCommandValidator()
    {
        RuleFor(command => command.ClassRoomId).NotEmpty();
        RuleFor(command => command.SubjectId).NotEmpty();
        RuleFor(command => command.TeacherId).NotEmpty();
        RuleFor(command => command.SemesterId).NotEmpty();
    }
}

/// <summary>
/// Ghi chú: AssignTeacherCommandHandler xử lý phân công giáo viên và kiểm tra teacher/subject/class/semester hợp lệ.
/// </summary>
public sealed class AssignTeacherCommandHandler(IClassService classService)
    : IRequestHandler<AssignTeacherCommand, Result<TeachingAssignmentResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command phân công giáo viên sang ClassService.
    /// </summary>
    public Task<Result<TeachingAssignmentResponse>> Handle(AssignTeacherCommand request, CancellationToken cancellationToken) =>
        classService.AssignTeacherAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListMyTeachingAssignmentsQueryHandler đọc danh sách lớp-môn thuộc giáo viên hiện tại qua ClassService.
/// </summary>
public sealed class ListMyTeachingAssignmentsQueryHandler(IClassService classService)
    : IRequestHandler<ListMyTeachingAssignmentsQuery, Result<IReadOnlyList<TeachingAssignmentSummaryResponse>>>
{
    public Task<Result<IReadOnlyList<TeachingAssignmentSummaryResponse>>> Handle(ListMyTeachingAssignmentsQuery request, CancellationToken cancellationToken) =>
        classService.ListMyTeachingAssignmentsAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListTeachingAssignmentsQueryHandler chuyển bộ lọc phân công của học vụ sang ClassService.
/// </summary>
public sealed class ListTeachingAssignmentsQueryHandler(IClassService classService)
    : IRequestHandler<ListTeachingAssignmentsQuery, Result<IReadOnlyList<TeachingAssignmentSummaryResponse>>>
{
    public Task<Result<IReadOnlyList<TeachingAssignmentSummaryResponse>>> Handle(ListTeachingAssignmentsQuery request, CancellationToken cancellationToken) =>
        classService.ListTeachingAssignmentsAsync(request, cancellationToken);
}
