namespace EduHub.WebApi.Dtos.Academics;

/// <summary>
/// Ghi chú: CreateAcademicYearRequest là DTO request API dùng để tạo năm học.
/// </summary>
public sealed record CreateAcademicYearRequest(string Name, DateOnly StartDate, DateOnly EndDate);

/// <summary>
/// Ghi chú: ListAcademicYearsRequest là DTO request API dùng để lọc và phân trang danh sách năm học.
/// </summary>
public sealed record ListAcademicYearsRequest(int? Page, int? PageSize, string? Search);

/// <summary>
/// Ghi chú: AcademicYearDto là DTO response API chứa thông tin năm học.
/// </summary>
public sealed record AcademicYearDto(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate, string Status);

/// <summary>
/// Ghi chú: CreateSemesterRequest là DTO request API dùng để tạo học kỳ.
/// </summary>
public sealed record CreateSemesterRequest(
    Guid AcademicYearId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    DateOnly GradeEntryFrom,
    DateOnly GradeEntryTo);

/// <summary>
/// Ghi chú: ListSemestersRequest là DTO request API dùng để lọc học kỳ theo năm học và phân trang.
/// </summary>
public sealed record ListSemestersRequest(Guid? AcademicYearId, int? Page, int? PageSize, string? Search);

/// <summary>
/// Ghi chú: SemesterDto là DTO response API chứa thông tin học kỳ.
/// </summary>
public sealed record SemesterDto(
    Guid Id,
    Guid AcademicYearId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    DateOnly GradeEntryFrom,
    DateOnly GradeEntryTo,
    string Status);

/// <summary>
/// Ghi chú: CreateSubjectRequest là DTO request API dùng để tạo môn học.
/// </summary>
public sealed record CreateSubjectRequest(string SubjectCode, string Name, int Credits, decimal? MaxScore);

/// <summary>
/// Ghi chú: ListSubjectsRequest là DTO request API dùng để lọc môn học theo trạng thái và phân trang.
/// </summary>
public sealed record ListSubjectsRequest(bool? IsActive, int? Page, int? PageSize, string? Search);

/// <summary>
/// Ghi chú: UpdateSubjectRequest là DTO request API dùng để cập nhật môn học.
/// </summary>
public sealed record UpdateSubjectRequest(string Name, int Credits, decimal MaxScore);

/// <summary>
/// Ghi chú: DisableSubjectRequest là DTO request API dùng để vô hiệu hóa môn học theo id trên route.
/// </summary>
public sealed record DisableSubjectRequest(Guid Id);

/// <summary>
/// Ghi chú: SubjectDto là DTO response API chứa thông tin môn học.
/// </summary>
public sealed record SubjectDto(Guid Id, string SubjectCode, string Name, int Credits, decimal MaxScore, bool IsActive);
