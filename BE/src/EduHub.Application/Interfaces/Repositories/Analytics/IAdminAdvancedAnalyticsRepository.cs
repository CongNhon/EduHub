using EduHub.Application.Contracts.Analytics;

namespace EduHub.Application.Interfaces.Repositories.Analytics;

/// <summary>
/// Ghi chú: IAdminAdvancedAnalyticsRepository định nghĩa các phương thức truy vấn dữ liệu cho phân tích nâng cao.
/// </summary>
public interface IAdminAdvancedAnalyticsRepository
{
    Task<Guid?> GetLatestSemesterIdAsync(CancellationToken cancellationToken);
    
    Task<Guid?> GetPreviousSemesterIdAsync(Guid currentSemesterId, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<AcademicScoreObservation>> ReadAcademicScoresAsync(
        IReadOnlyCollection<Guid> semesterIds,
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken);
        
    Task<IReadOnlyList<ExpectedGradeObservation>> ReadExpectedGradesAsync(
        Guid semesterId,
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken);
        
    Task<IReadOnlyList<StudentSubjectScoreObservation>> ReadStudentSubjectScoresAsync(
        Guid semesterId,
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken);
        
    Task<IReadOnlyList<SemesterDescriptor>> ReadTrendSemestersAsync(
        Guid currentSemesterId,
        int maxSemesters,
        CancellationToken cancellationToken);
        
    Task<DataQualityRawSnapshot> ReadDataQualitySnapshotAsync(
        Guid semesterId,
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StudentRiskInput>> ReadStudentRiskInputsAsync(
        Guid semesterId,
        Guid? previousSemesterId,
        AdminAdvancedAnalyticsFilter filter,
        CancellationToken cancellationToken);
}
