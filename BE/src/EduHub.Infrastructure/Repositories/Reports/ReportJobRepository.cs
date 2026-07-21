using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Reports;
using EduHub.Application.Interfaces.Repositories.Reports;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Entities.Reports;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Reports;

/// <summary>
/// Ghi chú: ReportJobRepository dùng EF Core để truy cập report jobs và quyền parent-student.
/// </summary>
public sealed class ReportJobRepository(ApplicationDbContext dbContext) : IReportJobRepository
{
    /// <summary>
    /// Ghi chú: ParentCanReadStudentAsync kiểm tra phụ huynh active có liên kết với học sinh.
    /// </summary>
    public Task<bool> ParentCanReadStudentAsync(Guid parentUserId, Guid studentId, CancellationToken cancellationToken) =>
        dbContext.ParentStudents.AsNoTracking().AnyAsync(
            link => link.ParentUserId == parentUserId && link.StudentId == studentId && link.IsActive,
            cancellationToken);

    /// <summary>
    /// Ghi chú: StudentExistsAsync kiểm tra học sinh tồn tại.
    /// </summary>
    public Task<bool> StudentExistsAsync(Guid studentId, CancellationToken cancellationToken) =>
        dbContext.Students.AsNoTracking().AnyAsync(student => student.Id == studentId, cancellationToken);

    /// <summary>
    /// Ghi chú: SemesterExistsAsync kiểm tra học kỳ tồn tại.
    /// </summary>
    public Task<bool> SemesterExistsAsync(Guid semesterId, CancellationToken cancellationToken) =>
        dbContext.Semesters.AsNoTracking().AnyAsync(semester => semester.Id == semesterId, cancellationToken);

    /// <summary>
    /// Ghi chú: StudentWasEnrolledAsync xác nhận học sinh có enrollment thuộc học kỳ được chọn.
    /// </summary>
    public Task<bool> StudentWasEnrolledAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken) =>
        dbContext.Enrollments.AsNoTracking().AnyAsync(enrollment => enrollment.StudentId == studentId && enrollment.SemesterId == semesterId, cancellationToken);

    /// <summary>
    /// Ghi chú: StudentHasPublishedGradesAsync xác nhận báo cáo học kỳ có ít nhất một điểm đã công bố hoặc khóa.
    /// </summary>
    public Task<bool> StudentHasPublishedGradesAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken) =>
        dbContext.GradeEntries.AsNoTracking().AnyAsync(entry =>
            entry.StudentId == studentId &&
            entry.Assignment.SemesterId == semesterId &&
            (entry.Status == GradeStatus.Published || entry.Status == GradeStatus.Locked),
            cancellationToken);

    /// <summary>
    /// Ghi chú: GetByRequesterAndIdempotencyKeyAsync lấy job cũ để request lặp không enqueue nhiều lần.
    /// </summary>
    public Task<ReportJob?> GetByRequesterAndIdempotencyKeyAsync(
        Guid requesterUserId,
        string idempotencyKey,
        CancellationToken cancellationToken) =>
        dbContext.ReportJobs.SingleOrDefaultAsync(
            job => job.RequesterUserId == requesterUserId && job.IdempotencyKey == idempotencyKey,
            cancellationToken);

    /// <summary>
    /// Ghi chú: GetAsync lấy report job theo id.
    /// </summary>
    public Task<ReportJob?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.ReportJobs.SingleOrDefaultAsync(job => job.Id == id, cancellationToken);

    /// <summary>
    /// Ghi chú: Add thêm report job mới vào DbContext.
    /// </summary>
    public void Add(ReportJob reportJob) => dbContext.ReportJobs.Add(reportJob);

    public Task<bool> HasOpenRequestAsync(Guid requesterUserId, Guid studentId, Guid semesterId, CancellationToken cancellationToken) =>
        dbContext.ReportRequests.AnyAsync(request => request.RequesterUserId == requesterUserId && request.StudentId == studentId && request.SemesterId == semesterId && request.Status != ReportRequestStatus.Rejected && request.Status != ReportRequestStatus.Completed && request.Status != ReportRequestStatus.Failed, cancellationToken);

    public Task<ReportRequest?> GetRequestAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.ReportRequests.SingleOrDefaultAsync(request => request.Id == id, cancellationToken);

    public Task<ReportRequestResponse?> GetRequestResponseAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.ReportRequests.AsNoTracking().Where(request => request.Id == id)
            .Select(request => new ReportRequestResponse(request.Id, request.StudentId, request.Student.StudentCode, request.Student.FullName, request.SemesterId, request.Semester.Name, request.RequesterUserId, request.RequesterUser.FullName, request.ReviewerUserId, request.ReviewerUser != null ? request.ReviewerUser.FullName : null, request.ReportJobId, request.Purpose, request.ReviewNote, request.Status.ToString(), request.ReportJob != null ? request.ReportJob.Status.ToString() : null, request.RequestedAtUtc, request.ReviewedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Ghi chú: ListRequestsAsync trả inbox cho AcademicAdmin hoặc chỉ lịch sử do Parent hiện tại tạo.
    /// </summary>
    public async Task<PagedResult<ReportRequestResponse>> ListRequestsAsync(Guid currentUserId, UserRole role, ReportRequestStatus? status, PageRequest pageRequest, CancellationToken cancellationToken)
    {
        var query = dbContext.ReportRequests.AsNoTracking();
        if (role == UserRole.Parent) query = query.Where(request => request.RequesterUserId == currentUserId);
        if (status.HasValue) query = query.Where(request => request.Status == status.Value);
        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.OrderByDescending(request => request.RequestedAtUtc).Skip(pageRequest.Skip).Take(pageRequest.PageSize)
            .Select(request => new ReportRequestResponse(request.Id, request.StudentId, request.Student.StudentCode, request.Student.FullName, request.SemesterId, request.Semester.Name, request.RequesterUserId, request.RequesterUser.FullName, request.ReviewerUserId, request.ReviewerUser != null ? request.ReviewerUser.FullName : null, request.ReportJobId, request.Purpose, request.ReviewNote, request.Status.ToString(), request.ReportJob != null ? request.ReportJob.Status.ToString() : null, request.RequestedAtUtc, request.ReviewedAtUtc))
            .ToListAsync(cancellationToken);
        return new PagedResult<ReportRequestResponse>(items, pageRequest.Page, pageRequest.PageSize, total);
    }

    public void AddRequest(ReportRequest request) => dbContext.ReportRequests.Add(request);

    public void AddOutboxMessage(OutboxMessage message) => dbContext.OutboxMessages.Add(message);
}
