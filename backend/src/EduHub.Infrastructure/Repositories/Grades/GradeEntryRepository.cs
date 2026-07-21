using EduHub.Application.Contracts.Grades;
using EduHub.Application.Interfaces.Repositories.Grades;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Grades;

/// <summary>
/// Ghi chú: GradeEntryRepository dùng EF Core để truy cập điểm, assignment, enrollment và outbox.
/// </summary>
public sealed class GradeEntryRepository(ApplicationDbContext dbContext) : IGradeEntryRepository
{
    public Task<TeachingAssignment?> GetAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken) =>
        dbContext.TeachingAssignments.AsNoTracking()
            .Include(assignment => assignment.Semester)
            .SingleOrDefaultAsync(
            assignment => assignment.Id == assignmentId && assignment.IsActive,
            cancellationToken);

    public Task<GradeComponent?> GetActiveComponentAsync(Guid componentId, CancellationToken cancellationToken) =>
        dbContext.GradeComponents.AsNoTracking().SingleOrDefaultAsync(
            component => component.Id == componentId && component.IsActive,
            cancellationToken);

    public Task<bool> StudentIsEnrolledAsync(
        Guid studentId,
        Guid classRoomId,
        Guid semesterId,
        CancellationToken cancellationToken) =>
        dbContext.Enrollments.AnyAsync(
            enrollment =>
                enrollment.StudentId == studentId &&
                enrollment.ClassRoomId == classRoomId &&
                enrollment.SemesterId == semesterId &&
                enrollment.Status == EnrollmentStatus.Active,
            cancellationToken);

    public Task<GradeEntry?> GetGradeEntryAsync(
        Guid studentId,
        Guid assignmentId,
        Guid componentId,
        CancellationToken cancellationToken) =>
        dbContext.GradeEntries
            .Include(entry => entry.Histories)
            .SingleOrDefaultAsync(
                entry =>
                    entry.StudentId == studentId &&
                    entry.AssignmentId == assignmentId &&
                    entry.ComponentId == componentId,
                cancellationToken);

    public Task<List<GradeEntry>> GetGradeEntriesByAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken) =>
        dbContext.GradeEntries
            .Include(entry => entry.Histories)
            .Where(entry => entry.AssignmentId == assignmentId)
            .ToListAsync(cancellationToken);

    public Task<List<GradeComponent>> GetActiveComponentsAsync(Guid subjectId, Guid semesterId, CancellationToken cancellationToken) =>
        dbContext.GradeComponents
            .AsNoTracking()
            .Where(component => component.SubjectId == subjectId && component.SemesterId == semesterId && component.IsActive)
            .OrderBy(component => component.DisplayOrder)
            .ToListAsync(cancellationToken);

    public Task<List<Guid>> GetActiveStudentIdsAsync(Guid classRoomId, Guid semesterId, CancellationToken cancellationToken) =>
        dbContext.Enrollments
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.ClassRoomId == classRoomId &&
                enrollment.SemesterId == semesterId &&
                enrollment.Status == EnrollmentStatus.Active)
            .Select(enrollment => enrollment.StudentId)
            .ToListAsync(cancellationToken);

    public Task<bool> GradebookHasStatusAsync(Guid assignmentId, GradeStatus status, CancellationToken cancellationToken) =>
        dbContext.GradeEntries.AnyAsync(
            entry => entry.AssignmentId == assignmentId && entry.Status == status,
            cancellationToken);

    /// <summary>
    /// Ghi chú: ParentCanReadStudentAsync kiểm tra liên kết phụ huynh-học sinh active trước khi đọc điểm.
    /// </summary>
    public Task<bool> ParentCanReadStudentAsync(Guid parentUserId, Guid studentId, CancellationToken cancellationToken) =>
        dbContext.ParentStudents.AsNoTracking().AnyAsync(
            link => link.ParentUserId == parentUserId && link.StudentId == studentId && link.IsActive,
            cancellationToken);

    /// <summary>
    /// Ghi chú: GetPublishedGradebookAsync đọc điểm Published/Locked cùng context học sinh, lớp, môn và học kỳ.
    /// </summary>
    public async Task<PublishedGradebookResponse?> GetPublishedGradebookAsync(
        Guid studentId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var assignment = await dbContext.TeachingAssignments.AsNoTracking()
            .Where(candidate => candidate.Id == assignmentId && candidate.IsActive)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.ClassRoom.ClassCode,
                ClassName = candidate.ClassRoom.Name,
                candidate.Subject.SubjectCode,
                SubjectName = candidate.Subject.Name,
                SemesterName = candidate.Semester.Name,
                TeacherName = candidate.Teacher.FullName
            })
            .SingleOrDefaultAsync(cancellationToken);
        var student = await dbContext.Students.AsNoTracking().Where(candidate => candidate.Id == studentId)
            .Select(candidate => new { candidate.StudentCode, candidate.FullName })
            .SingleOrDefaultAsync(cancellationToken);
        if (assignment is null || student is null) return null;

        var grades = await dbContext.GradeEntries
            .AsNoTracking()
            .Where(entry =>
                entry.StudentId == studentId &&
                entry.AssignmentId == assignmentId &&
                (entry.Status == GradeStatus.Published || entry.Status == GradeStatus.Locked))
            .OrderBy(entry => entry.Component.DisplayOrder)
            .Select(entry => new PublishedGradeEntryResponse(
                entry.ComponentId,
                entry.Component.Name,
                entry.Component.Weight,
                entry.Component.MaxScore,
                entry.Score,
                entry.PublicationVersion))
            .ToListAsync(cancellationToken);

        var publishedAt = await dbContext.GradeEntries.AsNoTracking()
            .Where(entry => entry.StudentId == studentId && entry.AssignmentId == assignmentId && (entry.Status == GradeStatus.Published || entry.Status == GradeStatus.Locked))
            .MaxAsync(entry => (DateTime?)entry.PublishedAtUtc, cancellationToken);
        var remark = await dbContext.StudentRemarks.AsNoTracking()
            .Where(candidate => candidate.StudentId == studentId && candidate.AssignmentId == assignmentId && candidate.IsPublished)
            .Select(candidate => candidate.Content)
            .SingleOrDefaultAsync(cancellationToken);

        return new PublishedGradebookResponse(studentId, student.StudentCode, student.FullName, assignmentId, assignment.ClassCode, assignment.ClassName, assignment.SubjectCode, assignment.SubjectName, assignment.SemesterName, assignment.TeacherName, publishedAt, remark, grades);
    }

    /// <summary>
    /// Ghi chú: GetGradebookAsync đọc một bounded read model để frontend không gọi N+1 theo từng học sinh.
    /// </summary>
    public async Task<GradebookResponse?> GetGradebookAsync(Guid assignmentId, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.TeachingAssignments.AsNoTracking().Where(candidate => candidate.Id == assignmentId && candidate.IsActive)
            .Select(candidate => new { candidate.Id, candidate.ClassRoomId, candidate.ClassRoom.ClassCode, ClassName = candidate.ClassRoom.Name, candidate.SubjectId, candidate.Subject.SubjectCode, SubjectName = candidate.Subject.Name, candidate.SemesterId, SemesterName = candidate.Semester.Name, candidate.TeacherId, TeacherName = candidate.Teacher.FullName })
            .SingleOrDefaultAsync(cancellationToken);
        if (assignment is null) return null;

        var components = await dbContext.GradeComponents.AsNoTracking()
            .Where(component => component.SubjectId == assignment.SubjectId && component.SemesterId == assignment.SemesterId && component.IsActive)
            .OrderBy(component => component.DisplayOrder)
            .Select(component => new GradebookComponentResponse(component.Id, component.Name, component.Weight, component.MaxScore, component.DisplayOrder, component.IsRequired))
            .ToListAsync(cancellationToken);
        var gradeEntries = await dbContext.GradeEntries.AsNoTracking().Where(entry => entry.AssignmentId == assignmentId)
            .Select(entry => new { entry.StudentId, entry.ComponentId, entry.Score, entry.Status, entry.Version, entry.PublicationVersion })
            .ToListAsync(cancellationToken);
        var remarks = await dbContext.StudentRemarks.AsNoTracking().Where(remark => remark.AssignmentId == assignmentId)
            .Select(remark => new { remark.StudentId, remark.Content, remark.Version })
            .ToListAsync(cancellationToken);
        var roster = await dbContext.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.ClassRoomId == assignment.ClassRoomId && enrollment.SemesterId == assignment.SemesterId && enrollment.Status == EnrollmentStatus.Active)
            .OrderBy(enrollment => enrollment.Student.FullName)
            .Select(enrollment => new { enrollment.StudentId, enrollment.Student.StudentCode, enrollment.Student.FullName })
            .ToListAsync(cancellationToken);

        var students = roster.Select(student =>
        {
            var remark = remarks.SingleOrDefault(candidate => candidate.StudentId == student.StudentId);
            var cells = components.Select(component =>
            {
                var entry = gradeEntries.SingleOrDefault(candidate => candidate.StudentId == student.StudentId && candidate.ComponentId == component.Id);
                return new GradebookCellResponse(component.Id, entry?.Score, entry?.Status.ToString() ?? GradeStatus.Draft.ToString(), entry?.Version, entry?.PublicationVersion ?? 0);
            }).ToList();
            return new GradebookStudentResponse(student.StudentId, student.StudentCode, student.FullName, remark?.Content, remark?.Version, cells);
        }).ToList();
        var status = gradeEntries.Count == 0 ? GradeStatus.Draft.ToString() : gradeEntries[0].Status.ToString();
        return new GradebookResponse(assignment.Id, assignment.ClassRoomId, assignment.ClassCode, assignment.ClassName, assignment.SubjectId, assignment.SubjectCode, assignment.SubjectName, assignment.SemesterId, assignment.SemesterName, assignment.TeacherId, assignment.TeacherName, status, components, students);
    }

    public Task<StudentRemark?> GetStudentRemarkAsync(Guid assignmentId, Guid studentId, CancellationToken cancellationToken) =>
        dbContext.StudentRemarks.SingleOrDefaultAsync(remark => remark.AssignmentId == assignmentId && remark.StudentId == studentId, cancellationToken);

    public Task<List<StudentRemark>> GetRemarksByAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken) =>
        dbContext.StudentRemarks.Where(remark => remark.AssignmentId == assignmentId).ToListAsync(cancellationToken);

    public void AddGradeEntry(GradeEntry gradeEntry) =>
        dbContext.GradeEntries.Add(gradeEntry);

    public void AddStudentRemark(StudentRemark remark) => dbContext.StudentRemarks.Add(remark);

    public void AddOutboxMessage(OutboxMessage outboxMessage) =>
        dbContext.OutboxMessages.Add(outboxMessage);
}
