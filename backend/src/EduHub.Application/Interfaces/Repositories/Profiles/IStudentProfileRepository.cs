using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;

namespace EduHub.Application.Interfaces.Repositories.Profiles;

/// <summary>
/// Ghi chú: IStudentProfileRepository là interface truy cập hồ sơ và yêu cầu sửa thông tin của học sinh.
/// </summary>
public interface IStudentProfileRepository
{
    /// <summary>
    /// Ghi chú: StudentCurrentEnrollment chứa lớp và học kỳ đang hoạt động của học sinh.
    /// </summary>
    public sealed record StudentCurrentEnrollment(Guid ClassRoomId, string ClassName, Guid SemesterId, string SemesterName);

    Task<Student?> GetStudentByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> HasPendingRequestAsync(Guid studentId, CancellationToken cancellationToken);
    void AddRequest(StudentProfileChangeRequest request);
    Task<IReadOnlyList<StudentProfileChangeRequest>> ListRequestsAsync(Guid? studentId, ProfileChangeRequestStatus? status, CancellationToken cancellationToken);
    Task<StudentProfileChangeRequest?> GetRequestAsync(Guid id, CancellationToken cancellationToken);
    Task<StudentCurrentEnrollment?> GetCurrentEnrollmentAsync(Guid studentId, CancellationToken cancellationToken);
}
