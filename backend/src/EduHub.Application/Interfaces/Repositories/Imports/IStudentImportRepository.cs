using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Entities.Students;

namespace EduHub.Application.Interfaces.Repositories.StudentImports;

/// <summary>
/// Ghi chú: IStudentImportRepository là interface tra cứu và thêm học sinh, tài khoản, phụ huynh, lớp từ Excel.
/// </summary>
public interface IStudentImportRepository
{
    Task<Student?> GetStudentByNormalizedCodeAsync(string normalizedStudentCode, CancellationToken cancellationToken);
    Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<bool> IsStudentUserLinkedAsync(Guid userId, CancellationToken cancellationToken);
    Task<ClassRoom?> GetClassRoomByNormalizedCodeAsync(string normalizedClassCode, CancellationToken cancellationToken);
    Task<Semester?> GetSemesterAsync(Guid academicYearId, string normalizedName, CancellationToken cancellationToken);
    Task<bool> ParentLinkExistsAsync(Guid parentUserId, Guid studentId, CancellationToken cancellationToken);
    Task<bool> TryIncrementClassEnrollmentCountAsync(Guid classRoomId, CancellationToken cancellationToken);
    void AddUser(User user);
    void AddStudent(Student student);
    void AddParentLink(ParentStudent link);
    void AddEnrollment(Enrollment enrollment);
}
