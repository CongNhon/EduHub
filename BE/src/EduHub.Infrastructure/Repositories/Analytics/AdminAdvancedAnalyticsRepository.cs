using EduHub.Application.Contracts.Analytics;
using EduHub.Application.Interfaces.Repositories.Analytics;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories.Analytics;

public sealed class AdminAdvancedAnalyticsRepository(ApplicationDbContext context) : IAdminAdvancedAnalyticsRepository
{
    public async Task<Guid?> GetLatestSemesterIdAsync(CancellationToken cancellationToken)
    {
        return await context.Semesters
            .AsNoTracking()
            .OrderByDescending(s => s.StartDate)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid?> GetPreviousSemesterIdAsync(Guid currentSemesterId, CancellationToken cancellationToken)
    {
        var current = await context.Semesters
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == currentSemesterId, cancellationToken);
            
        if (current == null) return null;

        return await context.Semesters
            .AsNoTracking()
            .Where(s => s.Id != currentSemesterId && s.StartDate < current.StartDate)
            .OrderByDescending(s => s.StartDate)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AcademicScoreObservation>> ReadAcademicScoresAsync(
        IReadOnlyCollection<Guid> semesterIds,
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken)
    {
        var gradeQuery = context.GradeEntries.AsNoTracking();

        if (filter.SubjectIds?.Count > 0)
            gradeQuery = gradeQuery.Where(ge => filter.SubjectIds.Contains(ge.Assignment.SubjectId));

        var scores = await gradeQuery
            .Where(ge => semesterIds.Contains(ge.Assignment.SemesterId))
            .GroupBy(ge => new { ge.StudentId, ge.Assignment.SemesterId })
            .Select(g => new
            {
                g.Key.StudentId,
                g.Key.SemesterId,
                AverageScore = g.Average(ge => ge.Score * 10m / ge.Component.MaxScore)
            })
            .ToListAsync(cancellationToken);

        var enrollmentsQuery = context.Enrollments
            .AsNoTracking()
            .Where(e => semesterIds.Contains(e.SemesterId));

        if (filter.GradeLevels?.Count > 0)
            enrollmentsQuery = enrollmentsQuery.Where(e => filter.GradeLevels.Contains(e.ClassRoom.GradeLevel));
        if (filter.ClassIds?.Count > 0)
            enrollmentsQuery = enrollmentsQuery.Where(e => filter.ClassIds.Contains(e.ClassRoomId));

        var enrollments = await enrollmentsQuery
            .Select(e => new { e.StudentId, e.SemesterId, e.ClassRoomId, e.ClassRoom.Name, e.ClassRoom.GradeLevel })
            .ToListAsync(cancellationToken);

        return enrollments
            .Join(scores, 
                e => new { e.StudentId, e.SemesterId }, 
                s => new { s.StudentId, s.SemesterId }, 
                (e, s) => new AcademicScoreObservation(
                    e.StudentId,
                    e.SemesterId,
                    s.AverageScore,
                    e.ClassRoomId,
                    e.Name,
                    null,
                    null,
                    e.GradeLevel))
            .ToList();
    }

    public async Task<IReadOnlyList<ExpectedGradeObservation>> ReadExpectedGradesAsync(
        Guid semesterId,
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken)
    {
        var enrollmentsQuery = context.Enrollments
            .AsNoTracking()
            .Where(e => e.SemesterId == semesterId);
            
        if (filter.ClassIds?.Count > 0)
            enrollmentsQuery = enrollmentsQuery.Where(e => filter.ClassIds.Contains(e.ClassRoomId));

        var enrollments = await enrollmentsQuery
            .Select(e => new { e.StudentId, e.ClassRoomId })
            .ToListAsync(cancellationToken);

        var gradeCounts = await context.GradeEntries
            .AsNoTracking()
            .Where(ge => ge.Assignment.SemesterId == semesterId)
            .GroupBy(ge => ge.StudentId)
            .Select(g => new { StudentId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var expectedCounts = await context.GradeComponents
            .AsNoTracking()
            .Where(gc => gc.SemesterId == semesterId && gc.IsActive)
            .GroupBy(gc => gc.SubjectId)
            .Select(g => new { SubjectId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var assignments = await context.TeachingAssignments
            .AsNoTracking()
            .Where(ta => ta.SemesterId == semesterId && ta.IsActive)
            .Select(ta => new { ta.ClassRoomId, ta.SubjectId })
            .ToListAsync(cancellationToken);

        var classExpected = assignments
            .Join(expectedCounts, ta => ta.SubjectId, ec => ec.SubjectId, (ta, ec) => new { ta.ClassRoomId, ec.Count })
            .GroupBy(x => x.ClassRoomId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

        var gradeCountDict = gradeCounts.ToDictionary(x => x.StudentId, x => x.Count);

        return enrollments
            .Select(e => new ExpectedGradeObservation(
                e.StudentId,
                classExpected.GetValueOrDefault(e.ClassRoomId, 0),
                gradeCountDict.GetValueOrDefault(e.StudentId, 0)
            ))
            .ToList();
    }

    public async Task<IReadOnlyList<StudentSubjectScoreObservation>> ReadStudentSubjectScoresAsync(
        Guid semesterId,
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken)
    {
        var query = context.GradeEntries
            .AsNoTracking()
            .Where(ge => ge.Assignment.SemesterId == semesterId);
            
        if (filter.SubjectIds?.Count > 0)
            query = query.Where(ge => filter.SubjectIds.Contains(ge.Assignment.SubjectId));

        return await query
            .GroupBy(ge => new { ge.StudentId, ge.Assignment.SubjectId })
            .Select(g => new StudentSubjectScoreObservation(
                g.Key.StudentId,
                g.Key.SubjectId,
                g.Average(ge => (decimal?)(ge.Score * 10m / ge.Component.MaxScore))))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SemesterDescriptor>> ReadTrendSemestersAsync(
        Guid currentSemesterId,
        int maxSemesters,
        CancellationToken cancellationToken)
    {
        var current = await context.Semesters.AsNoTracking().FirstOrDefaultAsync(s => s.Id == currentSemesterId, cancellationToken);
        if (current == null) return new List<SemesterDescriptor>();

        return await context.Semesters
            .AsNoTracking()
            .Include(s => s.AcademicYear)
            .Where(s => s.StartDate <= current.StartDate)
            .OrderByDescending(s => s.StartDate)
            .Take(maxSemesters)
            .Select(s => new SemesterDescriptor(
                s.Id,
                s.Name,
                s.AcademicYear.StartDate.Year,
                s.AcademicYear.EndDate.Year,
                s.StartDate.ToDateTime(TimeOnly.MinValue)))
            .ToListAsync(cancellationToken);
    }

    public async Task<DataQualityRawSnapshot> ReadDataQualitySnapshotAsync(
        Guid semesterId,
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken)
    {
        var studentsCount = await context.Students.CountAsync(cancellationToken);
        if (studentsCount == 0) return new DataQualityRawSnapshot(new List<DataQualityRawDimension>());

        var missingEmailCount = await context.Students.CountAsync(s => s.User == null || string.IsNullOrEmpty(s.User.Email), cancellationToken);
        var missingPhoneCount = await context.Students.CountAsync(s => string.IsNullOrEmpty(s.PhoneNumber), cancellationToken);
        
        var dimensions = new List<DataQualityRawDimension>
        {
            new("COMPLETENESS", studentsCount, missingEmailCount),
            new("VALIDITY", studentsCount, missingPhoneCount),
            new("CONSISTENCY", studentsCount, 0),
            new("INTEGRITY", studentsCount, 0),
            new("UNIQUENESS", studentsCount, 0),
            new("FRESHNESS", studentsCount, 0)
        };

        return new DataQualityRawSnapshot(dimensions);
    }

    public async Task<IReadOnlyList<StudentRiskInput>> ReadStudentRiskInputsAsync(
        Guid semesterId,
        Guid? previousSemesterId,
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken)
    {
        var enrollmentsQuery = context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.ClassRoom)
            .Where(e => e.SemesterId == semesterId);

        if (filter.ClassIds?.Count > 0)
            enrollmentsQuery = enrollmentsQuery.Where(e => filter.ClassIds.Contains(e.ClassRoomId));

        var enrollments = await enrollmentsQuery.ToListAsync(cancellationToken);

        var semesterIds = previousSemesterId.HasValue ? new[] { semesterId, previousSemesterId.Value } : new[] { semesterId };
        
        var scores = await context.GradeEntries
            .AsNoTracking()
            .Where(ge => semesterIds.Contains(ge.Assignment.SemesterId))
            .GroupBy(ge => new { ge.StudentId, ge.Assignment.SemesterId })
            .Select(g => new { g.Key.StudentId, g.Key.SemesterId, Avg = g.Average(ge => ge.Score * 10m / ge.Component.MaxScore) })
            .ToListAsync(cancellationToken);

        var failedCounts = await context.GradeEntries
            .AsNoTracking()
            .Where(ge => ge.Assignment.SemesterId == semesterId && ge.Score * 10m < ge.Component.MaxScore * 5m)
            .GroupBy(ge => ge.StudentId)
            .Select(g => new { StudentId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalSubjectCounts = await context.TeachingAssignments
            .AsNoTracking()
            .Where(ta => ta.SemesterId == semesterId && ta.IsActive)
            .GroupBy(ta => ta.ClassRoomId)
            .Select(g => new { ClassRoomId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var expectedGrades = await ReadExpectedGradesAsync(semesterId, filter, cancellationToken);

        var scoreDict = scores.GroupBy(s => s.StudentId).ToDictionary(g => g.Key, g => g.ToList());
        var failedDict = failedCounts.ToDictionary(x => x.StudentId, x => x.Count);
        var subjectDict = totalSubjectCounts.ToDictionary(x => x.ClassRoomId, x => x.Count);
        var expectedDict = expectedGrades.ToDictionary(x => x.StudentId, x => x);

        return enrollments.Select(e => {
            var studentScores = scoreDict.GetValueOrDefault(e.StudentId);
            var currentAvg = studentScores?.FirstOrDefault(s => s.SemesterId == semesterId)?.Avg;
            var prevAvg = previousSemesterId.HasValue ? studentScores?.FirstOrDefault(s => s.SemesterId == previousSemesterId)?.Avg : null;
            var expected = expectedDict.GetValueOrDefault(e.StudentId);
            decimal? missingRate = expected is { ExpectedCount: > 0 } ? (decimal)(expected.ExpectedCount - expected.RecordedCount) / expected.ExpectedCount * 100 : null;

            return new StudentRiskInput(
                e.StudentId,
                e.Student.StudentCode,
                e.Student.FullName,
                e.ClassRoomId,
                e.ClassRoom.ClassCode,
                e.ClassRoom.Name,
                e.ClassRoom.GradeLevel,
                currentAvg,
                prevAvg,
                missingRate,
                failedDict.GetValueOrDefault(e.StudentId, 0),
                subjectDict.GetValueOrDefault(e.ClassRoomId, 0),
                null);
        }).ToList();
    }
}
