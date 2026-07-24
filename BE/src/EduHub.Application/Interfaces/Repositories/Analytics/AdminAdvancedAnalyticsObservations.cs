namespace EduHub.Application.Interfaces.Repositories.Analytics;

/// <summary>
/// Ghi chú: AcademicScoreObservation chứa dữ liệu điểm số thô từ database.
/// </summary>
public sealed record AcademicScoreObservation(
    Guid StudentId,
    Guid SemesterId,
    decimal Score,
    Guid? ClassId = null,
    string? ClassName = null,
    Guid? SubjectId = null,
    string? SubjectName = null,
    int? GradeLevel = null);

/// <summary>
/// Ghi chú: ExpectedGradeObservation chứa thông tin về số lượng cột điểm dự kiến.
/// </summary>
public sealed record ExpectedGradeObservation(
    Guid StudentId,
    int ExpectedCount,
    int RecordedCount);

/// <summary>
/// Ghi chú: StudentSubjectScoreObservation chứa điểm trung bình môn của học sinh.
/// </summary>
public sealed record StudentSubjectScoreObservation(
    Guid StudentId,
    Guid SubjectId,
    decimal? AverageScore);

/// <summary>
/// Ghi chú: SemesterDescriptor chứa thông tin mô tả về học kỳ.
/// </summary>
public sealed record SemesterDescriptor(
    Guid SemesterId,
    string Name,
    int AcademicYearStart,
    int AcademicYearEnd,
    DateTime StartDate);

/// <summary>
/// Ghi chú: DataQualityRawDimension chứa dữ liệu thô cho một chiều chất lượng dữ liệu.
/// </summary>
public sealed record DataQualityRawDimension(
    string Code,
    int EligibleRecordCount,
    int AffectedRecordCount);

/// <summary>
/// Ghi chú: DataQualityRawSnapshot chứa ảnh chụp thô về chất lượng dữ liệu của một học kỳ.
/// </summary>
public sealed record DataQualityRawSnapshot(
    IReadOnlyList<DataQualityRawDimension> Dimensions);
