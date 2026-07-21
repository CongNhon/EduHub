using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Classes;
using EduHub.Application.Features.Classes.Common;
using EduHub.Application.Interfaces.Repositories.Classes;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Classes;

/// <summary>
/// Ghi chú: ClassRepository dùng EF Core để truy cập dữ liệu lớp học, phân công giáo viên và ghi danh học sinh.
/// </summary>
public sealed class ClassRepository(ApplicationDbContext dbContext) : IClassRepository
{
    /// <summary>
    /// Ghi chú: AcademicYearExistsAsync kiểm tra năm học tồn tại cho lớp học.
    /// </summary>
    public Task<bool> AcademicYearExistsAsync(Guid academicYearId, CancellationToken cancellationToken) =>
        dbContext.AcademicYears.AnyAsync(year => year.Id == academicYearId, cancellationToken);

    /// <summary>
    /// Ghi chú: ClassCodeExistsAsync kiểm tra mã lớp trùng trong năm học.
    /// </summary>
    public Task<bool> ClassCodeExistsAsync(
        Guid academicYearId,
        string normalizedClassCode,
        CancellationToken cancellationToken) =>
        dbContext.ClassRooms.AnyAsync(
            classRoom =>
                classRoom.AcademicYearId == academicYearId &&
                classRoom.NormalizedClassCode == normalizedClassCode,
            cancellationToken);

    /// <summary>
    /// Ghi chú: AddClassRoom thêm lớp học mới vào DbContext.
    /// </summary>
    public void AddClassRoom(ClassRoom classRoom) => dbContext.ClassRooms.Add(classRoom);

    /// <summary>
    /// Ghi chú: GetClassRoomAsync lấy lớp học theo id để cập nhật.
    /// </summary>
    public Task<ClassRoom?> GetClassRoomAsync(Guid classRoomId, CancellationToken cancellationToken) =>
        dbContext.ClassRooms.SingleOrDefaultAsync(candidate => candidate.Id == classRoomId, cancellationToken);

    /// <summary>
    /// Ghi chú: GetActiveClassRoomAsync lấy lớp học active theo id.
    /// </summary>
    public Task<ClassRoom?> GetActiveClassRoomAsync(Guid classRoomId, CancellationToken cancellationToken) =>
        dbContext.ClassRooms
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == classRoomId && candidate.IsActive, cancellationToken);

    /// <summary>
    /// Ghi chú: ListClassRoomsAsync đọc danh sách lớp học active đã phân trang.
    /// </summary>
    public async Task<PagedResult<ClassRoomResponse>> ListClassRoomsAsync(
        Guid? academicYearId,
        PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ClassRooms.AsNoTracking().Where(classRoom => classRoom.IsActive);
        if (academicYearId.HasValue)
        {
            query = query.Where(classRoom => classRoom.AcademicYearId == academicYearId.Value);
        }

        if (pageRequest.Search is not null)
        {
            var code = ClassNormalization.Code(pageRequest.Search);
            var pattern = $"%{pageRequest.Search.Trim()}%";
            query = query.Where(classRoom =>
                classRoom.NormalizedClassCode.Contains(code) ||
                EF.Functions.ILike(classRoom.Name, pattern));
        }

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderBy(classRoom => classRoom.ClassCode)
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .Select(classRoom => new ClassRoomResponse(
                classRoom.Id,
                classRoom.ClassCode,
                classRoom.Name,
                classRoom.AcademicYearId,
                classRoom.GradeLevel,
                classRoom.Capacity,
                classRoom.ActiveEnrollmentCount,
                classRoom.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResult<ClassRoomResponse>(items, pageRequest.Page, pageRequest.PageSize, total);
    }

    /// <summary>
    /// Ghi chú: GetSemesterAsync lấy học kỳ theo id.
    /// </summary>
    public Task<Semester?> GetSemesterAsync(Guid semesterId, CancellationToken cancellationToken) =>
        dbContext.Semesters.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == semesterId, cancellationToken);

    /// <summary>
    /// Ghi chú: ActiveSubjectExistsAsync kiểm tra môn học active theo id.
    /// </summary>
    public Task<bool> ActiveSubjectExistsAsync(Guid subjectId, CancellationToken cancellationToken) =>
        dbContext.Subjects.AnyAsync(subject => subject.Id == subjectId && subject.IsActive, cancellationToken);

    /// <summary>
    /// Ghi chú: ActiveTeacherExistsAsync kiểm tra giáo viên active có role Teacher.
    /// </summary>
    public Task<bool> ActiveTeacherExistsAsync(Guid teacherId, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(
            user => user.Id == teacherId && user.Role == UserRole.Teacher && user.IsActive,
            cancellationToken);

    /// <summary>
    /// Ghi chú: ActiveTeachingAssignmentExistsAsync kiểm tra phân công giáo viên active bị trùng scope.
    /// </summary>
    public Task<bool> ActiveTeachingAssignmentExistsAsync(
        Guid classRoomId,
        Guid subjectId,
        Guid semesterId,
        CancellationToken cancellationToken) =>
        dbContext.TeachingAssignments.AnyAsync(
            assignment =>
                assignment.ClassRoomId == classRoomId &&
                assignment.SubjectId == subjectId &&
                assignment.SemesterId == semesterId &&
                assignment.IsActive,
            cancellationToken);

    /// <summary>
    /// Ghi chú: ActiveTeacherCapabilityExistsAsync đọc capability active của đúng giáo viên-môn học.
    /// </summary>
    public Task<bool> ActiveTeacherCapabilityExistsAsync(Guid teacherId, Guid subjectId, CancellationToken cancellationToken) =>
        dbContext.TeacherSubjectCapabilities.AnyAsync(capability =>
            capability.TeacherId == teacherId && capability.SubjectId == subjectId && capability.IsActive,
            cancellationToken);

    /// <summary>
    /// Ghi chú: IsHomeroomTeacherForClassAsync tìm phân công GVCN active của giáo viên trong đúng lớp.
    /// </summary>
    public Task<bool> IsHomeroomTeacherForClassAsync(Guid teacherId, Guid classRoomId, CancellationToken cancellationToken) =>
        dbContext.HomeroomAssignments.AnyAsync(assignment =>
            assignment.TeacherId == teacherId && assignment.ClassRoomId == classRoomId && assignment.IsActive,
            cancellationToken);

    /// <summary>
    /// Ghi chú: AddTeachingAssignment thêm phân công giáo viên vào DbContext.
    /// </summary>
    public void AddTeachingAssignment(TeachingAssignment assignment) => dbContext.TeachingAssignments.Add(assignment);

    /// <summary>
    /// Ghi chú: ListTeachingAssignmentsAsync đọc đúng lớp-môn-học kỳ thuộc giáo viên và kèm trạng thái sổ điểm.
    /// </summary>
    public async Task<IReadOnlyList<TeachingAssignmentSummaryResponse>> ListTeachingAssignmentsAsync(Guid? teacherId, Guid? classRoomId, Guid? semesterId, CancellationToken cancellationToken)
    {
        var query = dbContext.TeachingAssignments.AsNoTracking().Where(assignment => assignment.IsActive);
        if (teacherId.HasValue) query = query.Where(assignment => assignment.TeacherId == teacherId.Value);
        if (classRoomId.HasValue) query = query.Where(assignment => assignment.ClassRoomId == classRoomId.Value);
        if (semesterId.HasValue) query = query.Where(assignment => assignment.SemesterId == semesterId.Value);
        var items = await query.OrderByDescending(assignment => assignment.Semester.StartDate).ThenBy(assignment => assignment.ClassRoom.ClassCode).ThenBy(assignment => assignment.Subject.Name)
            .Select(assignment => new
            {
                assignment.Id,
                assignment.ClassRoomId,
                assignment.ClassRoom.ClassCode,
                ClassName = assignment.ClassRoom.Name,
                assignment.SubjectId,
                assignment.Subject.SubjectCode,
                SubjectName = assignment.Subject.Name,
                assignment.SemesterId,
                SemesterName = assignment.Semester.Name,
                assignment.TeacherId,
                TeacherName = assignment.Teacher.FullName ?? assignment.Teacher.Email,
                StudentCount = dbContext.Enrollments.Count(enrollment => enrollment.ClassRoomId == assignment.ClassRoomId && enrollment.SemesterId == assignment.SemesterId && enrollment.Status == EnrollmentStatus.Active),
                GradebookStatus = dbContext.GradeEntries.Where(entry => entry.AssignmentId == assignment.Id).Select(entry => entry.Status).FirstOrDefault(),
                assignment.IsActive
            })
            .ToListAsync(cancellationToken);

        return items.Select(item => new TeachingAssignmentSummaryResponse(item.Id, item.ClassRoomId, item.ClassCode, item.ClassName, item.SubjectId, item.SubjectCode, item.SubjectName, item.SemesterId, item.SemesterName, item.TeacherId, item.TeacherName, item.StudentCount, item.GradebookStatus.ToString(), item.IsActive)).ToList();
    }

    /// <summary>
    /// Ghi chú: ActiveStudentExistsAsync kiểm tra học sinh active theo id.
    /// </summary>
    public Task<bool> ActiveStudentExistsAsync(Guid studentId, CancellationToken cancellationToken) =>
        dbContext.Students.AnyAsync(
            student => student.Id == studentId && student.Status == StudentStatus.Active,
            cancellationToken);

    /// <summary>
    /// Ghi chú: ActiveEnrollmentExistsAsync kiểm tra học sinh đã có enrollment active trong học kỳ.
    /// </summary>
    public Task<bool> ActiveEnrollmentExistsAsync(
        Guid studentId,
        Guid semesterId,
        CancellationToken cancellationToken) =>
        dbContext.Enrollments.AnyAsync(
            enrollment =>
                enrollment.StudentId == studentId &&
                enrollment.SemesterId == semesterId &&
                enrollment.Status == EnrollmentStatus.Active,
            cancellationToken);

    /// <summary>
    /// Ghi chú: ReserveSeatAsync tăng sĩ số active bằng atomic update không vượt capacity.
    /// </summary>
    public async Task<bool> ReserveSeatAsync(Guid classRoomId, CancellationToken cancellationToken)
    {
        var affectedRows = await dbContext.ClassRooms
            .Where(classRoom =>
                classRoom.Id == classRoomId &&
                classRoom.IsActive &&
                classRoom.ActiveEnrollmentCount < classRoom.Capacity)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    classRoom => classRoom.ActiveEnrollmentCount,
                    classRoom => classRoom.ActiveEnrollmentCount + 1),
                cancellationToken);

        return affectedRows == 1;
    }

    /// <summary>
    /// Ghi chú: ReleaseSeatAsync giảm sĩ số active khi học sinh rút hoặc chuyển lớp.
    /// </summary>
    public async Task ReleaseSeatAsync(Guid classRoomId, CancellationToken cancellationToken)
    {
        await dbContext.ClassRooms
            .Where(classRoom => classRoom.Id == classRoomId && classRoom.ActiveEnrollmentCount > 0)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    classRoom => classRoom.ActiveEnrollmentCount,
                    classRoom => classRoom.ActiveEnrollmentCount - 1),
                cancellationToken);
    }

    /// <summary>
    /// Ghi chú: AddEnrollment thêm ghi danh học sinh vào DbContext.
    /// </summary>
    public void AddEnrollment(Enrollment enrollment) => dbContext.Enrollments.Add(enrollment);

    /// <summary>
    /// Ghi chú: GetActiveEnrollmentAsync lấy enrollment active của học sinh trong lớp và học kỳ.
    /// </summary>
    public Task<Enrollment?> GetActiveEnrollmentAsync(
        Guid studentId,
        Guid classRoomId,
        Guid semesterId,
        CancellationToken cancellationToken) =>
        dbContext.Enrollments.SingleOrDefaultAsync(
            enrollment =>
                enrollment.StudentId == studentId &&
                enrollment.ClassRoomId == classRoomId &&
                enrollment.SemesterId == semesterId &&
                enrollment.Status == EnrollmentStatus.Active,
            cancellationToken);
}
