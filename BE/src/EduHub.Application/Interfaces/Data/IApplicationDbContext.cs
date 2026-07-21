using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Entities.Notifications;
using EduHub.Domain.Entities.Reports;
using EduHub.Domain.Entities.Students;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Application.Interfaces.Data;

/// <summary>
/// Ghi chú: IApplicationDbContext là interface cho hợp đồng truy cập database của Application.
/// </summary>
public interface IApplicationDbContext : IUnitOfWork
{
    DbSet<User> Users { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<AcademicYear> AcademicYears { get; }

    DbSet<Semester> Semesters { get; }

    DbSet<Subject> Subjects { get; }

    DbSet<CurriculumPlan> CurriculumPlans { get; }

    DbSet<CurriculumSubjectQuota> CurriculumSubjectQuotas { get; }

    DbSet<TeacherSubjectCapability> TeacherSubjectCapabilities { get; }

    DbSet<HomeroomAssignment> HomeroomAssignments { get; }

    DbSet<TimetableVersion> TimetableVersions { get; }

    DbSet<TimetableEntry> TimetableEntries { get; }

    DbSet<Student> Students { get; }

    DbSet<ParentStudent> ParentStudents { get; }

    DbSet<StudentProfileChangeRequest> StudentProfileChangeRequests { get; }

    DbSet<ClassRoom> ClassRooms { get; }

    DbSet<TeachingAssignment> TeachingAssignments { get; }

    DbSet<Enrollment> Enrollments { get; }

    DbSet<GradeComponent> GradeComponents { get; }

    DbSet<GradeEntry> GradeEntries { get; }

    DbSet<GradeChangeHistory> GradeChangeHistories { get; }

    DbSet<StudentRemark> StudentRemarks { get; }

    DbSet<OutboxMessage> OutboxMessages { get; }

    DbSet<EmailDigestDelivery> EmailDigestDeliveries { get; }

    DbSet<ExternalSyncRecord> ExternalSyncRecords { get; }

    DbSet<Notification> Notifications { get; }

    DbSet<ReportJob> ReportJobs { get; }

    DbSet<ReportRequest> ReportRequests { get; }
}
