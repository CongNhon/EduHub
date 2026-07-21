using EduHub.Application.Features.Students.Common;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Options;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EduHub.Infrastructure.Seed;

/// <summary>
/// Ghi chú: DevelopmentSchoolSeeder tạo trường THPT demo gồm chương trình 35 tuần, 9 lớp, 24 giáo viên và 270 học sinh.
/// </summary>
public static class DevelopmentSchoolSeeder
{
    private static readonly int[] GradeLevels = [10, 11, 12];

    private static readonly (string Code, string Name, int Credits)[] SubjectCatalog =
    [
        ("LIT", "Ngữ văn", 3), ("MATH", "Toán", 3), ("ENG", "Ngoại ngữ 1", 3),
        ("HIST", "Lịch sử", 2), ("PE", "Giáo dục thể chất", 2), ("DEF", "Giáo dục quốc phòng và an ninh", 1),
        ("EXP", "Hoạt động trải nghiệm, hướng nghiệp", 2), ("HOMEROOM", "Sinh hoạt lớp", 1), ("LOCAL", "Giáo dục địa phương", 1),
        ("GEO", "Địa lí", 2), ("ECONLAW", "Giáo dục kinh tế và pháp luật", 2), ("PHYS", "Vật lí", 2),
        ("CHEM", "Hóa học", 2), ("BIO", "Sinh học", 2), ("TECH", "Công nghệ", 2),
        ("INFO", "Tin học", 2), ("MUSIC", "Âm nhạc", 2), ("ART", "Mĩ thuật", 2),
        ("TOPIC-MATH", "Chuyên đề Toán", 1), ("TOPIC-PHYS", "Chuyên đề Vật lí", 1),
        ("TOPIC-INFO", "Chuyên đề Tin học", 1), ("GUIDED", "Tự học có hướng dẫn", 1),
        ("STEM", "STEM và câu lạc bộ", 1)
    ];

    private static readonly string[] TeachingSubjectCodes =
    [
        "LIT", "MATH", "ENG", "HIST", "PE", "DEF", "EXP", "LOCAL", "GEO", "PHYS", "CHEM", "INFO",
        "TOPIC-MATH", "TOPIC-PHYS", "TOPIC-INFO"
    ];

    /// <summary>
    /// Ghi chú: SeedDevelopmentSchoolAsync chạy sau seed happy-flow cũ và bổ sung bộ dữ liệu toàn trường nếu development seed bật.
    /// </summary>
    public static async Task SeedDevelopmentSchoolAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<DevelopmentSeedOptions>>().Value;
        if (!options.Enabled) return;
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();
        var seedPasswordHash = passwordHashService.HashPassword(options.Password);
        await SeedAsync(dbContext, seedPasswordHash, CancellationToken.None);
    }

    /// <summary>
    /// Ghi chú: SeedAsync tạo tuần tự học kỳ, môn, lớp, giáo viên, curriculum, GVCN và học sinh để giữ khóa ngoại hợp lệ.
    /// </summary>
    private static async Task SeedAsync(ApplicationDbContext dbContext, string seedPasswordHash, CancellationToken cancellationToken)
    {
        var academicYear = await dbContext.AcademicYears.SingleAsync(year => year.NormalizedName == "EDUHUB 2026-2027", cancellationToken);
        var semesters = await EnsureSemestersAsync(dbContext, academicYear, cancellationToken);
        var subjects = await EnsureSubjectsAsync(dbContext, cancellationToken);
        var classes = await EnsureClassesAsync(dbContext, academicYear, cancellationToken);
        var teachers = await EnsureTeachersAsync(dbContext, seedPasswordHash, cancellationToken);
        await EnsureCapabilitiesAsync(dbContext, teachers, subjects, cancellationToken);
        await EnsureCurriculumAsync(dbContext, academicYear, subjects, cancellationToken);
        await EnsureHomeroomsAsync(dbContext, classes, teachers, cancellationToken);
        await EnsureStudentsAsync(dbContext, classes, semesters["HK1"], seedPasswordHash, cancellationToken);
    }

    /// <summary>
    /// Ghi chú: EnsureSemestersAsync bổ sung HK2 để đủ 18 tuần HK1 và 17 tuần HK2 của năm học.
    /// </summary>
    private static async Task<Dictionary<string, Semester>> EnsureSemestersAsync(ApplicationDbContext dbContext, AcademicYear academicYear, CancellationToken cancellationToken)
    {
        var semesters = await dbContext.Semesters.Where(semester => semester.AcademicYearId == academicYear.Id).ToDictionaryAsync(semester => semester.NormalizedName, cancellationToken);
        if (!semesters.ContainsKey("HK2"))
        {
            var semester = new Semester(
                academicYear.Id,
                "HK2",
                "HK2",
                new DateOnly(2027, 1, 1),
                new DateOnly(2027, 5, 31),
                new DateOnly(2027, 1, 1),
                new DateOnly(2027, 5, 20));
            dbContext.Semesters.Add(semester);
            semesters["HK2"] = semester;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return semesters;
    }

    /// <summary>
    /// Ghi chú: EnsureSubjectsAsync seed toàn bộ môn bắt buộc, 9 môn lựa chọn, 3 chuyên đề và STEM.
    /// </summary>
    private static async Task<Dictionary<string, Subject>> EnsureSubjectsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        var subjects = await dbContext.Subjects.ToDictionaryAsync(subject => subject.NormalizedSubjectCode, cancellationToken);
        foreach (var specification in SubjectCatalog)
        {
            if (subjects.ContainsKey(specification.Code)) continue;
            var subject = new Subject(specification.Code, specification.Code, specification.Name, specification.Credits);
            dbContext.Subjects.Add(subject);
            subjects[specification.Code] = subject;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return subjects;
    }

    /// <summary>
    /// Ghi chú: EnsureClassesAsync tạo ba lớp cho mỗi khối 10, 11, 12 với sức chứa 45 học sinh.
    /// </summary>
    private static async Task<List<ClassRoom>> EnsureClassesAsync(ApplicationDbContext dbContext, AcademicYear academicYear, CancellationToken cancellationToken)
    {
        var classes = await dbContext.ClassRooms.Where(classRoom => classRoom.AcademicYearId == academicYear.Id).ToDictionaryAsync(classRoom => classRoom.NormalizedClassCode, cancellationToken);
        foreach (var grade in new[] { 10, 11, 12 })
        {
            for (var index = 1; index <= 3; index++)
            {
                var code = $"{grade}A{index}";
                if (classes.ContainsKey(code)) continue;
                var classRoom = new ClassRoom(code, code, $"Lớp {code}", academicYear.Id, grade, 45);
                dbContext.ClassRooms.Add(classRoom);
                classes[code] = classRoom;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return classes.Values.OrderBy(classRoom => classRoom.ClassCode).ToList();
    }

    /// <summary>
    /// Ghi chú: EnsureTeachersAsync tạo 24 tài khoản giáo viên có họ tên và mã nhân sự để phân công tự động.
    /// </summary>
    private static async Task<List<User>> EnsureTeachersAsync(ApplicationDbContext dbContext, string seedPasswordHash, CancellationToken cancellationToken)
    {
        var users = await dbContext.Users.ToDictionaryAsync(user => user.NormalizedEmail, cancellationToken);
        var teachers = new List<User>();
        for (var index = 1; index <= 24; index++)
        {
            var email = index == 1 ? DevelopmentAcademicSeeder.TeacherEmail : $"teacher{index:00}@eduhub.local";
            var normalizedEmail = email.ToUpperInvariant();
            if (!users.TryGetValue(normalizedEmail, out var teacher))
            {
                var primaryCode = TeachingSubjectCodes[(index - 1) % TeachingSubjectCodes.Length];
                teacher = new User(email, normalizedEmail, seedPasswordHash, UserRole.Teacher, $"Giáo viên {index:00} - {primaryCode}", $"GV{index:000}");
                dbContext.Users.Add(teacher);
                users[normalizedEmail] = teacher;
            }

            teachers.Add(teacher);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return teachers;
    }

    /// <summary>
    /// Ghi chú: EnsureCapabilitiesAsync gắn mỗi giáo viên một môn chính, một môn phụ và tải tối đa 18 tiết/tuần.
    /// </summary>
    private static async Task EnsureCapabilitiesAsync(ApplicationDbContext dbContext, List<User> teachers, Dictionary<string, Subject> subjects, CancellationToken cancellationToken)
    {
        var existing = (await dbContext.TeacherSubjectCapabilities.Select(capability => new { capability.TeacherId, capability.SubjectId }).ToListAsync(cancellationToken))
            .Select(item => (item.TeacherId, item.SubjectId)).ToHashSet();
        for (var index = 0; index < teachers.Count; index++)
        {
            var primary = subjects[TeachingSubjectCodes[index % TeachingSubjectCodes.Length]];
            var secondary = subjects[TeachingSubjectCodes[(index + 5) % TeachingSubjectCodes.Length]];
            if (existing.Add((teachers[index].Id, primary.Id)))
            {
                dbContext.TeacherSubjectCapabilities.Add(new TeacherSubjectCapability(teachers[index].Id, primary.Id, TeacherSubjectPriority.Primary, 18));
            }

            if (existing.Add((teachers[index].Id, secondary.Id)))
            {
                dbContext.TeacherSubjectCapabilities.Add(new TeacherSubjectCapability(teachers[index].Id, secondary.Id, TeacherSubjectPriority.Secondary, 18));
            }
        }

        var guidedStudy = subjects["GUIDED"];
        foreach (var teacher in teachers.Take(3))
        {
            if (existing.Add((teacher.Id, guidedStudy.Id)))
            {
                dbContext.TeacherSubjectCapabilities.Add(new TeacherSubjectCapability(teacher.Id, guidedStudy.Id, TeacherSubjectPriority.Secondary, 18));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ghi chú: EnsureCurriculumAsync tạo chương trình 1015 tiết để mỗi lớp học đủ 29 tiết trong toàn bộ 35 tuần thực học.
    /// </summary>
    private static async Task EnsureCurriculumAsync(ApplicationDbContext dbContext, AcademicYear academicYear, Dictionary<string, Subject> subjects, CancellationToken cancellationToken)
    {
        var plans = await dbContext.CurriculumPlans.Include(plan => plan.SubjectQuotas)
            .Where(plan => plan.AcademicYearId == academicYear.Id)
            .ToDictionaryAsync(plan => plan.GradeLevel, cancellationToken);
        foreach (var grade in GradeLevels)
        {
            if (!plans.TryGetValue(grade, out var plan))
            {
                plan = new CurriculumPlan(academicYear.Id, grade, $"Chương trình THPT khối {grade}", 35, 18, 17);
                dbContext.CurriculumPlans.Add(plan);
                plans[grade] = plan;
            }

            AddQuota(plan, subjects["LIT"], CurriculumSubjectKind.Required, 105, 54, 51, true, 2);
            AddQuota(plan, subjects["MATH"], CurriculumSubjectKind.Required, 105, 54, 51, true, 2);
            AddQuota(plan, subjects["ENG"], CurriculumSubjectKind.Required, 105, 54, 51, true, 2);
            AddQuota(plan, subjects["HIST"], CurriculumSubjectKind.Required, 52, 27, 25, false, 1);
            AddQuota(plan, subjects["PE"], CurriculumSubjectKind.Required, 70, 36, 34, true, 2);
            AddQuota(plan, subjects["DEF"], CurriculumSubjectKind.Required, 35, 18, 17, false, 1);
            AddQuota(plan, subjects["EXP"], CurriculumSubjectKind.Required, 70, 36, 34, true, 2);
            AddQuota(plan, subjects["HOMEROOM"], CurriculumSubjectKind.Required, 35, 18, 17, false, 1, true);
            AddQuota(plan, subjects["LOCAL"], CurriculumSubjectKind.Required, 35, 18, 17, false, 1);
            AddQuota(plan, subjects["PHYS"], CurriculumSubjectKind.Elective, 70, 36, 34, true, 2);
            AddQuota(plan, subjects["CHEM"], CurriculumSubjectKind.Elective, 70, 36, 34, true, 2);
            AddQuota(plan, subjects["INFO"], CurriculumSubjectKind.Elective, 70, 36, 34, true, 2);
            AddQuota(plan, subjects["GEO"], CurriculumSubjectKind.Elective, 70, 36, 34, true, 2);
            AddQuota(plan, subjects["TOPIC-MATH"], CurriculumSubjectKind.LearningTopic, 35, 18, 17, false, 1);
            AddQuota(plan, subjects["TOPIC-PHYS"], CurriculumSubjectKind.LearningTopic, 35, 18, 17, false, 1);
            AddQuota(plan, subjects["TOPIC-INFO"], CurriculumSubjectKind.LearningTopic, 35, 18, 17, false, 1);
            AddQuota(plan, subjects["GUIDED"], CurriculumSubjectKind.Required, 18, 9, 9, false, 1);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ghi chú: AddQuota thêm rule số tiết môn học vào đúng chương trình khối.
    /// </summary>
    private static void AddQuota(
        CurriculumPlan plan,
        Subject subject,
        CurriculumSubjectKind kind,
        int annual,
        int semester1,
        int semester2,
        bool canDouble,
        int maxPerDay,
        bool includesHomeroom = false,
        TimetableSession? preferredSession = null)
    {
        if (plan.SubjectQuotas.Any(quota => quota.SubjectId == subject.Id)) return;
        plan.SubjectQuotas.Add(new CurriculumSubjectQuota(
            plan.Id,
            subject.Id,
            kind,
            annual,
            semester1,
            semester2,
            canDouble,
            maxPerDay,
            includesHomeroom,
            preferredSession));
    }

    /// <summary>
    /// Ghi chú: EnsureHomeroomsAsync phân công 9 giáo viên khác teacher happy-flow làm GVCN cho 9 lớp.
    /// </summary>
    private static async Task EnsureHomeroomsAsync(ApplicationDbContext dbContext, IReadOnlyList<ClassRoom> classes, IReadOnlyList<User> teachers, CancellationToken cancellationToken)
    {
        var classIds = (await dbContext.HomeroomAssignments.Where(assignment => assignment.IsActive).Select(assignment => assignment.ClassRoomId).ToListAsync(cancellationToken)).ToHashSet();
        var teacherIds = (await dbContext.HomeroomAssignments.Where(assignment => assignment.IsActive).Select(assignment => assignment.TeacherId).ToListAsync(cancellationToken)).ToHashSet();
        var available = teachers.Skip(9).Concat(teachers.Skip(1).Take(8)).Where(teacher => !teacherIds.Contains(teacher.Id)).ToList();
        foreach (var classRoom in classes.Where(classRoom => !classIds.Contains(classRoom.Id)))
        {
            var teacher = available.FirstOrDefault();
            if (teacher is null) break;
            available.RemoveAt(0);
            dbContext.HomeroomAssignments.Add(new HomeroomAssignment(classRoom.Id, teacher.Id, DateTime.UtcNow));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ghi chú: EnsureStudentsAsync tạo đủ 30 học sinh/lớp, tài khoản Student/Parent, link và enrollment HK1.
    /// </summary>
    private static async Task EnsureStudentsAsync(ApplicationDbContext dbContext, IReadOnlyList<ClassRoom> classes, Semester semester, string seedPasswordHash, CancellationToken cancellationToken)
    {
        var users = await dbContext.Users.ToDictionaryAsync(user => user.NormalizedEmail, cancellationToken);
        var students = await dbContext.Students.ToDictionaryAsync(student => student.NormalizedStudentCode, cancellationToken);
        var links = (await dbContext.ParentStudents.Select(link => new { link.ParentUserId, link.StudentId }).ToListAsync(cancellationToken))
            .Select(item => (item.ParentUserId, item.StudentId)).ToHashSet();
        var enrollments = (await dbContext.Enrollments.Select(enrollment => new { enrollment.StudentId, enrollment.ClassRoomId, enrollment.SemesterId }).ToListAsync(cancellationToken))
            .Select(item => (item.StudentId, item.ClassRoomId, item.SemesterId)).ToHashSet();
        var names = new[] { "Nguyễn Minh An", "Trần Gia Hân", "Lê Hoàng Nam", "Phạm Ngọc Mai", "Võ Đức Anh", "Đặng Khánh Linh", "Bùi Quốc Bảo", "Đỗ Thanh Hà", "Hồ Nhật Minh", "Ngô Phương Thảo" };
        var globalIndex = 0;
        foreach (var classRoom in classes)
        {
            for (var seat = 1; seat <= 30; seat++)
            {
                globalIndex++;
                var useHappyFlowStudent = classRoom.ClassCode == "10A1" && seat == 1;
                var code = useHappyFlowStudent ? DevelopmentAcademicSeeder.StudentCode : $"STU{globalIndex + 10000:00000}";
                if (!students.TryGetValue(code, out var student))
                {
                    var fullName = $"{names[(globalIndex - 1) % names.Length]} {globalIndex:000}";
                    var birthYear = 2026 - classRoom.GradeLevel - 5;
                    student = new Student(code, code, fullName, StudentNormalization.SearchText(fullName), new DateOnly(birthYear, globalIndex % 12 + 1, globalIndex % 27 + 1), globalIndex % 2 == 0 ? "Nữ" : "Nam", null, "TP. Hồ Chí Minh");
                    dbContext.Students.Add(student);
                    students[code] = student;
                }

                var studentEmail = useHappyFlowStudent ? DevelopmentAcademicSeeder.StudentEmail : $"student.{classRoom.ClassCode.ToLowerInvariant()}.{seat:00}@eduhub.local";
                var normalizedStudentEmail = studentEmail.ToUpperInvariant();
                if (!users.TryGetValue(normalizedStudentEmail, out var studentUser))
                {
                    studentUser = new User(studentEmail, normalizedStudentEmail, seedPasswordHash, UserRole.Student, student.FullName, student.StudentCode);
                    dbContext.Users.Add(studentUser);
                    users[normalizedStudentEmail] = studentUser;
                }

                if (student.UserId is null) student.LinkUserAccount(studentUser.Id, DateTime.UtcNow);
                var parentEmail = useHappyFlowStudent ? DevelopmentAcademicSeeder.ParentEmail : $"parent.{classRoom.ClassCode.ToLowerInvariant()}.{seat:00}@eduhub.local";
                var normalizedParentEmail = parentEmail.ToUpperInvariant();
                if (!users.TryGetValue(normalizedParentEmail, out var parent))
                {
                    parent = new User(parentEmail, normalizedParentEmail, seedPasswordHash, UserRole.Parent, $"Phụ huynh {student.FullName}", null, $"090{globalIndex:0000000}");
                    dbContext.Users.Add(parent);
                    users[normalizedParentEmail] = parent;
                }

                if (links.Add((parent.Id, student.Id))) dbContext.ParentStudents.Add(new ParentStudent(parent.Id, student.Id, seat % 2 == 0 ? "Mẹ" : "Cha", DateTime.UtcNow));
                if (enrollments.Add((student.Id, classRoom.Id, semester.Id))) dbContext.Enrollments.Add(new Enrollment(student.Id, classRoom.Id, semester.Id, DateTime.UtcNow));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        var activeCounts = await dbContext.Enrollments.Where(enrollment => enrollment.SemesterId == semester.Id && enrollment.Status == EnrollmentStatus.Active)
            .GroupBy(enrollment => enrollment.ClassRoomId).Select(group => new { ClassRoomId = group.Key, Count = group.Count() }).ToDictionaryAsync(item => item.ClassRoomId, item => item.Count, cancellationToken);
        foreach (var classRoom in classes) classRoom.SynchronizeActiveEnrollmentCount(activeCounts.GetValueOrDefault(classRoom.Id), DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
