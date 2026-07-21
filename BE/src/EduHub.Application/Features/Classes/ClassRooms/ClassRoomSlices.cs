using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Classes;
using EduHub.Application.Interfaces.Services.Classes;
using FluentValidation;
using MediatR;

namespace EduHub.Application.Features.Classes.ClassRooms;

/// <summary>
/// Ghi chú: CreateClassRoomCommandValidator kiểm tra dữ liệu đầu vào cho lớp học mới trước khi handler chạy.
/// </summary>
public sealed class CreateClassRoomCommandValidator : AbstractValidator<CreateClassRoomCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo validator cho lớp học mới.
    /// </summary>
    public CreateClassRoomCommandValidator()
    {
        RuleFor(command => command.ClassCode).NotEmpty().MaximumLength(64);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(128);
        RuleFor(command => command.AcademicYearId).NotEmpty();
        RuleFor(command => command.GradeLevel).GreaterThan(0);
        RuleFor(command => command.Capacity).GreaterThan(0);
    }
}

/// <summary>
/// Ghi chú: CreateClassRoomCommandHandler xử lý tạo lớp học mới và kiểm tra mã lớp trùng trong năm học.
/// </summary>
public sealed class CreateClassRoomCommandHandler(IClassService classService)
    : IRequestHandler<CreateClassRoomCommand, Result<ClassRoomResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command tạo lớp học sang ClassService.
    /// </summary>
    public Task<Result<ClassRoomResponse>> Handle(CreateClassRoomCommand request, CancellationToken cancellationToken) =>
        classService.CreateClassRoomAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: UpdateClassRoomCommandValidator kiểm tra dữ liệu đầu vào cho lớp học hiện có trước khi handler chạy.
/// </summary>
public sealed class UpdateClassRoomCommandValidator : AbstractValidator<UpdateClassRoomCommand>
{
    /// <summary>
    /// Ghi chú: Constructor khởi tạo validator cho cập nhật lớp học.
    /// </summary>
    public UpdateClassRoomCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(128);
        RuleFor(command => command.GradeLevel).GreaterThan(0);
        RuleFor(command => command.Capacity).GreaterThan(0);
    }
}

/// <summary>
/// Ghi chú: UpdateClassRoomCommandHandler xử lý cập nhật tên lớp, khối lớp và sức chứa.
/// </summary>
public sealed class UpdateClassRoomCommandHandler(IClassService classService)
    : IRequestHandler<UpdateClassRoomCommand, Result<ClassRoomResponse>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển command cập nhật lớp học sang ClassService.
    /// </summary>
    public Task<Result<ClassRoomResponse>> Handle(UpdateClassRoomCommand request, CancellationToken cancellationToken) =>
        classService.UpdateClassRoomAsync(request, cancellationToken);
}

/// <summary>
/// Ghi chú: ListClassRoomsQueryHandler xử lý đọc danh sách lớp học theo năm học, tìm kiếm và phân trang.
/// </summary>
public sealed class ListClassRoomsQueryHandler(IClassService classService)
    : IRequestHandler<ListClassRoomsQuery, Result<PagedResult<ClassRoomResponse>>>
{
    /// <summary>
    /// Ghi chú: Handle chuyển query danh sách lớp học sang ClassService.
    /// </summary>
    public Task<Result<PagedResult<ClassRoomResponse>>> Handle(ListClassRoomsQuery request, CancellationToken cancellationToken) =>
        classService.ListClassRoomsAsync(request, cancellationToken);
}
