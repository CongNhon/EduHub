using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Reports;
using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Options;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EduHub.Infrastructure.Seed;

/// <summary>
/// Ghi chú: DevelopmentScenarioSeeder bổ sung dữ liệu cho các màn hình điểm, nhận xét, báo cáo, hồ sơ và thời khóa biểu.
/// </summary>
public static class DevelopmentScenarioSeeder
{
    private static readonly string[] GradedSubjectCodes = ["MATH", "LIT", "ENG", "PHYS"];
    private static readonly string[] TimetableSubjectCodes =
    [
        "MATH", "MATH", "LIT", "ENG", "PHYS",
        "LIT", "ENG", "ENG", "CHEM", "PE",
        "MATH", "HIST", "INFO", "GEO",
        "LIT", "PHYS", "PHYS", "CHEM", "CHEM",
        "INFO", "INFO", "GEO", "GEO", "EXP",
        "PE", "DEF", "EXP", "LOCAL", "HOMEROOM"
    ];

    /// <summary>
    /// Ghi chú: SeedDevelopmentScenariosAsync chạy sau school seed để mọi khóa ngoại học sinh, lớp, giáo viên và học kỳ đã tồn tại.
    /// </summary>
    public static async Task SeedDevelopmentScenariosAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<DevelopmentSeedOptions>>().Value;
        if (!options.Enabled) return;

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cancellationToken = CancellationToken.None;
        var semester = await dbContext.Semesters
            .OrderByDescending(item => item.StartDate)
            .FirstAsync(cancellationToken);
        var classes = await dbContext.ClassRooms.OrderBy(item => item.ClassCode).ToListAsync(cancellationToken);
        var subjects = await dbContext.Subjects.ToDictionaryAsync(item => item.NormalizedSubjectCode, cancellationToken);
        var normalizedAcademicEmail = DevelopmentAcademicSeeder.AcademicEmail.ToUpperInvariant();
        var academicAdmin = await dbContext.Users.SingleAsync(
            item => item.NormalizedEmail == normalizedAcademicEmail,
            cancellationToken);

        var assignments = await EnsureTeachingAssignmentsAsync(dbContext, semester, classes, subjects, cancellationToken);
        var components = await EnsureGradeComponentsAsync(dbContext, semester, subjects, cancellationToken);
        await EnsureGradesAndRemarksAsync(dbContext, semester, classes, assignments, components, cancellationToken);
        await EnsureProfileRequestsAsync(dbContext, academicAdmin.Id, cancellationToken);
        await EnsureReportRequestsAsync(dbContext, academicAdmin.Id, semester.Id, cancellationToken);
        await EnsurePublishedTimetableAsync(dbContext, academicAdmin.Id, semester, classes, subjects, assignments, cancellationToken);
    }

    /// <summary>
    /// Ghi chú: EnsureTeachingAssignmentsAsync phân công giáo viên đủ năng lực cho các môn xuất hiện trong điểm và TKB mẫu.
    /// </summary>
    private static async Task<List<TeachingAssignment>> EnsureTeachingAssignmentsAsync(
        ApplicationDbContext dbContext,
        Semester semester,
        List<ClassRoom> classes,
        Dictionary<string, Subject> subjects,
        CancellationToken cancellationToken)
    {
        var subjectCodes = TimetableSubjectCodes.Where(code => code != "HOMEROOM").Distinct().ToList();
        var subjectIds = subjectCodes.Select(code => subjects[code].Id).ToHashSet();
        var capabilities = await dbContext.TeacherSubjectCapabilities
            .Where(item => subjectIds.Contains(item.SubjectId))
            .OrderBy(item => item.Priority)
            .ThenBy(item => item.TeacherId)
            .ToListAsync(cancellationToken);
        var assignments = await dbContext.TeachingAssignments
            .Where(item => item.SemesterId == semester.Id && item.IsActive)
            .ToListAsync(cancellationToken);

        for (var classIndex = 0; classIndex < classes.Count; classIndex++)
        {
            for (var subjectIndex = 0; subjectIndex < subjectCodes.Count; subjectIndex++)
            {
                var subject = subjects[subjectCodes[subjectIndex]];
                if (assignments.Any(item => item.ClassRoomId == classes[classIndex].Id && item.SubjectId == subject.Id)) continue;
                var candidates = capabilities.Where(item => item.SubjectId == subject.Id).ToList();
                if (candidates.Count == 0) continue;
                var teacher = candidates[(classIndex + subjectIndex) % candidates.Count];
                var assignment = new TeachingAssignment(classes[classIndex].Id, subject.Id, teacher.TeacherId, semester.Id, DateTime.UtcNow.AddDays(-20));
                dbContext.TeachingAssignments.Add(assignment);
                assignments.Add(assignment);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return assignments;
    }

    /// <summary>
    /// Ghi chú: EnsureGradeComponentsAsync tạo cấu hình Thường xuyên, Giữa kỳ và Cuối kỳ cho bốn môn có dữ liệu điểm mẫu.
    /// </summary>
    private static async Task<Dictionary<Guid, List<GradeComponent>>> EnsureGradeComponentsAsync(
        ApplicationDbContext dbContext,
        Semester semester,
        Dictionary<string, Subject> subjects,
        CancellationToken cancellationToken)
    {
        var subjectIds = GradedSubjectCodes.Select(code => subjects[code].Id).ToHashSet();
        var components = await dbContext.GradeComponents
            .Where(item => item.SemesterId == semester.Id && subjectIds.Contains(item.SubjectId) && item.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var subjectId in subjectIds)
        {
            if (components.Any(item => item.SubjectId == subjectId)) continue;
            var created = new[]
            {
                new GradeComponent(subjectId, semester.Id, "Thường xuyên", "THUONG XUYEN", .2m, 10m, 1, true, true, 1),
                new GradeComponent(subjectId, semester.Id, "Giữa kỳ", "GIUA KY", .3m, 10m, 2, true, true, 1),
                new GradeComponent(subjectId, semester.Id, "Cuối kỳ", "CUOI KY", .5m, 10m, 3, true, true, 1)
            };
            dbContext.GradeComponents.AddRange(created);
            components.AddRange(created);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return components.GroupBy(item => item.SubjectId).ToDictionary(group => group.Key, group => group.OrderBy(item => item.DisplayOrder).ToList());
    }

    /// <summary>
    /// Ghi chú: EnsureGradesAndRemarksAsync tạo phổ điểm 0-10, đủ Draft, Submitted, Published, Locked và nhận xét công bố/nháp.
    /// </summary>
    private static async Task EnsureGradesAndRemarksAsync(
        ApplicationDbContext dbContext,
        Semester semester,
        List<ClassRoom> classes,
        IReadOnlyList<TeachingAssignment> assignments,
        Dictionary<Guid, List<GradeComponent>> components,
        CancellationToken cancellationToken)
    {
        var enrollments = await dbContext.Enrollments
            .Where(item => item.SemesterId == semester.Id && item.Status == EnrollmentStatus.Active)
            .OrderBy(item => item.StudentId)
            .ToListAsync(cancellationToken);
        var existingGrades = (await dbContext.GradeEntries
                .Select(item => new { item.StudentId, item.AssignmentId, item.ComponentId })
                .ToListAsync(cancellationToken))
            .Select(item => (item.StudentId, item.AssignmentId, item.ComponentId))
            .ToHashSet();
        var existingRemarks = (await dbContext.StudentRemarks
                .Select(item => new { item.StudentId, item.AssignmentId })
                .ToListAsync(cancellationToken))
            .Select(item => (item.StudentId, item.AssignmentId))
            .ToHashSet();
        var now = DateTime.UtcNow;

        for (var classIndex = 0; classIndex < classes.Count; classIndex++)
        {
            var classRoom = classes[classIndex];
            var studentIds = enrollments.Where(item => item.ClassRoomId == classRoom.Id).Select(item => item.StudentId).Take(12).ToList();
            var classAssignments = assignments
                .Where(item => item.ClassRoomId == classRoom.Id && components.ContainsKey(item.SubjectId))
                .OrderBy(item => item.SubjectId)
                .ToList();

            for (var assignmentIndex = 0; assignmentIndex < classAssignments.Count; assignmentIndex++)
            {
                var assignment = classAssignments[assignmentIndex];
                for (var studentIndex = 0; studentIndex < studentIds.Count; studentIndex++)
                {
                    for (var componentIndex = 0; componentIndex < components[assignment.SubjectId].Count; componentIndex++)
                    {
                        var component = components[assignment.SubjectId][componentIndex];
                        if (!existingGrades.Add((studentIds[studentIndex], assignment.Id, component.Id))) continue;
                        var rawScore = 3.2m
                            + studentIndex % 7 * .8m
                            + assignmentIndex % 4 * .35m
                            + componentIndex * .25m
                            + (classIndex % 3 - 1) * .45m;
                        var score = Math.Min(10m, Math.Max(0m, rawScore));
                        var grade = new GradeEntry(studentIds[studentIndex], assignment.Id, component.Id, score, assignment.TeacherId, now.AddDays(-12));
                        switch ((studentIndex * 3 + assignmentIndex * 2 + componentIndex + classIndex) % 7)
                        {
                            case 1:
                            case 2:
                                grade.Submit(now.AddDays(-10));
                                break;
                            case 3:
                            case 4:
                            case 5:
                                grade.Submit(now.AddDays(-10));
                                grade.Publish(now.AddDays(-8));
                                break;
                            case 6:
                                grade.Submit(now.AddDays(-10));
                                grade.Publish(now.AddDays(-8));
                                grade.Lock(now.AddDays(-5));
                                break;
                        }

                        dbContext.GradeEntries.Add(grade);
                    }

                    if (studentIndex < 4 && existingRemarks.Add((studentIds[studentIndex], assignment.Id)))
                    {
                        var remark = new StudentRemark(studentIds[studentIndex], assignment.Id, assignment.TeacherId, RemarkContent(studentIndex), now.AddDays(-7));
                        if (studentIndex % 2 == 0) remark.Publish(now.AddDays(-6));
                        dbContext.StudentRemarks.Add(remark);
                    }
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ghi chú: EnsureProfileRequestsAsync tạo ba yêu cầu sửa hồ sơ ở trạng thái Pending, Approved và Rejected.
    /// </summary>
    private static async Task EnsureProfileRequestsAsync(ApplicationDbContext dbContext, Guid reviewerId, CancellationToken cancellationToken)
    {
        if (await dbContext.StudentProfileChangeRequests.AnyAsync(item => item.Reason.StartsWith("[SEED]"), cancellationToken)) return;
        var students = await dbContext.Students.Where(item => item.UserId != null).OrderBy(item => item.StudentCode).Take(3).ToListAsync(cancellationToken);
        if (students.Count < 3) return;
        var now = DateTime.UtcNow;

        var pending = CreateProfileRequest(students[0], "[SEED] Cập nhật số điện thoại liên hệ", "seed/profile/pending.png");
        var approved = CreateProfileRequest(students[1], "[SEED] Điều chỉnh địa chỉ thường trú", "seed/profile/approved.png");
        approved.Approve(reviewerId, "Bằng chứng hợp lệ.", now.AddDays(-3));
        var rejected = CreateProfileRequest(students[2], "[SEED] Yêu cầu thiếu giấy tờ xác nhận", "seed/profile/rejected.png");
        rejected.Reject(reviewerId, "Ảnh bằng chứng chưa thể hiện thông tin cần sửa.", now.AddDays(-2));
        dbContext.StudentProfileChangeRequests.AddRange(pending, approved, rejected);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ghi chú: EnsureReportRequestsAsync tạo inbox báo cáo phụ huynh gồm Pending, Approved và Rejected cho ba gia đình khác nhau.
    /// </summary>
    private static async Task EnsureReportRequestsAsync(ApplicationDbContext dbContext, Guid reviewerId, Guid semesterId, CancellationToken cancellationToken)
    {
        if (await dbContext.ReportRequests.AnyAsync(item => item.Purpose.StartsWith("[SEED]"), cancellationToken)) return;
        var links = await dbContext.ParentStudents.Where(item => item.IsActive).OrderBy(item => item.StudentId).Take(3).ToListAsync(cancellationToken);
        if (links.Count < 3) return;
        var now = DateTime.UtcNow;

        var pending = new ReportRequest(links[0].ParentUserId, links[0].StudentId, semesterId, "[SEED] Xin bảng điểm để theo dõi học tập", now.AddDays(-3));
        var approved = new ReportRequest(links[1].ParentUserId, links[1].StudentId, semesterId, "[SEED] Bổ sung hồ sơ học bổng", now.AddDays(-5));
        approved.Approve(reviewerId, "Đủ điều kiện tạo báo cáo.", now.AddDays(-4));
        var rejected = new ReportRequest(links[2].ParentUserId, links[2].StudentId, semesterId, "[SEED] Yêu cầu báo cáo chưa nêu rõ mục đích", now.AddDays(-6));
        rejected.Reject(reviewerId, "Vui lòng ghi rõ đơn vị tiếp nhận báo cáo.", now.AddDays(-5));
        dbContext.ReportRequests.AddRange(pending, approved, rejected);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ghi chú: EnsurePublishedTimetableAsync tạo TKB tuần 1 đủ 29 tiết cho mọi lớp và công bố phiên bản để các role xem được.
    /// </summary>
    private static async Task EnsurePublishedTimetableAsync(
        ApplicationDbContext dbContext,
        Guid createdByUserId,
        Semester semester,
        List<ClassRoom> classes,
        Dictionary<string, Subject> subjects,
        IReadOnlyList<TeachingAssignment> assignments,
        CancellationToken cancellationToken)
    {
        const string versionName = "[SEED] Thời khóa biểu tuần 1";
        if (await dbContext.TimetableVersions.AnyAsync(item => item.SemesterId == semester.Id && item.Name == versionName, cancellationToken)) return;
        var now = DateTime.UtcNow;
        var version = new TimetableVersion(semester.Id, versionName, createdByUserId, now.AddDays(-2));
        dbContext.TimetableVersions.Add(version);

        for (var classIndex = 0; classIndex < classes.Count; classIndex++)
        {
            for (var slotIndex = 0; slotIndex < TimetableSubjectCodes.Length; slotIndex++)
            {
                var subjectCode = TimetableSubjectCodes[slotIndex];
                var subject = subjects[subjectCode];
                var dayAndPeriod = ResolveSlot(slotIndex);
                var teacherId = classIndex == 0
                    ? assignments.FirstOrDefault(item => item.ClassRoomId == classes[classIndex].Id && item.SubjectId == subject.Id)?.TeacherId
                    : null;
                version.Entries.Add(new TimetableEntry(
                    version.Id,
                    classes[classIndex].Id,
                    subject.Id,
                    teacherId,
                    1,
                    dayAndPeriod.Day,
                    TimetableSession.Morning,
                    dayAndPeriod.Period,
                    TimetableEntryKind.Curriculum,
                    true,
                    subjectCode == "HOMEROOM",
                    subjectCode == "HOMEROOM" ? "Sinh hoạt lớp cuối tuần" : null));
            }
        }

        version.Publish(now.AddDays(-1));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ghi chú: ResolveSlot đổi vị trí 0-28 thành thứ và tiết, trong đó thứ Tư có 4 tiết và thứ Bảy có tiết 5 sinh hoạt lớp.
    /// </summary>
    private static (int Day, int Period) ResolveSlot(int slotIndex)
    {
        var periodsByDay = new[] { 5, 5, 4, 5, 5, 5 };
        var remaining = slotIndex;
        for (var day = 1; day <= periodsByDay.Length; day++)
        {
            if (remaining < periodsByDay[day - 1]) return (day, remaining + 1);
            remaining -= periodsByDay[day - 1];
        }

        throw new InvalidOperationException("Seed timetable slot is outside the 29-period week.");
    }

    /// <summary>
    /// Ghi chú: CreateProfileRequest tạo yêu cầu sửa hồ sơ giữ nguyên thông tin gốc và thay đổi trường liên hệ mẫu.
    /// </summary>
    private static StudentProfileChangeRequest CreateProfileRequest(Student student, string reason, string evidenceKey) =>
        new(
            student.Id,
            student.UserId!.Value,
            student.FullName,
            student.DateOfBirth,
            student.Gender,
            "0909000000",
            student.Address ?? "TP. Hồ Chí Minh",
            reason,
            evidenceKey);

    /// <summary>
    /// Ghi chú: RemarkContent trả nội dung nhận xét cụ thể theo mức độ học tập của học sinh mẫu.
    /// </summary>
    private static string RemarkContent(int index) => index switch
    {
        0 => "Tiếp thu tốt, chủ động phát biểu và hoàn thành bài tập đúng hạn.",
        1 => "Kết quả ổn định, cần trình bày lời giải rõ ràng hơn.",
        2 => "Có tiến bộ nhưng cần ôn lại kiến thức nền và luyện tập thêm.",
        _ => "Cần tăng mức độ tập trung và trao đổi với giáo viên khi chưa hiểu bài."
    };
}
