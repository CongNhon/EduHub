using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Repositories.Analytics;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Analytics;

/// <summary>
/// Ghi chú: AdminAnalyticsRepository dùng EF Core tổng hợp dữ liệu PostgreSQL cho dashboard SystemAdmin.
/// </summary>
public sealed class AdminAnalyticsRepository(ApplicationDbContext dbContext) : IAdminAnalyticsRepository
{
    /// <summary>
    /// Ghi chú: ResolveSemesterContextAsync chọn học kỳ yêu cầu, ưu tiên học kỳ active rồi mới dùng học kỳ gần nhất.
    /// </summary>
    public async Task<AnalyticsSemesterContext?> ResolveSemesterContextAsync(Guid? semesterId, CancellationToken cancellationToken)
    {
        var rows = await dbContext.Semesters
            .AsNoTracking()
            .OrderByDescending(semester => semester.StartDate)
            .Select(semester => new
            {
                semester.Id,
                semester.AcademicYearId,
                semester.Name,
                AcademicYearName = semester.AcademicYear.Name,
                semester.StartDate,
                semester.EndDate,
                semester.Status
            })
            .ToListAsync(cancellationToken);

        var selected = semesterId.HasValue
            ? rows.SingleOrDefault(semester => semester.Id == semesterId.Value)
            : rows.FirstOrDefault(semester => semester.Status == SemesterStatus.Active) ?? rows.FirstOrDefault();

        if (selected is null)
        {
            return null;
        }

        var available = rows
            .Select(semester => new AnalyticsSemesterResponse(
                semester.Id,
                semester.AcademicYearId,
                semester.Name,
                semester.AcademicYearName,
                semester.StartDate,
                semester.EndDate,
                semester.Status.ToString()))
            .ToList();

        var selectedSemester = available.Single(semester => semester.Id == selected.Id);
        return new AnalyticsSemesterContext(selectedSemester, available);
    }

    /// <summary>
    /// Ghi chú: GetOverviewAsync đếm người dùng active, học sinh xếp lớp, lớp học và các hàng đợi nghiệp vụ.
    /// </summary>
    public async Task<AdminOverviewResponse> GetOverviewAsync(
        AnalyticsSemesterContext context,
        DateTime generatedAtUtc,
        CancellationToken cancellationToken)
    {
        var semester = context.SelectedSemester;
        var activeStudents = await dbContext.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.SemesterId == semester.Id &&
                                 enrollment.Status == EnrollmentStatus.Active &&
                                 enrollment.Student.Status == StudentStatus.Active)
            .Select(enrollment => enrollment.StudentId)
            .Distinct()
            .CountAsync(cancellationToken);

        var activeTeachers = await dbContext.Users.CountAsync(
            user => user.Role == UserRole.Teacher && user.IsActive,
            cancellationToken);
        var activeParents = await dbContext.Users.CountAsync(
            user => user.Role == UserRole.Parent && user.IsActive,
            cancellationToken);
        var activeClasses = await dbContext.ClassRooms.CountAsync(
            classRoom => classRoom.AcademicYearId == semester.AcademicYearId && classRoom.IsActive,
            cancellationToken);
        var activeSubjects = await dbContext.Subjects.CountAsync(subject => subject.IsActive, cancellationToken);
        var pendingProfileRequests = await dbContext.StudentProfileChangeRequests.CountAsync(
            request => request.Status == ProfileChangeRequestStatus.Pending,
            cancellationToken);
        var openReportRequests = await dbContext.ReportRequests.CountAsync(
            request => request.Status == ReportRequestStatus.Pending ||
                       request.Status == ReportRequestStatus.Reviewing ||
                       request.Status == ReportRequestStatus.Approved ||
                       request.Status == ReportRequestStatus.Generating,
            cancellationToken);
        var pendingOutboxMessages = await dbContext.OutboxMessages.CountAsync(
            message => message.ProcessedAtUtc == null,
            cancellationToken);
        var failedExternalSyncs = await dbContext.ExternalSyncRecords.CountAsync(
            record => record.Status == ExternalSyncStatus.FailedPermanent,
            cancellationToken);

        var roleRows = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.IsActive)
            .GroupBy(user => user.Role)
            .Select(group => new { Role = group.Key, Count = group.Count() })
            .OrderBy(row => row.Role)
            .ToListAsync(cancellationToken);
        var usersByRole = roleRows
            .Select(row => new UserRoleCountResponse(row.Role.ToString(), row.Count))
            .ToList();

        var gradeLevelRows = await dbContext.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.SemesterId == semester.Id &&
                                 enrollment.Status == EnrollmentStatus.Active &&
                                 enrollment.Student.Status == StudentStatus.Active)
            .GroupBy(enrollment => enrollment.ClassRoom.GradeLevel)
            .Select(group => new
            {
                GradeLevel = group.Key,
                StudentCount = group.Select(enrollment => enrollment.StudentId).Distinct().Count()
            })
            .OrderBy(row => row.GradeLevel)
            .ToListAsync(cancellationToken);
        var studentsByGradeLevel = gradeLevelRows
            .Select(row => new GradeLevelEnrollmentCountResponse(row.GradeLevel, row.StudentCount))
            .ToList();

        return new AdminOverviewResponse(
            semester,
            context.AvailableSemesters,
            generatedAtUtc,
            activeStudents,
            activeTeachers,
            activeParents,
            activeClasses,
            activeSubjects,
            pendingProfileRequests,
            openReportRequests,
            pendingOutboxMessages,
            failedExternalSyncs,
            usersByRole,
            studentsByGradeLevel);
    }

    /// <summary>
    /// Ghi chú: GetAcademicAnalyticsAsync chuẩn hóa GradeEntry về thang 10 và tổng hợp theo môn, lớp, khoảng điểm, trạng thái.
    /// </summary>
    public async Task<AdminAcademicAnalyticsResponse> GetAcademicAnalyticsAsync(
        AnalyticsSemesterResponse semester,
        DateTime generatedAtUtc,
        CancellationToken cancellationToken)
    {
        var allGrades = dbContext.GradeEntries
            .AsNoTracking()
            .Where(entry => entry.Component.SemesterId == semester.Id);
        var publishedGrades = allGrades.Where(entry =>
            (entry.Status == GradeStatus.Published || entry.Status == GradeStatus.Locked) &&
            entry.Component.MaxScore > 0);

        var totalGradeCount = await allGrades.CountAsync(cancellationToken);
        var publishedGradeCount = await publishedGrades.CountAsync(cancellationToken);
        var averageScore = publishedGradeCount == 0
            ? null
            : await publishedGrades.AverageAsync(
                entry => (decimal?)(entry.Score * 10m / entry.Component.MaxScore),
                cancellationToken);
        var passedGradeCount = await publishedGrades.CountAsync(
            entry => entry.Score * 10m >= entry.Component.MaxScore * 5m,
            cancellationToken);

        var distributionRows = await publishedGrades
            .GroupBy(entry => entry.Score * 10m < entry.Component.MaxScore * 3m
                ? 0
                : entry.Score * 10m < entry.Component.MaxScore * 5m
                    ? 1
                    : entry.Score * 10m < entry.Component.MaxScore * 6.5m
                        ? 2
                        : entry.Score * 10m < entry.Component.MaxScore * 8m
                            ? 3
                            : entry.Score * 10m < entry.Component.MaxScore * 9m
                                ? 4
                                : 5)
            .Select(group => new { Bucket = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var distributionCounts = distributionRows.ToDictionary(row => row.Bucket, row => row.Count);
        var gradeDistribution = new List<GradeDistributionBucketResponse>
        {
            new("Dưới 3 (Kém)", 0m, 3m, distributionCounts.GetValueOrDefault(0)),
            new("3 - dưới 5 (Yếu)", 3m, 5m, distributionCounts.GetValueOrDefault(1)),
            new("5 - dưới 6.5 (TB)", 5m, 6.5m, distributionCounts.GetValueOrDefault(2)),
            new("6.5 - dưới 8 (Khá)", 6.5m, 8m, distributionCounts.GetValueOrDefault(3)),
            new("8 - dưới 9 (Giỏi)", 8m, 9m, distributionCounts.GetValueOrDefault(4)),
            new("9 - 10 (Xuất sắc)", 9m, 10m, distributionCounts.GetValueOrDefault(5))
        };

        var subjectRows = await publishedGrades
            .GroupBy(entry => new { entry.Assignment.Subject.SubjectCode, entry.Assignment.Subject.Name })
            .Select(group => new
            {
                group.Key.SubjectCode,
                SubjectName = group.Key.Name,
                Average = group.Average(entry => entry.Score * 10m / entry.Component.MaxScore),
                Passed = group.Count(entry => entry.Score * 10m >= entry.Component.MaxScore * 5m),
                Total = group.Count()
            })
            .OrderBy(row => row.SubjectName)
            .ToListAsync(cancellationToken);
        var subjectPerformance = subjectRows
            .Select(row => new SubjectPerformanceResponse(
                row.SubjectCode,
                row.SubjectName,
                Round(row.Average),
                Percentage(row.Passed, row.Total),
                row.Total))
            .ToList();

        var classRows = await publishedGrades
            .GroupBy(entry => new
            {
                entry.Assignment.ClassRoom.ClassCode,
                entry.Assignment.ClassRoom.Name,
                entry.Assignment.ClassRoom.GradeLevel
            })
            .Select(group => new
            {
                group.Key.ClassCode,
                ClassName = group.Key.Name,
                group.Key.GradeLevel,
                Average = group.Average(entry => entry.Score * 10m / entry.Component.MaxScore),
                Passed = group.Count(entry => entry.Score * 10m >= entry.Component.MaxScore * 5m),
                Total = group.Count()
            })
            .OrderBy(row => row.ClassCode)
            .ToListAsync(cancellationToken);
        var classPerformance = classRows
            .Select(row => new ClassPerformanceResponse(
                row.ClassCode,
                row.ClassName,
                row.GradeLevel,
                Round(row.Average),
                Percentage(row.Passed, row.Total),
                row.Total))
            .ToList();

        var statusRows = await allGrades
            .GroupBy(entry => entry.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var statusCounts = statusRows.ToDictionary(row => row.Status, row => row.Count);
        var gradeStatuses = Enum.GetValues<GradeStatus>()
            .Select(status => new GradeStatusCountResponse(status.ToString(), statusCounts.GetValueOrDefault(status)))
            .ToList();

        return new AdminAcademicAnalyticsResponse(
            semester,
            generatedAtUtc,
            Round(averageScore),
            Percentage(passedGradeCount, publishedGradeCount),
            publishedGradeCount,
            totalGradeCount,
            gradeDistribution,
            subjectPerformance,
            classPerformance,
            gradeStatuses);
    }

    /// <summary>
    /// Ghi chú: GetDataQualityAsync đếm học sinh, lớp, giáo viên và assignment thiếu liên kết bắt buộc của học kỳ.
    /// </summary>
    public async Task<AdminDataQualityResponse> GetDataQualityAsync(
        AnalyticsSemesterResponse semester,
        DateTime generatedAtUtc,
        CancellationToken cancellationToken)
    {
        var studentsWithoutEnrollment = await dbContext.Students.CountAsync(
            student => student.Status == StudentStatus.Active &&
                       !dbContext.Enrollments.Any(enrollment =>
                           enrollment.StudentId == student.Id &&
                           enrollment.SemesterId == semester.Id &&
                           enrollment.Status == EnrollmentStatus.Active),
            cancellationToken);
        var studentsWithoutParent = await dbContext.Students.CountAsync(
            student => student.Status == StudentStatus.Active &&
                       !dbContext.ParentStudents.Any(link => link.StudentId == student.Id && link.IsActive),
            cancellationToken);
        var studentsWithoutLogin = await dbContext.Students.CountAsync(
            student => student.Status == StudentStatus.Active &&
                       (student.UserId == null || !dbContext.Users.Any(user => user.Id == student.UserId && user.IsActive)),
            cancellationToken);
        var classesWithoutHomeroom = await dbContext.ClassRooms.CountAsync(
            classRoom => classRoom.AcademicYearId == semester.AcademicYearId &&
                         classRoom.IsActive &&
                         !dbContext.HomeroomAssignments.Any(assignment =>
                             assignment.ClassRoomId == classRoom.Id && assignment.IsActive),
            cancellationToken);
        var teachersWithoutCapability = await dbContext.Users.CountAsync(
            teacher => teacher.Role == UserRole.Teacher &&
                       teacher.IsActive &&
                       !dbContext.TeacherSubjectCapabilities.Any(capability =>
                           capability.TeacherId == teacher.Id && capability.IsActive),
            cancellationToken);
        var classesWithoutPublishedTimetable = await dbContext.ClassRooms.CountAsync(
            classRoom => classRoom.AcademicYearId == semester.AcademicYearId &&
                         classRoom.IsActive &&
                         !dbContext.TimetableEntries.Any(entry =>
                             entry.ClassRoomId == classRoom.Id &&
                             entry.TimetableVersion.SemesterId == semester.Id &&
                             entry.TimetableVersion.Status == TimetableVersionStatus.Published),
            cancellationToken);
        var assignmentsWithoutGrades = await dbContext.TeachingAssignments.CountAsync(
            assignment => assignment.SemesterId == semester.Id &&
                          assignment.IsActive &&
                          !dbContext.GradeEntries.Any(entry => entry.AssignmentId == assignment.Id),
            cancellationToken);
        var classesOverCapacity = await dbContext.ClassRooms.CountAsync(
            classRoom => classRoom.AcademicYearId == semester.AcademicYearId &&
                         classRoom.IsActive &&
                         dbContext.Enrollments.Count(enrollment =>
                             enrollment.ClassRoomId == classRoom.Id &&
                             enrollment.SemesterId == semester.Id &&
                             enrollment.Status == EnrollmentStatus.Active) > classRoom.Capacity,
            cancellationToken);

        var issues = new List<DataQualityIssueResponse>
        {
            new("STUDENT_WITHOUT_ENROLLMENT", "Học sinh active chưa được xếp lớp", "Critical", studentsWithoutEnrollment),
            new("STUDENT_WITHOUT_PARENT", "Học sinh active chưa liên kết phụ huynh", "Warning", studentsWithoutParent),
            new("STUDENT_WITHOUT_LOGIN", "Học sinh active chưa có tài khoản đăng nhập", "Warning", studentsWithoutLogin),
            new("CLASS_WITHOUT_HOMEROOM", "Lớp active chưa có giáo viên chủ nhiệm", "Critical", classesWithoutHomeroom),
            new("TEACHER_WITHOUT_CAPABILITY", "Giáo viên active chưa khai báo năng lực môn", "Warning", teachersWithoutCapability),
            new("CLASS_WITHOUT_TIMETABLE", "Lớp active chưa có thời khóa biểu đã công bố", "Critical", classesWithoutPublishedTimetable),
            new("ASSIGNMENT_WITHOUT_GRADE", "Phân công giảng dạy chưa có dữ liệu điểm", "Warning", assignmentsWithoutGrades),
            new("CLASS_OVER_CAPACITY", "Lớp có số học sinh vượt sĩ số tối đa", "Critical", classesOverCapacity)
        };

        return new AdminDataQualityResponse(
            semester,
            generatedAtUtc,
            issues.Sum(issue => issue.Count),
            issues.Where(issue => issue.Severity == "Critical").Sum(issue => issue.Count),
            issues);
    }

    /// <summary>
    /// Ghi chú: Round làm tròn điểm hoặc tỷ lệ analytics đến hai chữ số thập phân.
    /// </summary>
    private static decimal? Round(decimal? value) =>
        value.HasValue ? decimal.Round(value.Value, 2, MidpointRounding.AwayFromZero) : null;

    /// <summary>
    /// Ghi chú: Percentage tính tỷ lệ phần trăm từ số bản ghi đạt trên tổng bản ghi.
    /// </summary>
    private static decimal? Percentage(int numerator, int denominator) =>
        denominator == 0 ? null : Round(numerator * 100m / denominator);
}
