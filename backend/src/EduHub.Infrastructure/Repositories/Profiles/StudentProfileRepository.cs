using EduHub.Application.Interfaces.Repositories.Profiles;
using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Profiles;

/// <summary>
/// Ghi chú: StudentProfileRepository dùng EF Core để đọc hồ sơ và lưu yêu cầu sửa thông tin học sinh.
/// </summary>
public sealed class StudentProfileRepository(ApplicationDbContext dbContext) : IStudentProfileRepository
{
    public Task<Student?> GetStudentByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Students.Include(student => student.User).SingleOrDefaultAsync(student => student.UserId == userId, cancellationToken);

    public Task<bool> HasPendingRequestAsync(Guid studentId, CancellationToken cancellationToken) =>
        dbContext.StudentProfileChangeRequests.AnyAsync(request => request.StudentId == studentId && request.Status == ProfileChangeRequestStatus.Pending, cancellationToken);

    public void AddRequest(StudentProfileChangeRequest request) => dbContext.StudentProfileChangeRequests.Add(request);

    /// <summary>
    /// Ghi chú: ListRequestsAsync tải yêu cầu kèm học sinh, người gửi và người duyệt để service tạo response đầy đủ.
    /// </summary>
    public async Task<IReadOnlyList<StudentProfileChangeRequest>> ListRequestsAsync(Guid? studentId, ProfileChangeRequestStatus? status, CancellationToken cancellationToken)
    {
        var query = dbContext.StudentProfileChangeRequests.AsNoTracking()
            .Include(request => request.Student)
            .Include(request => request.RequesterUser)
            .Include(request => request.ReviewerUser)
            .AsQueryable();
        if (studentId.HasValue) query = query.Where(request => request.StudentId == studentId.Value);
        if (status.HasValue) query = query.Where(request => request.Status == status.Value);
        return await query.OrderByDescending(request => request.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task<StudentProfileChangeRequest?> GetRequestAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.StudentProfileChangeRequests
            .Include(request => request.Student).ThenInclude(student => student.User)
            .Include(request => request.RequesterUser)
            .Include(request => request.ReviewerUser)
            .SingleOrDefaultAsync(request => request.Id == id, cancellationToken);

    /// <summary>
    /// Ghi chú: GetCurrentEnrollmentAsync lấy lớp-học kỳ active mới nhất để học sinh mở thời khóa biểu.
    /// </summary>
    public Task<IStudentProfileRepository.StudentCurrentEnrollment?> GetCurrentEnrollmentAsync(Guid studentId, CancellationToken cancellationToken) =>
        dbContext.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.StudentId == studentId && enrollment.Status == EnrollmentStatus.Active)
            .OrderByDescending(enrollment => enrollment.Semester.StartDate)
            .Select(enrollment => new IStudentProfileRepository.StudentCurrentEnrollment(
                enrollment.ClassRoomId,
                enrollment.ClassRoom.Name,
                enrollment.SemesterId,
                enrollment.Semester.Name))
            .FirstOrDefaultAsync(cancellationToken);
}
