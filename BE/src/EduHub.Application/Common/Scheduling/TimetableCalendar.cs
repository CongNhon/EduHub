using EduHub.Domain.Enums;

namespace EduHub.Application.Common.Scheduling;

/// <summary>
/// Ghi chú: TimetableCalendar tập trung cách tính ngày tuần học và thời gian bắt đầu/kết thúc từng tiết của trường.
/// </summary>
public static class TimetableCalendar
{
    private static readonly TimeOnly MorningStart = new(7, 15);
    private static readonly TimeOnly AfternoonStart = new(13, 15);
    private const int LessonMinutes = 45;
    private const int TransitionMinutes = 5;

    /// <summary>
    /// Ghi chú: GetTeachingStartDate đưa ngày bắt đầu học kỳ về Thứ Hai đầu tiên để tuần học luôn chạy Thứ Hai-Thứ Bảy.
    /// </summary>
    public static DateOnly GetTeachingStartDate(DateOnly semesterStartDate)
    {
        var offset = ((int)DayOfWeek.Monday - (int)semesterStartDate.DayOfWeek + 7) % 7;
        return semesterStartDate.AddDays(offset);
    }

    /// <summary>
    /// Ghi chú: GetWeekDates trả ngày Thứ Hai và Thứ Bảy của đúng số tuần trong học kỳ.
    /// </summary>
    public static (DateOnly StartDate, DateOnly EndDate) GetWeekDates(DateOnly semesterStartDate, int weekNumber)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(weekNumber, 1);
        var startDate = GetTeachingStartDate(semesterStartDate).AddDays((weekNumber - 1) * 7);
        return (startDate, startDate.AddDays(5));
    }

    /// <summary>
    /// Ghi chú: GetPeriodTimes trả giờ học 45 phút và khoảng chuyển tiết 5 phút cho buổi sáng hoặc chiều.
    /// </summary>
    public static (TimeOnly StartTime, TimeOnly EndTime) GetPeriodTimes(TimetableSession session, int periodNumber)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(periodNumber, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(periodNumber, 5);
        var sessionStart = session == TimetableSession.Morning ? MorningStart : AfternoonStart;
        var startTime = sessionStart.AddMinutes((periodNumber - 1) * (LessonMinutes + TransitionMinutes));
        return (startTime, startTime.AddMinutes(LessonMinutes));
    }
}
