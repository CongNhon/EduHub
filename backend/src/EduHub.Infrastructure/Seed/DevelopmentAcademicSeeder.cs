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
/// Ghi chú: DevelopmentAcademicSeeder tạo bộ dữ liệu học vụ mẫu để test Swagger/Postman happy-flow EduHub.
/// </summary>
public static class DevelopmentAcademicSeeder
{
    public const string AcademicEmail = "academic@eduhub.local";
    public const string TeacherEmail = "teacher@eduhub.local";
    public const string ParentEmail = "parent@eduhub.local";
    public const string StudentEmail = "student@eduhub.local";
    public const string SeedPassword = "Admin@123456";
    public const string AcademicYearName = "EduHub 2026-2027";
    public const string SubjectCode = "MATH10";
    public const string ClassCode = "10A1";
    public const string StudentCode = "STU0001";

    /// <summary>
    /// Ghi chú: SeedDevelopmentAcademicDataAsync tạo tài khoản role mẫu, năm học, học kỳ, môn, lớp, học sinh, enrollment, assignment và component điểm.
    /// </summary>
    public static async Task SeedDevelopmentAcademicDataAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<DevelopmentSeedOptions>>().Value;
        if (!options.Enabled)
        {
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();
        await SeedDevelopmentAcademicDataAsync(dbContext, passwordHashService, options.Password, CancellationToken.None);
    }

    /// <summary>
    /// Ghi chú: SeedDevelopmentAcademicDataAsync ghi trực tiếp dữ liệu học vụ mẫu vào DbContext dùng cho integration test.
    /// </summary>
    public static Task SeedDevelopmentAcademicDataAsync(
        ApplicationDbContext dbContext,
        IPasswordHashService passwordHashService,
        CancellationToken cancellationToken) =>
        SeedDevelopmentAcademicDataAsync(dbContext, passwordHashService, SeedPassword, cancellationToken);

    /// <summary>
    /// Ghi chu: SeedDevelopmentAcademicDataAsync tao du lieu mau bang password runtime da lay tu secret configuration.
    /// </summary>
    private static async Task SeedDevelopmentAcademicDataAsync(
        ApplicationDbContext dbContext,
        IPasswordHashService passwordHashService,
        string seedPassword,
        CancellationToken cancellationToken)
    {
        var academicAdmin = await EnsureUserAsync(dbContext, passwordHashService, AcademicEmail, UserRole.AcademicAdmin, seedPassword, cancellationToken);
        var teacher = await EnsureUserAsync(dbContext, passwordHashService, TeacherEmail, UserRole.Teacher, seedPassword, cancellationToken);
        var parent = await EnsureUserAsync(dbContext, passwordHashService, ParentEmail, UserRole.Parent, seedPassword, cancellationToken);
        await EnsureUserAsync(dbContext, passwordHashService, StudentEmail, UserRole.Student, seedPassword, cancellationToken);

        var academicYear = await dbContext.AcademicYears.SingleOrDefaultAsync(
            year => year.NormalizedName == "EDUHUB 2026-2027",
            cancellationToken);
        if (academicYear is null)
        {
            academicYear = new AcademicYear(
                AcademicYearName,
                AcademicYearName.ToUpperInvariant(),
                new DateOnly(2026, 8, 1),
                new DateOnly(2027, 5, 31));
            dbContext.AcademicYears.Add(academicYear);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var semester = await dbContext.Semesters.SingleOrDefaultAsync(
            item => item.AcademicYearId == academicYear.Id && item.NormalizedName == "HK1",
            cancellationToken);
        if (semester is null)
        {
            semester = new Semester(
                academicYear.Id,
                "HK1",
                "HK1",
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 12, 31),
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 12, 20));
            dbContext.Semesters.Add(semester);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var subject = await dbContext.Subjects.SingleOrDefaultAsync(
            item => item.NormalizedSubjectCode == SubjectCode,
            cancellationToken);
        if (subject is null)
        {
            subject = new Subject(SubjectCode, SubjectCode, "Toan 10", 3);
            dbContext.Subjects.Add(subject);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var classRoom = await dbContext.ClassRooms.SingleOrDefaultAsync(
            item => item.AcademicYearId == academicYear.Id && item.NormalizedClassCode == ClassCode,
            cancellationToken);
        if (classRoom is null)
        {
            classRoom = new ClassRoom(ClassCode, ClassCode, "Lop 10A1", academicYear.Id, 10, 45);
            dbContext.ClassRooms.Add(classRoom);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var student = await dbContext.Students.SingleOrDefaultAsync(
            item => item.NormalizedStudentCode == StudentCode,
            cancellationToken);
        if (student is null)
        {
            student = new Student(StudentCode, StudentCode, "Nguyen Van An", "NGUYEN VAN AN", new DateOnly(2010, 5, 20));
            dbContext.Students.Add(student);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.ParentStudents.AnyAsync(
            link => link.ParentUserId == parent.Id && link.StudentId == student.Id && link.IsActive,
            cancellationToken))
        {
            dbContext.ParentStudents.Add(new ParentStudent(parent.Id, student.Id, "Cha/me", DateTime.UtcNow));
        }

        if (!await dbContext.Enrollments.AnyAsync(
            enrollment => enrollment.StudentId == student.Id && enrollment.ClassRoomId == classRoom.Id && enrollment.SemesterId == semester.Id,
            cancellationToken))
        {
            dbContext.Enrollments.Add(new Enrollment(student.Id, classRoom.Id, semester.Id, DateTime.UtcNow));
        }

        if (!await dbContext.TeachingAssignments.AnyAsync(
            assignment => assignment.ClassRoomId == classRoom.Id &&
                assignment.SubjectId == subject.Id &&
                assignment.TeacherId == teacher.Id &&
                assignment.SemesterId == semester.Id &&
                assignment.IsActive,
            cancellationToken))
        {
            dbContext.TeachingAssignments.Add(new TeachingAssignment(classRoom.Id, subject.Id, teacher.Id, semester.Id, DateTime.UtcNow));
        }

        if (!await dbContext.GradeComponents.AnyAsync(
            component => component.SubjectId == subject.Id && component.SemesterId == semester.Id && component.IsActive,
            cancellationToken))
        {
            dbContext.GradeComponents.AddRange(
                new GradeComponent(subject.Id, semester.Id, "Giua ky", "GIUA KY", 0.4m, 10m, 1, true, true, 1),
                new GradeComponent(subject.Id, semester.Id, "Cuoi ky", "CUOI KY", 0.6m, 10m, 2, true, true, 1));
        }

        _ = academicAdmin;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ghi chú: EnsureUserAsync tạo tài khoản dev theo email/role nếu chưa tồn tại trong bảng Users.
    /// </summary>
    private static async Task<User> EnsureUserAsync(
        ApplicationDbContext dbContext,
        IPasswordHashService passwordHashService,
        string email,
        UserRole role,
        string seedPassword,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var user = await dbContext.Users.SingleOrDefaultAsync(
            candidate => candidate.NormalizedEmail == normalizedEmail,
            cancellationToken);
        if (user is not null)
        {
            return user;
        }

        user = new User(email, normalizedEmail, passwordHashService.HashPassword(seedPassword), role);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }
}
