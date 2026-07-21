using EduHub.Application.Common.Scheduling;
using EduHub.Application.Contracts.Scheduling;
using EduHub.Application.Interfaces.Repositories.Scheduling;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Scheduling;

/// <summary>
/// Ghi chú: SchedulingRepository dùng EF Core để truy cập chương trình, năng lực giáo viên, GVCN và thời khóa biểu.
/// </summary>
public sealed class SchedulingRepository(ApplicationDbContext dbContext) : ISchedulingRepository
{
    public Task<AcademicYear?> GetAcademicYearAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.AcademicYears.AsNoTracking().SingleOrDefaultAsync(year => year.Id == id, cancellationToken);

    public Task<Semester?> GetSemesterAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Semesters.Include(semester => semester.AcademicYear).SingleOrDefaultAsync(semester => semester.Id == id, cancellationToken);

    public Task<bool> CurriculumPlanExistsAsync(Guid academicYearId, int gradeLevel, CancellationToken cancellationToken) =>
        dbContext.CurriculumPlans.AnyAsync(plan => plan.AcademicYearId == academicYearId && plan.GradeLevel == gradeLevel, cancellationToken);

    /// <summary>
    /// Ghi chú: GetSubjectsAsync lấy các môn mà quản trị học vụ đưa vào quota chương trình.
    /// </summary>
    public async Task<IReadOnlyList<Subject>> GetSubjectsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        await dbContext.Subjects.AsNoTracking().Where(subject => ids.Contains(subject.Id) && subject.IsActive).ToListAsync(cancellationToken);

    public void AddCurriculumPlan(CurriculumPlan plan) => dbContext.CurriculumPlans.Add(plan);

    /// <summary>
    /// Ghi chú: ListCurriculumPlansAsync trả chương trình và quota môn đã cấu hình theo năm hoặc khối.
    /// </summary>
    public async Task<IReadOnlyList<CurriculumPlanResponse>> ListCurriculumPlansAsync(Guid? academicYearId, int? gradeLevel, CancellationToken cancellationToken)
    {
        var query = dbContext.CurriculumPlans.AsNoTracking().Include(plan => plan.SubjectQuotas).ThenInclude(quota => quota.Subject).AsQueryable();
        if (academicYearId.HasValue) query = query.Where(plan => plan.AcademicYearId == academicYearId.Value);
        if (gradeLevel.HasValue) query = query.Where(plan => plan.GradeLevel == gradeLevel.Value);
        var plans = await query.OrderBy(plan => plan.GradeLevel).ToListAsync(cancellationToken);
        return plans.Select(ToResponse).ToList();
    }

    /// <summary>
    /// Ghi chú: GetCurriculumPlansForYearAsync tải entity chương trình và quota để service sinh lịch.
    /// </summary>
    public async Task<IReadOnlyList<CurriculumPlan>> GetCurriculumPlansForYearAsync(Guid academicYearId, CancellationToken cancellationToken) =>
        await dbContext.CurriculumPlans.Include(plan => plan.SubjectQuotas).ThenInclude(quota => quota.Subject)
            .Where(plan => plan.AcademicYearId == academicYearId && plan.IsActive).ToListAsync(cancellationToken);

    public Task<User?> GetActiveTeacherAsync(Guid teacherId, CancellationToken cancellationToken) =>
        dbContext.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Id == teacherId && user.Role == UserRole.Teacher && user.IsActive, cancellationToken);

    public Task<Subject?> GetActiveSubjectAsync(Guid subjectId, CancellationToken cancellationToken) =>
        dbContext.Subjects.AsNoTracking().SingleOrDefaultAsync(subject => subject.Id == subjectId && subject.IsActive, cancellationToken);

    public Task<bool> TeacherCapabilityExistsAsync(Guid teacherId, Guid subjectId, CancellationToken cancellationToken) =>
        dbContext.TeacherSubjectCapabilities.AnyAsync(capability => capability.TeacherId == teacherId && capability.SubjectId == subjectId, cancellationToken);

    public Task<int> CountCapabilitiesAsync(Guid teacherId, TeacherSubjectPriority priority, CancellationToken cancellationToken) =>
        dbContext.TeacherSubjectCapabilities.CountAsync(capability => capability.TeacherId == teacherId && capability.Priority == priority && capability.IsActive, cancellationToken);

    public void AddTeacherCapability(TeacherSubjectCapability capability) => dbContext.TeacherSubjectCapabilities.Add(capability);

    /// <summary>
    /// Ghi chú: ListTeacherCapabilitiesAsync trả năng lực giảng dạy kèm tên giáo viên và môn học.
    /// </summary>
    public async Task<IReadOnlyList<TeacherCapabilityResponse>> ListTeacherCapabilitiesAsync(Guid? teacherId, Guid? subjectId, CancellationToken cancellationToken)
    {
        var query = dbContext.TeacherSubjectCapabilities.AsNoTracking().Include(capability => capability.Teacher).Include(capability => capability.Subject).AsQueryable();
        if (teacherId.HasValue) query = query.Where(capability => capability.TeacherId == teacherId.Value);
        if (subjectId.HasValue) query = query.Where(capability => capability.SubjectId == subjectId.Value);
        return await query.OrderBy(capability => capability.Teacher.FullName).ThenBy(capability => capability.Priority)
            .Select(capability => new TeacherCapabilityResponse(
                capability.Id,
                capability.TeacherId,
                capability.Teacher.FullName,
                capability.SubjectId,
                capability.Subject.SubjectCode,
                capability.Subject.Name,
                capability.Priority.ToString(),
                capability.MaxPeriodsPerWeek,
                capability.IsActive))
            .ToListAsync(cancellationToken);
    }

    public Task<ClassRoom?> GetClassRoomAsync(Guid classRoomId, CancellationToken cancellationToken) =>
        dbContext.ClassRooms.AsNoTracking().SingleOrDefaultAsync(classRoom => classRoom.Id == classRoomId && classRoom.IsActive, cancellationToken);

    /// <summary>
    /// Ghi chú: GetActiveHomeroomForClassAsync tải phân công GVCN active kèm lớp và giáo viên để service thay thế an toàn.
    /// </summary>
    public Task<HomeroomAssignment?> GetActiveHomeroomForClassAsync(Guid classRoomId, CancellationToken cancellationToken) =>
        dbContext.HomeroomAssignments
            .Include(assignment => assignment.ClassRoom)
            .Include(assignment => assignment.Teacher)
            .SingleOrDefaultAsync(assignment => assignment.ClassRoomId == classRoomId && assignment.IsActive, cancellationToken);

    public Task<bool> HasActiveHomeroomForClassAsync(Guid classRoomId, CancellationToken cancellationToken) =>
        dbContext.HomeroomAssignments.AnyAsync(assignment => assignment.ClassRoomId == classRoomId && assignment.IsActive, cancellationToken);

    public Task<bool> HasActiveHomeroomForTeacherAsync(Guid teacherId, CancellationToken cancellationToken) =>
        dbContext.HomeroomAssignments.AnyAsync(assignment => assignment.TeacherId == teacherId && assignment.IsActive, cancellationToken);

    public Task<bool> HasActiveTeachingAssignmentAsync(Guid classRoomId, Guid teacherId, CancellationToken cancellationToken) =>
        dbContext.TeachingAssignments.AnyAsync(assignment => assignment.ClassRoomId == classRoomId && assignment.TeacherId == teacherId && assignment.IsActive, cancellationToken);

    public void AddHomeroomAssignment(HomeroomAssignment assignment) => dbContext.HomeroomAssignments.Add(assignment);

    /// <summary>
    /// Ghi chú: ListHomeroomAssignmentsAsync trả lớp và giáo viên chủ nhiệm theo năm học.
    /// </summary>
    public async Task<IReadOnlyList<HomeroomAssignmentResponse>> ListHomeroomAssignmentsAsync(Guid? academicYearId, CancellationToken cancellationToken)
    {
        var query = dbContext.HomeroomAssignments.AsNoTracking().Include(assignment => assignment.ClassRoom).Include(assignment => assignment.Teacher)
            .Where(assignment => assignment.IsActive).AsQueryable();
        if (academicYearId.HasValue) query = query.Where(assignment => assignment.ClassRoom.AcademicYearId == academicYearId.Value);
        return await query.OrderBy(assignment => assignment.ClassRoom.ClassCode)
            .Select(assignment => new HomeroomAssignmentResponse(
                assignment.Id,
                assignment.ClassRoomId,
                assignment.ClassRoom.ClassCode,
                assignment.ClassRoom.Name,
                assignment.TeacherId,
                assignment.Teacher.FullName,
                assignment.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClassRoom>> ListClassRoomsAsync(Guid academicYearId, CancellationToken cancellationToken) =>
        await dbContext.ClassRooms.AsNoTracking().Where(classRoom => classRoom.AcademicYearId == academicYearId && classRoom.IsActive)
            .OrderBy(classRoom => classRoom.ClassCode).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> ListActiveTeachersAsync(CancellationToken cancellationToken) =>
        await dbContext.Users.AsNoTracking().Where(user => user.Role == UserRole.Teacher && user.IsActive).OrderBy(user => user.FullName).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TeacherSubjectCapability>> ListActiveCapabilitiesAsync(CancellationToken cancellationToken) =>
        await dbContext.TeacherSubjectCapabilities.AsNoTracking().Include(capability => capability.Teacher).Include(capability => capability.Subject)
            .Where(capability => capability.IsActive && capability.Teacher.IsActive).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TeachingAssignment>> ListTeachingAssignmentsAsync(Guid semesterId, CancellationToken cancellationToken) =>
        await dbContext.TeachingAssignments.Include(assignment => assignment.ClassRoom).Include(assignment => assignment.Subject).Include(assignment => assignment.Teacher)
            .Where(assignment => assignment.SemesterId == semesterId && assignment.IsActive).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<HomeroomAssignment>> ListActiveHomeroomAssignmentsAsync(Guid academicYearId, CancellationToken cancellationToken) =>
        await dbContext.HomeroomAssignments.Include(assignment => assignment.ClassRoom).Include(assignment => assignment.Teacher)
            .Where(assignment => assignment.IsActive && assignment.ClassRoom.AcademicYearId == academicYearId).ToListAsync(cancellationToken);

    public void AddTeachingAssignment(TeachingAssignment assignment) => dbContext.TeachingAssignments.Add(assignment);

    public void AddTimetableVersion(TimetableVersion version) => dbContext.TimetableVersions.Add(version);

    /// <summary>
    /// Ghi chú: ListTimetableVersionsAsync trả lịch sử sinh và công bố thời khóa biểu của học kỳ.
    /// </summary>
    public async Task<IReadOnlyList<TimetableVersionResponse>> ListTimetableVersionsAsync(Guid semesterId, CancellationToken cancellationToken) =>
        await dbContext.TimetableVersions.AsNoTracking().Where(version => version.SemesterId == semesterId)
            .OrderByDescending(version => version.GeneratedAtUtc)
            .Select(version => new TimetableVersionResponse(
                version.Id,
                version.SemesterId,
                version.Semester.Name,
                version.Name,
                version.Status.ToString(),
                version.GeneratedAtUtc,
                version.PublishedAtUtc,
                version.Entries.Count))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Ghi chú: GetPublishedTimetableVersionAsync lấy phiên bản hiện hành của học kỳ để người dùng mở lịch.
    /// </summary>
    public Task<TimetableVersionResponse?> GetPublishedTimetableVersionAsync(Guid semesterId, CancellationToken cancellationToken) =>
        dbContext.TimetableVersions.AsNoTracking()
            .Where(version => version.SemesterId == semesterId && version.Status == TimetableVersionStatus.Published)
            .OrderByDescending(version => version.PublishedAtUtc)
            .Select(version => new TimetableVersionResponse(
                version.Id,
                version.SemesterId,
                version.Semester.Name,
                version.Name,
                version.Status.ToString(),
                version.GeneratedAtUtc,
                version.PublishedAtUtc,
                version.Entries.Count))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<TimetableVersion?> GetTimetableVersionAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.TimetableVersions.Include(version => version.Semester).Include(version => version.Entries).SingleOrDefaultAsync(version => version.Id == id, cancellationToken);

    /// <summary>
    /// Ghi chú: ListTimetableEntriesAsync trả các tiết trong phiên bản, kèm lớp, môn và giáo viên để hiển thị dạng lưới.
    /// </summary>
    public async Task<IReadOnlyList<TimetableEntryResponse>> ListTimetableEntriesAsync(Guid timetableVersionId, Guid? classRoomId, int? weekNumber, CancellationToken cancellationToken)
    {
        var query = dbContext.TimetableEntries.AsNoTracking()
            .Include(entry => entry.TimetableVersion).ThenInclude(version => version.Semester)
            .Include(entry => entry.ClassRoom)
            .Include(entry => entry.Subject)
            .Include(entry => entry.Teacher)
            .Where(entry => entry.TimetableVersionId == timetableVersionId);
        if (classRoomId.HasValue) query = query.Where(entry => entry.ClassRoomId == classRoomId.Value);
        if (weekNumber.HasValue) query = query.Where(entry => entry.WeekNumber == weekNumber.Value);
        var entries = await query.OrderBy(entry => entry.ClassRoom.ClassCode).ThenBy(entry => entry.WeekNumber)
            .ThenBy(entry => entry.DayOfWeek).ThenBy(entry => entry.Session).ThenBy(entry => entry.PeriodNumber)
            .ToListAsync(cancellationToken);
        return entries.Select(entry =>
        {
            var week = TimetableCalendar.GetWeekDates(entry.TimetableVersion.Semester.StartDate, entry.WeekNumber);
            var period = TimetableCalendar.GetPeriodTimes(entry.Session, entry.PeriodNumber);
            return new TimetableEntryResponse(
                entry.Id,
                entry.TimetableVersionId,
                entry.ClassRoomId,
                entry.ClassRoom.ClassCode,
                entry.ClassRoom.Name,
                entry.SubjectId,
                entry.Subject.SubjectCode,
                entry.Subject.Name,
                entry.TeacherId,
                entry.Teacher == null ? null : entry.Teacher.FullName,
                entry.WeekNumber,
                week.StartDate,
                week.EndDate,
                entry.DayOfWeek,
                entry.Session.ToString(),
                entry.PeriodNumber,
                period.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
                period.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
                entry.Kind.ToString(),
                entry.CountsTowardQuota,
                entry.IsLocked,
                entry.Note);
        }).ToList();
    }

    public Task<TimetableEntry?> GetTimetableEntryAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.TimetableEntries.Include(entry => entry.TimetableVersion).ThenInclude(version => version.Semester)
            .Include(entry => entry.ClassRoom).Include(entry => entry.Subject).Include(entry => entry.Teacher)
            .SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);

    /// <summary>
    /// Ghi chú: ListTimetableEntriesForClassSubjectAsync tải có tracking mọi tiết của lớp-môn để service đổi cùng một giáo viên.
    /// </summary>
    public async Task<IReadOnlyList<TimetableEntry>> ListTimetableEntriesForClassSubjectAsync(Guid timetableVersionId, Guid classRoomId, Guid subjectId, CancellationToken cancellationToken) =>
        await dbContext.TimetableEntries
            .Where(entry => entry.TimetableVersionId == timetableVersionId && entry.ClassRoomId == classRoomId && entry.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Ghi chú: ListTeacherTimetableEntriesAsync đọc lịch giáo viên trong bản nháp để so slot, tải ngày và tải tuần.
    /// </summary>
    public async Task<IReadOnlyList<TimetableEntry>> ListTeacherTimetableEntriesAsync(Guid timetableVersionId, Guid teacherId, CancellationToken cancellationToken) =>
        await dbContext.TimetableEntries.AsNoTracking()
            .Where(entry => entry.TimetableVersionId == timetableVersionId && entry.TeacherId == teacherId)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Ghi chú: ListActiveTeachingAssignmentsForScopeAsync tải assignment lớp-môn-học kỳ có tracking để đổi TeacherId nhưng giữ assignment ID.
    /// </summary>
    public async Task<IReadOnlyList<TeachingAssignment>> ListActiveTeachingAssignmentsForScopeAsync(Guid classRoomId, Guid subjectId, Guid semesterId, CancellationToken cancellationToken) =>
        await dbContext.TeachingAssignments
            .Where(assignment => assignment.ClassRoomId == classRoomId && assignment.SubjectId == subjectId && assignment.SemesterId == semesterId && assignment.IsActive)
            .ToListAsync(cancellationToken);

    public Task<TimetableEntry?> GetClassSlotEntryAsync(Guid timetableVersionId, Guid classRoomId, int weekNumber, int dayOfWeek, TimetableSession session, int periodNumber, CancellationToken cancellationToken) =>
        dbContext.TimetableEntries.Include(entry => entry.TimetableVersion).ThenInclude(version => version.Semester)
            .Include(entry => entry.ClassRoom).Include(entry => entry.Subject).Include(entry => entry.Teacher)
            .SingleOrDefaultAsync(entry => entry.TimetableVersionId == timetableVersionId && entry.ClassRoomId == classRoomId &&
                entry.WeekNumber == weekNumber && entry.DayOfWeek == dayOfWeek && entry.Session == session && entry.PeriodNumber == periodNumber,
                cancellationToken);

    /// <summary>
    /// Ghi chú: GetQuotaForClassSubjectAsync lấy rule tiết đôi và số tiết tối đa/ngày của đúng khối lớp.
    /// </summary>
    public Task<CurriculumSubjectQuota?> GetQuotaForClassSubjectAsync(Guid classRoomId, Guid subjectId, CancellationToken cancellationToken) =>
        (from classRoom in dbContext.ClassRooms
         join plan in dbContext.CurriculumPlans on new { classRoom.AcademicYearId, classRoom.GradeLevel } equals new { plan.AcademicYearId, plan.GradeLevel }
         join quota in dbContext.CurriculumSubjectQuotas on plan.Id equals quota.CurriculumPlanId
         where classRoom.Id == classRoomId && quota.SubjectId == subjectId
         select quota).AsNoTracking().SingleOrDefaultAsync(cancellationToken);

    public Task<TeacherSubjectCapability?> GetActiveTeacherCapabilityAsync(Guid teacherId, Guid subjectId, CancellationToken cancellationToken) =>
        dbContext.TeacherSubjectCapabilities.Include(capability => capability.Teacher)
            .SingleOrDefaultAsync(capability => capability.TeacherId == teacherId && capability.SubjectId == subjectId && capability.IsActive && capability.Teacher.IsActive, cancellationToken);

    public Task<bool> TeacherSlotConflictAsync(Guid timetableVersionId, Guid teacherId, int weekNumber, int dayOfWeek, TimetableSession session, int periodNumber, IReadOnlyCollection<Guid> excludedEntryIds, CancellationToken cancellationToken) =>
        dbContext.TimetableEntries.AnyAsync(candidate =>
            !excludedEntryIds.Contains(candidate.Id) &&
            candidate.TimetableVersionId == timetableVersionId &&
            candidate.TeacherId == teacherId &&
            candidate.WeekNumber == weekNumber &&
            candidate.DayOfWeek == dayOfWeek &&
            candidate.Session == session &&
            candidate.PeriodNumber == periodNumber,
            cancellationToken);

    public Task<int> CountTeacherPeriodsAsync(Guid timetableVersionId, Guid teacherId, int weekNumber, CancellationToken cancellationToken) =>
        dbContext.TimetableEntries.CountAsync(entry => entry.TimetableVersionId == timetableVersionId && entry.TeacherId == teacherId && entry.WeekNumber == weekNumber, cancellationToken);

    public Task<bool> IsHomeroomTeacherForClassAsync(Guid teacherId, Guid classRoomId, CancellationToken cancellationToken) =>
        dbContext.HomeroomAssignments.AnyAsync(assignment => assignment.TeacherId == teacherId && assignment.ClassRoomId == classRoomId && assignment.IsActive, cancellationToken);

    public async Task<IReadOnlyList<TimetableVersion>> ListPublishedTimetableVersionsAsync(Guid semesterId, Guid exceptId, CancellationToken cancellationToken) =>
        await dbContext.TimetableVersions.Where(version => version.SemesterId == semesterId && version.Id != exceptId && version.Status == TimetableVersionStatus.Published)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Ghi chú: CanViewClassTimetableAsync kiểm tra giáo viên dạy lớp, học sinh thuộc lớp hoặc phụ huynh có con trong lớp.
    /// </summary>
    public Task<bool> CanViewClassTimetableAsync(Guid userId, UserRole role, Guid classRoomId, Guid semesterId, CancellationToken cancellationToken) =>
        role switch
        {
            UserRole.Teacher => dbContext.TeachingAssignments.AnyAsync(
                assignment => assignment.TeacherId == userId && assignment.ClassRoomId == classRoomId && assignment.SemesterId == semesterId && assignment.IsActive,
                cancellationToken),
            UserRole.Student => dbContext.Enrollments.AnyAsync(
                enrollment => enrollment.ClassRoomId == classRoomId && enrollment.SemesterId == semesterId &&
                    enrollment.Student.UserId == userId && enrollment.Status == EnrollmentStatus.Active,
                cancellationToken),
            UserRole.Parent => dbContext.Enrollments.AnyAsync(
                enrollment => enrollment.ClassRoomId == classRoomId && enrollment.SemesterId == semesterId &&
                    enrollment.Status == EnrollmentStatus.Active && dbContext.ParentStudents.Any(link =>
                        link.ParentUserId == userId && link.StudentId == enrollment.StudentId && link.IsActive),
                cancellationToken),
            _ => Task.FromResult(false)
        };

    /// <summary>
    /// Ghi chú: ToResponse chuyển entity chương trình và quota đã include thành response cho API.
    /// </summary>
    private static CurriculumPlanResponse ToResponse(CurriculumPlan plan) =>
        new(
            plan.Id,
            plan.AcademicYearId,
            plan.GradeLevel,
            plan.Name,
            plan.TotalWeeks,
            plan.Semester1Weeks,
            plan.Semester2Weeks,
            plan.SubjectQuotas.Sum(quota => quota.AnnualPeriods),
            plan.IsActive,
            plan.SubjectQuotas.OrderBy(quota => quota.Subject.SubjectCode).Select(quota => new CurriculumSubjectQuotaResponse(
                quota.Id,
                quota.SubjectId,
                quota.Subject.SubjectCode,
                quota.Subject.Name,
                quota.Kind.ToString(),
                quota.AnnualPeriods,
                quota.Semester1Periods,
                quota.Semester2Periods,
                quota.CanDoublePeriod,
                quota.MaxPeriodsPerDay,
                quota.IncludesHomeroom,
                quota.PreferredSession?.ToString())).ToList());
}
