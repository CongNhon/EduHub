using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Grades;
using EduHub.Application.Interfaces.Services.Grades;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Grades.GradeEntries;

/// <summary>
/// Ghi chú: UpdateGradeCommandValidator kiểm tra request tạo/sửa điểm của một học sinh.
/// </summary>
public sealed class UpdateGradeCommandValidator : AbstractValidator<UpdateGradeCommand>
{
    public UpdateGradeCommandValidator()
    {
        RuleFor(command => command.StudentId).NotEmpty();
        RuleFor(command => command.AssignmentId).NotEmpty();
        RuleFor(command => command.ComponentId).NotEmpty();
        RuleFor(command => command.Score).GreaterThanOrEqualTo(0m);
        RuleFor(command => command.Reason).MaximumLength(512);
    }
}

/// <summary>
/// Ghi chú: UpdateGradeCommandHandler chuyển request sửa điểm sang GradeEntryService.
/// </summary>
public sealed class UpdateGradeCommandHandler(IGradeEntryService gradeEntryService)
    : IRequestHandler<UpdateGradeCommand, Result<GradeEntryResponse>>
{
    public Task<Result<GradeEntryResponse>> Handle(UpdateGradeCommand request, CancellationToken cancellationToken) =>
        gradeEntryService.UpdateGradeAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: BulkUpdateGradesCommandValidator kiểm tra request nhập điểm hàng loạt.
/// </summary>
public sealed class BulkUpdateGradesCommandValidator : AbstractValidator<BulkUpdateGradesCommand>
{
    public BulkUpdateGradesCommandValidator()
    {
        RuleFor(command => command.AssignmentId).NotEmpty();
        RuleFor(command => command.Items).NotEmpty();
        RuleForEach(command => command.Items).ChildRules(item =>
        {
            item.RuleFor(row => row.StudentId).NotEmpty();
            item.RuleFor(row => row.ComponentId).NotEmpty();
            item.RuleFor(row => row.Score).GreaterThanOrEqualTo(0m);
            item.RuleFor(row => row.Reason).MaximumLength(512);
        });
    }
}

/// <summary>
/// Ghi chú: BulkUpdateGradesCommandHandler chuyển request nhập điểm hàng loạt sang GradeEntryService.
/// </summary>
public sealed class BulkUpdateGradesCommandHandler(IGradeEntryService gradeEntryService)
    : IRequestHandler<BulkUpdateGradesCommand, Result<BulkUpdateGradesResponse>>
{
    public Task<Result<BulkUpdateGradesResponse>> Handle(BulkUpdateGradesCommand request, CancellationToken cancellationToken) =>
        gradeEntryService.BulkUpdateGradesAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: SubmitGradebookCommandValidator kiểm tra assignment cần nộp sổ điểm.
/// </summary>
public sealed class SubmitGradebookCommandValidator : AbstractValidator<SubmitGradebookCommand>
{
    public SubmitGradebookCommandValidator()
    {
        RuleFor(command => command.AssignmentId).NotEmpty();
    }
}

public sealed class SubmitGradebookCommandHandler(IGradeEntryService gradeEntryService)
    : IRequestHandler<SubmitGradebookCommand, Result<GradebookStateResponse>>
{
    public Task<Result<GradebookStateResponse>> Handle(SubmitGradebookCommand request, CancellationToken cancellationToken) =>
        gradeEntryService.SubmitGradebookAsync(request, cancellationToken);
}

public sealed class PublishGradebookCommandHandler(IGradeEntryService gradeEntryService)
    : IRequestHandler<PublishGradebookCommand, Result<GradebookStateResponse>>
{
    public Task<Result<GradebookStateResponse>> Handle(PublishGradebookCommand request, CancellationToken cancellationToken) =>
        gradeEntryService.PublishGradebookAsync(request, cancellationToken);
}

public sealed class ReopenGradebookCommandValidator : AbstractValidator<ReopenGradebookCommand>
{
    public ReopenGradebookCommandValidator()
    {
        RuleFor(command => command.AssignmentId).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(512);
    }
}

public sealed class ReopenGradebookCommandHandler(IGradeEntryService gradeEntryService)
    : IRequestHandler<ReopenGradebookCommand, Result<GradebookStateResponse>>
{
    public Task<Result<GradebookStateResponse>> Handle(ReopenGradebookCommand request, CancellationToken cancellationToken) =>
        gradeEntryService.ReopenGradebookAsync(request, cancellationToken);
}

public sealed class LockGradebookCommandHandler(IGradeEntryService gradeEntryService)
    : IRequestHandler<LockGradebookCommand, Result<GradebookStateResponse>>
{
    public Task<Result<GradebookStateResponse>> Handle(LockGradebookCommand request, CancellationToken cancellationToken) =>
        gradeEntryService.LockGradebookAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetPublishedGradesForParentQueryValidator kiểm tra học sinh và assignment trước khi phụ huynh đọc điểm đã công bố.
/// </summary>
public sealed class GetPublishedGradesForParentQueryValidator : AbstractValidator<GetPublishedGradesForParentQuery>
{
    /// <summary>
    /// Ghi chú: Constructor khai báo rule bắt buộc cho student id và assignment id.
    /// </summary>
    public GetPublishedGradesForParentQueryValidator()
    {
        RuleFor(query => query.StudentId).NotEmpty();
        RuleFor(query => query.AssignmentId).NotEmpty();
    }
}

/// <summary>
/// Ghi chú: GetPublishedGradesForParentQueryHandler chuyển query phụ huynh đọc điểm đã công bố sang GradeEntryService.
/// </summary>
public sealed class GetPublishedGradesForParentQueryHandler(IGradeEntryService gradeEntryService)
    : IRequestHandler<GetPublishedGradesForParentQuery, Result<PublishedGradebookResponse>>
{
    /// <summary>
    /// Ghi chú: Handle gọi GradeEntryService để kiểm tra liên kết phụ huynh-học sinh và đọc cache/DB.
    /// </summary>
    public Task<Result<PublishedGradebookResponse>> Handle(
        GetPublishedGradesForParentQuery request,
        CancellationToken cancellationToken) =>
        gradeEntryService.GetPublishedGradesForParentAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: GetGradebookQueryHandler đọc bounded gradebook cho giáo viên hoặc quản trị học vụ qua GradeEntryService.
/// </summary>
public sealed class GetGradebookQueryHandler(IGradeEntryService gradeEntryService)
    : IRequestHandler<GetGradebookQuery, Result<GradebookResponse>>
{
    public Task<Result<GradebookResponse>> Handle(GetGradebookQuery request, CancellationToken cancellationToken) =>
        gradeEntryService.GetGradebookAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: UpdateStudentRemarkCommandValidator kiểm tra nhận xét môn học trước khi giáo viên lưu.
/// </summary>
public sealed class UpdateStudentRemarkCommandValidator : AbstractValidator<UpdateStudentRemarkCommand>
{
    public UpdateStudentRemarkCommandValidator()
    {
        RuleFor(command => command.AssignmentId).NotEmpty();
        RuleFor(command => command.StudentId).NotEmpty();
        RuleFor(command => command.Content).NotEmpty().MaximumLength(2000);
    }
}

/// <summary>
/// Ghi chú: UpdateStudentRemarkCommandHandler lưu nhận xét học sinh qua GradeEntryService.
/// </summary>
public sealed class UpdateStudentRemarkCommandHandler(IGradeEntryService gradeEntryService)
    : IRequestHandler<UpdateStudentRemarkCommand, Result<StudentRemarkResponse>>
{
    public Task<Result<StudentRemarkResponse>> Handle(UpdateStudentRemarkCommand request, CancellationToken cancellationToken) =>
        gradeEntryService.UpdateStudentRemarkAsync(request, cancellationToken);
}
