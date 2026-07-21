using EduHub.Application.Interfaces.Repositories.StudentImports;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Entities.Students;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.StudentImports;

/// <summary>
/// Ghi chú: StudentImportRepository dùng EF Core để tra cứu và thêm dữ liệu của từng dòng import Excel.
/// </summary>
public sealed class StudentImportRepository(ApplicationDbContext dbContext) : IStudentImportRepository
{
    public Task<Student?> GetStudentByNormalizedCodeAsync(string normalizedStudentCode, CancellationToken cancellationToken) =>
        dbContext.Students.SingleOrDefaultAsync(student => student.NormalizedStudentCode == normalizedStudentCode, cancellationToken);

    public Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<bool> IsStudentUserLinkedAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Students.AnyAsync(student => student.UserId == userId, cancellationToken);

    public Task<ClassRoom?> GetClassRoomByNormalizedCodeAsync(string normalizedClassCode, CancellationToken cancellationToken) =>
        dbContext.ClassRooms.SingleOrDefaultAsync(classRoom => classRoom.NormalizedClassCode == normalizedClassCode && classRoom.IsActive, cancellationToken);

    public Task<Semester?> GetSemesterAsync(Guid academicYearId, string normalizedName, CancellationToken cancellationToken) =>
        dbContext.Semesters.SingleOrDefaultAsync(semester => semester.AcademicYearId == academicYearId && semester.NormalizedName == normalizedName, cancellationToken);

    public Task<bool> ParentLinkExistsAsync(Guid parentUserId, Guid studentId, CancellationToken cancellationToken) =>
        dbContext.ParentStudents.AnyAsync(link => link.ParentUserId == parentUserId && link.StudentId == studentId, cancellationToken);

    /// <summary>
    /// Ghi chú: TryIncrementClassEnrollmentCountAsync giữ active count đúng và chặn import vượt sức chứa lớp.
    /// </summary>
    public async Task<bool> TryIncrementClassEnrollmentCountAsync(Guid classRoomId, CancellationToken cancellationToken) =>
        await dbContext.ClassRooms.Where(classRoom => classRoom.Id == classRoomId && classRoom.ActiveEnrollmentCount < classRoom.Capacity)
            .ExecuteUpdateAsync(setters => setters.SetProperty(classRoom => classRoom.ActiveEnrollmentCount, classRoom => classRoom.ActiveEnrollmentCount + 1), cancellationToken) == 1;

    public void AddUser(User user) => dbContext.Users.Add(user);
    public void AddStudent(Student student) => dbContext.Students.Add(student);
    public void AddParentLink(ParentStudent link) => dbContext.ParentStudents.Add(link);
    public void AddEnrollment(Enrollment enrollment) => dbContext.Enrollments.Add(enrollment);
}
