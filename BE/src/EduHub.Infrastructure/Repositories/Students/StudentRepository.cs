using EduHub.Application.Common.Models;
using EduHub.Application.Features.Students.Common;
using EduHub.Application.Contracts.Students;
using EduHub.Application.Interfaces.Repositories.Students;
using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Students;

/// <summary>
/// Ghi chú: StudentRepository dùng EF Core để truy cập dữ liệu học sinh và liên kết phụ huynh-học sinh.
/// </summary>
public sealed class StudentRepository(ApplicationDbContext dbContext) : IStudentRepository
{
    /// <summary>
    /// Ghi chú: StudentCodeExistsAsync kiểm tra mã học sinh đã tồn tại.
    /// </summary>
    public Task<bool> StudentCodeExistsAsync(string normalizedStudentCode, CancellationToken cancellationToken) =>
        dbContext.Students.AnyAsync(student => student.NormalizedStudentCode == normalizedStudentCode, cancellationToken);

    /// <summary>
    /// Ghi chú: AddStudent thêm hồ sơ học sinh mới vào DbContext.
    /// </summary>
    public void AddStudent(Student student) => dbContext.Students.Add(student);

    /// <summary>
    /// Ghi chú: GetStudentAsync lấy học sinh theo id để cập nhật.
    /// </summary>
    public Task<Student?> GetStudentAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Students.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

    /// <summary>
    /// Ghi chú: GetStudentByScopeAsync đọc chi tiết học sinh theo quyền truy cập của user hiện tại.
    /// </summary>
    public Task<StudentResponse?> GetStudentByScopeAsync(
        Guid id,
        UserRole? role,
        Guid? userId,
        CancellationToken cancellationToken) =>
        ApplyReadScope(dbContext.Students.AsNoTracking(), role, userId)
            .Where(candidate => candidate.Id == id)
            .Select(candidate => new StudentResponse(
                candidate.Id,
                candidate.StudentCode,
                candidate.FullName,
                candidate.DateOfBirth,
                candidate.Status.ToString(),
                candidate.Version,
                dbContext.Enrollments.Where(enrollment => enrollment.StudentId == candidate.Id && enrollment.Status == EnrollmentStatus.Active).OrderByDescending(enrollment => enrollment.Semester.StartDate).Select(enrollment => (Guid?)enrollment.ClassRoomId).FirstOrDefault(),
                dbContext.Enrollments.Where(enrollment => enrollment.StudentId == candidate.Id && enrollment.Status == EnrollmentStatus.Active).OrderByDescending(enrollment => enrollment.Semester.StartDate).Select(enrollment => enrollment.ClassRoom.ClassCode).FirstOrDefault(),
                dbContext.Enrollments.Where(enrollment => enrollment.StudentId == candidate.Id && enrollment.Status == EnrollmentStatus.Active).OrderByDescending(enrollment => enrollment.Semester.StartDate).Select(enrollment => enrollment.ClassRoom.Name).FirstOrDefault(),
                dbContext.ParentStudents.Count(link => link.StudentId == candidate.Id && link.IsActive),
                candidate.User != null ? candidate.User.Email : null,
                candidate.User != null ? candidate.User.IsActive : null))
            .SingleOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Ghi chú: ListStudentsByScopeAsync đọc danh sách học sinh theo quyền truy cập, trạng thái và phân trang.
    /// </summary>
    public async Task<PagedResult<StudentResponse>> ListStudentsByScopeAsync(
        StudentStatus? status,
        Guid? classRoomId,
        PageRequest pageRequest,
        UserRole? role,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var query = ApplyReadScope(dbContext.Students.AsNoTracking(), role, userId);
        if (status.HasValue)
        {
            query = query.Where(student => student.Status == status.Value);
        }

        if (classRoomId.HasValue)
        {
            query = query.Where(student => dbContext.Enrollments.Any(enrollment => enrollment.StudentId == student.Id && enrollment.ClassRoomId == classRoomId.Value && enrollment.Status == EnrollmentStatus.Active));
        }

        if (pageRequest.Search is not null)
        {
            var code = StudentNormalization.Code(pageRequest.Search);
            var normalizedName = StudentNormalization.SearchText(pageRequest.Search);
            var pattern = $"%{pageRequest.Search.Trim()}%";
            var normalizedNamePattern = $"%{normalizedName}%";
            query = query.Where(student =>
                student.NormalizedStudentCode.Contains(code) ||
                EF.Functions.ILike(student.NormalizedFullName, normalizedNamePattern) ||
                dbContext.Enrollments.Any(enrollment => enrollment.StudentId == student.Id && enrollment.Status == EnrollmentStatus.Active && (EF.Functions.ILike(enrollment.ClassRoom.Name, pattern) || EF.Functions.ILike(enrollment.ClassRoom.ClassCode, pattern))) ||
                dbContext.ParentStudents.Any(link => link.StudentId == student.Id && link.IsActive && (EF.Functions.ILike(link.ParentUser.FullName, pattern) || EF.Functions.ILike(link.ParentUser.Email, pattern))));
        }

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderBy(student => student.StudentCode)
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .Select(student => new StudentResponse(
                student.Id,
                student.StudentCode,
                student.FullName,
                student.DateOfBirth,
                student.Status.ToString(),
                student.Version,
                dbContext.Enrollments.Where(enrollment => enrollment.StudentId == student.Id && enrollment.Status == EnrollmentStatus.Active).OrderByDescending(enrollment => enrollment.Semester.StartDate).Select(enrollment => (Guid?)enrollment.ClassRoomId).FirstOrDefault(),
                dbContext.Enrollments.Where(enrollment => enrollment.StudentId == student.Id && enrollment.Status == EnrollmentStatus.Active).OrderByDescending(enrollment => enrollment.Semester.StartDate).Select(enrollment => enrollment.ClassRoom.ClassCode).FirstOrDefault(),
                dbContext.Enrollments.Where(enrollment => enrollment.StudentId == student.Id && enrollment.Status == EnrollmentStatus.Active).OrderByDescending(enrollment => enrollment.Semester.StartDate).Select(enrollment => enrollment.ClassRoom.Name).FirstOrDefault(),
                dbContext.ParentStudents.Count(link => link.StudentId == student.Id && link.IsActive),
                student.User != null ? student.User.Email : null,
                student.User != null ? student.User.IsActive : null))
            .ToListAsync(cancellationToken);

        return new PagedResult<StudentResponse>(items, pageRequest.Page, pageRequest.PageSize, total);
    }

    /// <summary>
    /// Ghi chú: GetStudentDetailByScopeAsync đọc hồ sơ, lịch sử lớp và phụ huynh theo quyền người dùng hiện tại.
    /// </summary>
    public async Task<StudentDetailResponse?> GetStudentDetailByScopeAsync(Guid studentId, UserRole? role, Guid? userId, CancellationToken cancellationToken)
    {
        var student = await GetStudentByScopeAsync(studentId, role, userId, cancellationToken);
        if (student is null) return null;

        var enrollments = await dbContext.Enrollments.AsNoTracking().Where(enrollment => enrollment.StudentId == studentId)
            .OrderByDescending(enrollment => enrollment.Semester.StartDate)
            .Select(enrollment => new StudentEnrollmentSummaryResponse(enrollment.Id, enrollment.ClassRoomId, enrollment.ClassRoom.ClassCode, enrollment.ClassRoom.Name, enrollment.SemesterId, enrollment.Semester.Name, enrollment.Status.ToString(), enrollment.EnrolledAtUtc, enrollment.EndedAtUtc))
            .ToListAsync(cancellationToken);

        var guardianQuery = dbContext.ParentStudents.AsNoTracking().Where(link => link.StudentId == studentId);
        if (role == UserRole.Parent && userId.HasValue) guardianQuery = guardianQuery.Where(link => link.ParentUserId == userId.Value && link.IsActive);
        var guardians = await guardianQuery.OrderByDescending(link => link.IsActive)
            .Select(link => new StudentGuardianResponse(link.Id, link.ParentUserId, link.ParentUser.FullName, link.ParentUser.Email, link.ParentUser.PhoneNumber, link.Relationship, link.IsActive))
            .ToListAsync(cancellationToken);

        return new StudentDetailResponse(student, enrollments, guardians);
    }

    /// <summary>
    /// Ghi chú: ListChildrenAsync trả các học sinh có liên kết active với phụ huynh cùng lớp và học kỳ hiện tại.
    /// </summary>
    public async Task<IReadOnlyList<ChildSummaryResponse>> ListChildrenAsync(Guid parentUserId, CancellationToken cancellationToken) =>
        await dbContext.ParentStudents.AsNoTracking().Where(link => link.ParentUserId == parentUserId && link.IsActive)
            .OrderBy(link => link.Student.FullName)
            .Select(link => new ChildSummaryResponse(
                link.StudentId,
                link.Student.StudentCode,
                link.Student.FullName,
                link.Student.DateOfBirth,
                link.Relationship,
                dbContext.Enrollments.Where(enrollment => enrollment.StudentId == link.StudentId && enrollment.Status == EnrollmentStatus.Active).OrderByDescending(enrollment => enrollment.Semester.StartDate).Select(enrollment => (Guid?)enrollment.ClassRoomId).FirstOrDefault(),
                dbContext.Enrollments.Where(enrollment => enrollment.StudentId == link.StudentId && enrollment.Status == EnrollmentStatus.Active).OrderByDescending(enrollment => enrollment.Semester.StartDate).Select(enrollment => enrollment.ClassRoom.ClassCode).FirstOrDefault(),
                dbContext.Enrollments.Where(enrollment => enrollment.StudentId == link.StudentId && enrollment.Status == EnrollmentStatus.Active).OrderByDescending(enrollment => enrollment.Semester.StartDate).Select(enrollment => enrollment.ClassRoom.Name).FirstOrDefault(),
                dbContext.Enrollments.Where(enrollment => enrollment.StudentId == link.StudentId && enrollment.Status == EnrollmentStatus.Active).OrderByDescending(enrollment => enrollment.Semester.StartDate).Select(enrollment => (Guid?)enrollment.SemesterId).FirstOrDefault(),
                dbContext.Enrollments.Where(enrollment => enrollment.StudentId == link.StudentId && enrollment.Status == EnrollmentStatus.Active).OrderByDescending(enrollment => enrollment.Semester.StartDate).Select(enrollment => enrollment.Semester.Name).FirstOrDefault()))
            .ToListAsync(cancellationToken);

    public Task<bool> StudentUserIsValidAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(user => user.Id == userId && user.Role == UserRole.Student && user.IsActive, cancellationToken);

    public Task<bool> StudentUserLinkedAsync(Guid userId, Guid exceptStudentId, CancellationToken cancellationToken) =>
        dbContext.Students.AnyAsync(student => student.UserId == userId && student.Id != exceptStudentId, cancellationToken);

    /// <summary>
    /// Ghi chú: StudentExistsAsync kiểm tra học sinh tồn tại theo id.
    /// </summary>
    public Task<bool> StudentExistsAsync(Guid studentId, CancellationToken cancellationToken) =>
        dbContext.Students.AnyAsync(student => student.Id == studentId, cancellationToken);

    /// <summary>
    /// Ghi chú: ParentUserIsValidAsync kiểm tra user phụ huynh active có role Parent.
    /// </summary>
    public Task<bool> ParentUserIsValidAsync(Guid parentUserId, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(
            user => user.Id == parentUserId && user.Role == UserRole.Parent && user.IsActive,
            cancellationToken);

    /// <summary>
    /// Ghi chú: GetParentStudentLinkAsync lấy liên kết phụ huynh-học sinh bất kể active hay inactive.
    /// </summary>
    public Task<ParentStudent?> GetParentStudentLinkAsync(
        Guid studentId,
        Guid parentUserId,
        CancellationToken cancellationToken) =>
        dbContext.ParentStudents.SingleOrDefaultAsync(
            link => link.StudentId == studentId && link.ParentUserId == parentUserId,
            cancellationToken);

    /// <summary>
    /// Ghi chú: GetActiveParentStudentLinkAsync lấy liên kết phụ huynh-học sinh đang active.
    /// </summary>
    public Task<ParentStudent?> GetActiveParentStudentLinkAsync(
        Guid studentId,
        Guid parentUserId,
        CancellationToken cancellationToken) =>
        dbContext.ParentStudents.SingleOrDefaultAsync(
            link => link.StudentId == studentId && link.ParentUserId == parentUserId && link.IsActive,
            cancellationToken);

    /// <summary>
    /// Ghi chú: AddParentStudentLink thêm liên kết phụ huynh-học sinh mới vào DbContext.
    /// </summary>
    public void AddParentStudentLink(ParentStudent link) => dbContext.ParentStudents.Add(link);

    /// <summary>
    /// Ghi chú: ApplyReadScope lọc học sinh theo quyền Admin hoặc Parent hiện tại.
    /// </summary>
    private IQueryable<Student> ApplyReadScope(IQueryable<Student> students, UserRole? role, Guid? userId)
    {
        if (role is UserRole.SystemAdmin or UserRole.AcademicAdmin)
        {
            return students;
        }

        if (role is UserRole.Parent && userId.HasValue)
        {
            return students.Where(student => dbContext.ParentStudents.Any(link =>
                link.StudentId == student.Id &&
                link.ParentUserId == userId.Value &&
                link.IsActive));
        }

        if (role is UserRole.Student && userId.HasValue)
        {
            return students.Where(student => student.UserId == userId.Value);
        }

        return students.Where(_ => false);
    }
}
