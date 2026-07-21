using EduHub.Domain.Enums;

namespace EduHub.Application.Interfaces.Services.Scheduling;

/// <summary>
/// Ghi chú: TimetableGenerationRequirement mô tả số tiết cần xếp cho một lớp-môn-giáo viên trong một tuần thực học.
/// </summary>
public sealed record TimetableGenerationRequirement(
    Guid ClassRoomId,
    Guid SubjectId,
    Guid? TeacherId,
    int WeekNumber,
    int RequiredPeriods,
    bool CanDoublePeriod,
    int MaxPeriodsPerDay,
    bool IncludesHomeroom,
    Guid? HomeroomTeacherId,
    TimetableSession? PreferredSession);

/// <summary>
/// Ghi chú: TimetableGenerationPlacement là slot do thuật toán xếp lịch chọn cho một tiết học.
/// </summary>
public sealed record TimetableGenerationPlacement(
    Guid ClassRoomId,
    Guid SubjectId,
    Guid? TeacherId,
    int WeekNumber,
    int DayOfWeek,
    TimetableSession Session,
    int PeriodNumber,
    bool IsHomeroom);

/// <summary>
/// Ghi chú: TimetableGenerationResult trả các slot khả thi hoặc lý do không thể sinh thời khóa biểu.
/// </summary>
public sealed record TimetableGenerationResult(
    bool Success,
    string? FailureReason,
    IReadOnlyList<TimetableGenerationPlacement> Placements);

/// <summary>
/// Ghi chú: ITimetableGenerator là interface thuật toán constraint solver dùng để xếp tiết không trùng lớp hoặc giáo viên.
/// </summary>
public interface ITimetableGenerator
{
    /// <summary>
    /// Ghi chú: Generate nhận nhu cầu lớp-môn của toàn học kỳ và trả thời khóa biểu từng tuần thỏa hard constraints.
    /// </summary>
    TimetableGenerationResult Generate(IReadOnlyList<TimetableGenerationRequirement> requirements);
}
