using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Domain.Enums;

namespace EduHub.Application.Contracts.Scheduling;

/// <summary>
/// Ghi chú: CurriculumSubjectQuotaInput chứa quota một môn khi quản trị học vụ tạo chương trình cho khối.
/// </summary>
public sealed record CurriculumSubjectQuotaInput(
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
/// Ghi chú: CurriculumSubjectQuotaResponse trả quota và tên môn thuộc chương trình học của khối.
/// </summary>
public sealed record CurriculumSubjectQuotaResponse(
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
/// Ghi chú: CurriculumPlanResponse trả chương trình 35 tuần và toàn bộ quota môn của một khối.
/// </summary>
public sealed record CurriculumPlanResponse(
    Guid Id,
    Guid AcademicYearId,
    int GradeLevel,
    string Name,
    int TotalWeeks,
    int Semester1Weeks,
    int Semester2Weeks,
    int AnnualPeriodTotal,
    bool IsActive,
    IReadOnlyList<CurriculumSubjectQuotaResponse> SubjectQuotas);

/// <summary>
/// Ghi chú: CreateCurriculumPlanCommand tạo chương trình và quota môn cho một khối trong năm học.
/// </summary>
public sealed record CreateCurriculumPlanCommand(
    Guid AcademicYearId,
    int GradeLevel,
    string Name,
    int TotalWeeks,
    int Semester1Weeks,
    int Semester2Weeks,
    IReadOnlyList<CurriculumSubjectQuotaInput> SubjectQuotas)
    : ICommand<Result<CurriculumPlanResponse>>;

/// <summary>
/// Ghi chú: ListCurriculumPlansQuery đọc các chương trình theo năm học hoặc khối lớp.
/// </summary>
public sealed record ListCurriculumPlansQuery(Guid? AcademicYearId, int? GradeLevel)
    : IQuery<Result<IReadOnlyList<CurriculumPlanResponse>>>;

/// <summary>
/// Ghi chú: TeacherCapabilityResponse trả môn chính hoặc phụ và tải dạy tối đa của giáo viên.
/// </summary>
public sealed record TeacherCapabilityResponse(
    Guid Id,
    Guid TeacherId,
    string TeacherName,
    Guid SubjectId,
    string SubjectCode,
    string SubjectName,
    string Priority,
    int MaxPeriodsPerWeek,
    bool IsActive);

/// <summary>
/// Ghi chú: CreateTeacherCapabilityCommand khai báo một môn giáo viên có thể được hệ thống tự động phân công.
/// </summary>
public sealed record CreateTeacherCapabilityCommand(
    Guid TeacherId,
    Guid SubjectId,
    TeacherSubjectPriority Priority,
    int MaxPeriodsPerWeek)
    : ICommand<Result<TeacherCapabilityResponse>>;

/// <summary>
/// Ghi chú: ListTeacherCapabilitiesQuery đọc năng lực giảng dạy theo giáo viên hoặc môn học.
/// </summary>
public sealed record ListTeacherCapabilitiesQuery(Guid? TeacherId, Guid? SubjectId)
    : IQuery<Result<IReadOnlyList<TeacherCapabilityResponse>>>;

/// <summary>
/// Ghi chú: HomeroomAssignmentResponse trả giáo viên chủ nhiệm đang phụ trách một lớp.
/// </summary>
public sealed record HomeroomAssignmentResponse(
    Guid Id,
    Guid ClassRoomId,
    string ClassCode,
    string ClassName,
    Guid TeacherId,
    string TeacherName,
    bool IsActive);

/// <summary>
/// Ghi chú: AssignHomeroomTeacherCommand phân công giáo viên chủ nhiệm cho lớp bằng thao tác thủ công.
/// </summary>
public sealed record AssignHomeroomTeacherCommand(Guid ClassRoomId, Guid TeacherId)
    : ICommand<Result<HomeroomAssignmentResponse>>;

/// <summary>
/// Ghi chú: ListHomeroomAssignmentsQuery đọc danh sách chủ nhiệm theo năm học.
/// </summary>
public sealed record ListHomeroomAssignmentsQuery(Guid? AcademicYearId)
    : IQuery<Result<IReadOnlyList<HomeroomAssignmentResponse>>>;

/// <summary>
/// Ghi chú: TimetableEntryResponse trả một tiết học của lớp trong tuần thực học có ngày và giờ cụ thể.
/// </summary>
public sealed record TimetableEntryResponse(
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
/// Ghi chú: TimetableVersionResponse trả thông tin phiên bản và số tiết đã xếp.
/// </summary>
public sealed record TimetableVersionResponse(
    Guid Id,
    Guid SemesterId,
    string SemesterName,
    string Name,
    string Status,
    DateTime GeneratedAtUtc,
    DateTime? PublishedAtUtc,
    int EntryCount);

/// <summary>
/// Ghi chú: GenerateTimetableResponse trả phiên bản mới và số phân công giáo viên/GVCN được tạo tự động.
/// </summary>
public sealed record GenerateTimetableResponse(
    TimetableVersionResponse Version,
    int AutoCreatedTeachingAssignments,
    int AutoCreatedHomeroomAssignments,
    int EntryCount);

/// <summary>
/// Ghi chú: TimetableWeekResponse mô tả số tuần, khoảng ngày và trạng thái tuần hiện tại trong học kỳ.
/// </summary>
public sealed record TimetableWeekResponse(
    int WeekNumber,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent);

/// <summary>
/// Ghi chú: GenerateTimetableCommand tự phân giáo viên còn thiếu và sinh thời khóa biểu theo từng tuần của học kỳ.
/// </summary>
public sealed record GenerateTimetableCommand(Guid SemesterId, string Name)
    : ICommand<Result<GenerateTimetableResponse>>;

/// <summary>
/// Ghi chú: ListTimetableVersionsQuery đọc lịch sử phiên bản thời khóa biểu của học kỳ.
/// </summary>
public sealed record ListTimetableVersionsQuery(Guid SemesterId)
    : IQuery<Result<IReadOnlyList<TimetableVersionResponse>>>;

/// <summary>
/// Ghi chú: GetPublishedTimetableVersionQuery lấy phiên bản đang công bố để các role mở lịch hiện hành.
/// </summary>
public sealed record GetPublishedTimetableVersionQuery(Guid SemesterId)
    : IQuery<Result<TimetableVersionResponse>>;

/// <summary>
/// Ghi chú: GetTimetableEntriesQuery đọc các tiết của phiên bản, có thể lọc theo lớp.
/// </summary>
public sealed record GetTimetableEntriesQuery(Guid TimetableVersionId, Guid? ClassRoomId, int? WeekNumber)
    : IQuery<Result<IReadOnlyList<TimetableEntryResponse>>>;

/// <summary>
/// Ghi chú: ListTimetableWeeksQuery lấy toàn bộ tuần thực học và đánh dấu tuần hiện tại của học kỳ.
/// </summary>
public sealed record ListTimetableWeeksQuery(Guid SemesterId)
    : IQuery<Result<IReadOnlyList<TimetableWeekResponse>>>;

/// <summary>
/// Ghi chú: PublishTimetableCommand công bố bản nháp và lưu trữ bản công bố cũ của cùng học kỳ.
/// </summary>
public sealed record PublishTimetableCommand(Guid TimetableVersionId)
    : ICommand<Result<TimetableVersionResponse>>;

/// <summary>
/// Ghi chú: MoveTimetableEntryCommand hoán đổi entry đang chọn với entry ở slot đích để lịch lớp không bị trống.
/// </summary>
public sealed record MoveTimetableEntryCommand(
    Guid TimetableEntryId,
    int WeekNumber,
    int DayOfWeek,
    TimetableSession Session,
    int PeriodNumber)
    : ICommand<Result<TimetableEntryResponse>>;

/// <summary>
/// Ghi chú: AssignClassSubjectTeacherCommand đổi giáo viên của toàn bộ lớp-môn-học kỳ dựa trên một entry trong bản nháp.
/// </summary>
public sealed record AssignClassSubjectTeacherCommand(Guid TimetableEntryId, Guid TeacherId)
    : ICommand<Result<TimetableEntryResponse>>;

/// <summary>
/// Ghi chú: SetTimetableEntryLockCommand khóa hoặc mở khóa slot để lần sinh lại không thay đổi tiết đó.
/// </summary>
public sealed record SetTimetableEntryLockCommand(Guid TimetableEntryId, bool IsLocked)
    : ICommand<Result<TimetableEntryResponse>>;
