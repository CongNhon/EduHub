using EduHub.Application.Interfaces.Data;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Entities.Notifications;
using EduHub.Domain.Entities.Reports;
using EduHub.Domain.Entities.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EduHub.Infrastructure.Persistence;

/// <summary>
/// Ghi chú: ApplicationDbContext đại diện cho EF Core DbContext của EduHub trong hệ thống EduHub.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();

    public DbSet<Semester> Semesters => Set<Semester>();

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<CurriculumPlan> CurriculumPlans => Set<CurriculumPlan>();

    public DbSet<CurriculumSubjectQuota> CurriculumSubjectQuotas => Set<CurriculumSubjectQuota>();

    public DbSet<TeacherSubjectCapability> TeacherSubjectCapabilities => Set<TeacherSubjectCapability>();

    public DbSet<HomeroomAssignment> HomeroomAssignments => Set<HomeroomAssignment>();

    public DbSet<TimetableVersion> TimetableVersions => Set<TimetableVersion>();

    public DbSet<TimetableEntry> TimetableEntries => Set<TimetableEntry>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<ParentStudent> ParentStudents => Set<ParentStudent>();

    public DbSet<StudentProfileChangeRequest> StudentProfileChangeRequests => Set<StudentProfileChangeRequest>();

    public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();

    public DbSet<TeachingAssignment> TeachingAssignments => Set<TeachingAssignment>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<GradeComponent> GradeComponents => Set<GradeComponent>();

    public DbSet<GradeEntry> GradeEntries => Set<GradeEntry>();

    public DbSet<GradeChangeHistory> GradeChangeHistories => Set<GradeChangeHistory>();

    public DbSet<StudentRemark> StudentRemarks => Set<StudentRemark>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<EmailDigestDelivery> EmailDigestDeliveries => Set<EmailDigestDelivery>();

    public DbSet<ExternalSyncRecord> ExternalSyncRecords => Set<ExternalSyncRecord>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<ReportJob> ReportJobs => Set<ReportJob>();

    public DbSet<ReportRequest> ReportRequests => Set<ReportRequest>();

    /// <summary>
    /// Ghi chú: BeginTransactionAsync mở database transaction cho command cần ghi dữ liệu.
    /// </summary>
    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        new EfUnitOfWorkTransaction(await Database.BeginTransactionAsync(cancellationToken));

    /// <summary>
    /// Ghi chú: ConfigureConventions thực hiện phần xử lý của EF Core DbContext của EduHub.
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp with time zone");
        configurationBuilder.Properties<DateTime?>().HaveColumnType("timestamp with time zone");
        configurationBuilder.Properties<decimal>().HavePrecision(18, 4);
    }

    /// <summary>
    /// Ghi chú: OnModelCreating thực hiện phần xử lý của EF Core DbContext của EduHub.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    private sealed class EfUnitOfWorkTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) =>
            transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default) =>
            transaction.RollbackAsync(cancellationToken);

        public ValueTask DisposeAsync() =>
            transaction.DisposeAsync();
    }
}
