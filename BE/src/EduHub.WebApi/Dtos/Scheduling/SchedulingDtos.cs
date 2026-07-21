using EduHub.Domain.Enums;

namespace EduHub.WebApi.Dtos.Scheduling;

/// <summary>
/// Ghi chú: CurriculumSubjectQuotaRequest là dữ liệu quota một môn do học vụ nhập cho chương trình khối.
/// </summary>
public sealed record CurriculumSubjectQuotaRequest(
    Guid SubjectId,
    CurriculumSubjectKind Kind,
    int AnnualPeriods,
    int Semester1Periods,
    int Semester2Periods,
    bool CanDoublePeriod,
    int MaxPeriodsPerDay,
    bool IncludesHomeroom,
    TimetableSession? PreferredSession);

/// <summary>
/// Ghi chú: CreateCurriculumPlanRequest là DTO tạo chương trình học cho một khối trong năm học.
/// </summary>
public sealed record CreateCurriculumPlanRequest(
    Guid AcademicYearId,
    int GradeLevel,
    string Name,
    int TotalWeeks,
    int Semester1Weeks,
    int Semester2Weeks,
    IReadOnlyList<CurriculumSubjectQuotaRequest> SubjectQuotas);

/// <summary>
/// Ghi chú: ListCurriculumPlansRequest là bộ lọc chương trình theo năm học hoặc khối.
/// </summary>
public sealed record ListCurriculumPlansRequest(Guid? AcademicYearId, int? GradeLevel);

/// <summary>
/// Ghi chú: CurriculumSubjectQuotaDto trả quota môn học trên màn hình cấu hình chương trình.
/// </summary>
public sealed record CurriculumSubjectQuotaDto(
    Guid Id,
    Guid SubjectId,
    string SubjectCode,
    string SubjectName,
    string Kind,
    int AnnualPeriods,
    int Semester1Periods,
    int Semester2Periods,
    bool CanDoublePeriod,
    int MaxPeriodsPerDay,
    bool IncludesHomeroom,
    string? PreferredSession);

/// <summary>
/// Ghi chú: CurriculumPlanDto trả chương trình 35 tuần và danh sách quota môn của khối.
/// </summary>
public sealed record CurriculumPlanDto(
    Guid Id,
    Guid AcademicYearId,
    int GradeLevel,
    string Name,
    int TotalWeeks,
    int Semester1Weeks,
    int Semester2Weeks,
    int AnnualPeriodTotal,
    bool IsActive,
    IReadOnlyList<CurriculumSubjectQuotaDto> SubjectQuotas);

/// <summary>
/// Ghi chú: CreateTeacherCapabilityRequest là DTO khai báo môn chính hoặc phụ của giáo viên.
/// </summary>
public sealed record CreateTeacherCapabilityRequest(Guid TeacherId, Guid SubjectId, TeacherSubjectPriority Priority, int MaxPeriodsPerWeek);

/// <summary>
/// Ghi chú: ListTeacherCapabilitiesRequest là bộ lọc năng lực theo giáo viên hoặc môn.
/// </summary>
public sealed record ListTeacherCapabilitiesRequest(Guid? TeacherId, Guid? SubjectId);

/// <summary>
/// Ghi chú: TeacherCapabilityDto trả năng lực và tải dạy tối đa của giáo viên.
/// </summary>
public sealed record TeacherCapabilityDto(Guid Id, Guid TeacherId, string TeacherName, Guid SubjectId, string SubjectCode, string SubjectName, string Priority, int MaxPeriodsPerWeek, bool IsActive);

/// <summary>
/// Ghi chú: AssignHomeroomTeacherRequest là DTO chọn giáo viên chủ nhiệm cho lớp trên route.
/// </summary>
public sealed record AssignHomeroomTeacherRequest(Guid TeacherId);

/// <summary>
/// Ghi chú: ListHomeroomAssignmentsRequest là bộ lọc GVCN theo năm học.
/// </summary>
public sealed record ListHomeroomAssignmentsRequest(Guid? AcademicYearId);

/// <summary>
/// Ghi chú: HomeroomAssignmentDto trả lớp và giáo viên chủ nhiệm đang phụ trách.
/// </summary>
public sealed record HomeroomAssignmentDto(Guid Id, Guid ClassRoomId, string ClassCode, string ClassName, Guid TeacherId, string TeacherName, bool IsActive);

/// <summary>
/// Ghi chú: GenerateTimetableRequest là DTO yêu cầu sinh bản nháp thời khóa biểu cho học kỳ.
/// </summary>
public sealed record GenerateTimetableRequest(Guid SemesterId, string Name);

/// <summary>
/// Ghi chú: TimetableVersionDto trả trạng thái và số tiết của một phiên bản thời khóa biểu.
/// </summary>
public sealed record TimetableVersionDto(Guid Id, Guid SemesterId, string SemesterName, string Name, string Status, DateTime GeneratedAtUtc, DateTime? PublishedAtUtc, int EntryCount);

/// <summary>
/// Ghi chú: GenerateTimetableDto trả kết quả sinh lịch và số phân công được tạo tự động.
/// </summary>
public sealed record GenerateTimetableDto(TimetableVersionDto Version, int AutoCreatedTeachingAssignments, int AutoCreatedHomeroomAssignments, int EntryCount);

/// <summary>
/// Ghi chú: TimetableWeekDto trả số tuần, khoảng ngày và trạng thái tuần hiện tại cho bộ chọn lịch.
/// </summary>
public sealed record TimetableWeekDto(int WeekNumber, DateOnly StartDate, DateOnly EndDate, bool IsCurrent);

/// <summary>
/// Ghi chú: TimetableEntryDto trả một tiết học thuộc tuần thực tế, ngày, buổi và khung giờ cụ thể.
/// </summary>
public sealed record TimetableEntryDto(
    Guid Id,
    Guid TimetableVersionId,
    Guid ClassRoomId,
    string ClassCode,
    string ClassName,
    Guid SubjectId,
    string SubjectCode,
    string SubjectName,
    Guid? TeacherId,
    string? TeacherName,
    int WeekNumber,
    DateOnly WeekStartDate,
    DateOnly WeekEndDate,
    int DayOfWeek,
    string Session,
    int PeriodNumber,
    string StartTime,
    string EndTime,
    string Kind,
    bool CountsTowardQuota,
    bool IsLocked,
    string? Note);

/// <summary>
/// Ghi chú: GetTimetableEntriesRequest là bộ lọc tiết theo lớp trong một phiên bản.
/// </summary>
public sealed record GetTimetableEntriesRequest(Guid? ClassRoomId, int? WeekNumber);

/// <summary>
/// Ghi chú: MoveTimetableEntryRequest là DTO chuyển tiết sang tuần, ngày, buổi và số tiết khác.
/// </summary>
public sealed record MoveTimetableEntryRequest(int WeekNumber, int DayOfWeek, TimetableSession Session, int PeriodNumber);

/// <summary>
/// Ghi chú: AssignClassSubjectTeacherRequest chọn giáo viên cho toàn bộ môn của lớp dựa trên ô lịch đang mở.
/// </summary>
public sealed record AssignClassSubjectTeacherRequest(Guid TeacherId);

/// <summary>
/// Ghi chú: SetTimetableEntryLockRequest là DTO khóa hoặc mở khóa tiết đã chỉnh tay.
/// </summary>
public sealed record SetTimetableEntryLockRequest(bool IsLocked);
