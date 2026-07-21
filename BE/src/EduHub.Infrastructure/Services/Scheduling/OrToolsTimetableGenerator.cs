using EduHub.Application.Interfaces.Services.Scheduling;
using EduHub.Domain.Enums;
using Google.OrTools.Sat;

namespace EduHub.Infrastructure.Services.Scheduling;

/// <summary>
/// Ghi chú: OrToolsTimetableGenerator dùng CP-SAT để xếp tiết, chống trùng lớp/giáo viên và giữ rule tiết đôi liền nhau.
/// </summary>
public sealed class OrToolsTimetableGenerator : ITimetableGenerator
{
    private static readonly List<ScheduleSlot> Slots = BuildSlots();

    /// <summary>
    /// Ghi chú: Generate tạo mô hình constraint cho toàn trường và trả lỗi khi không tồn tại phương án khả thi.
    /// </summary>
    public TimetableGenerationResult Generate(IReadOnlyList<TimetableGenerationRequirement> requirements)
    {
        var fixedPlacements = requirements
            .Where(requirement => requirement.IncludesHomeroom && requirement.RequiredPeriods > 0)
            .Select(requirement => new TimetableGenerationPlacement(
                requirement.ClassRoomId,
                requirement.SubjectId,
                requirement.HomeroomTeacherId,
                requirement.WeekNumber,
                6,
                TimetableSession.Morning,
                5,
                true))
            .ToList();

        var lessons = requirements.SelectMany((requirement, requirementIndex) =>
        {
            var remaining = requirement.RequiredPeriods - (requirement.IncludesHomeroom && requirement.RequiredPeriods > 0 ? 1 : 0);
            return Enumerable.Range(0, Math.Max(remaining, 0)).Select(occurrence => new Lesson(requirementIndex, occurrence, requirement));
        }).ToList();

        var model = new CpModel();
        var variables = new Dictionary<(int LessonIndex, int SlotIndex), BoolVar>();
        var objectiveVariables = new List<IntVar>();
        var objectiveWeights = new List<long>();

        for (var lessonIndex = 0; lessonIndex < lessons.Count; lessonIndex++)
        {
            var lesson = lessons[lessonIndex];
            var allowed = new List<ILiteral>();
            for (var slotIndex = 0; slotIndex < Slots.Count; slotIndex++)
            {
                var slot = Slots[slotIndex];
                var variable = model.NewBoolVar($"lesson_{lessonIndex}_slot_{slotIndex}");
                variables[(lessonIndex, slotIndex)] = variable;
                allowed.Add(variable);
                var preferencePenalty = lesson.Requirement.PreferredSession switch
                {
                    TimetableSession.Morning when slot.Session == TimetableSession.Afternoon => 6,
                    TimetableSession.Afternoon when slot.Session == TimetableSession.Morning => 6,
                    null when slot.Session == TimetableSession.Afternoon => 1,
                    _ => 0
                };
                if (preferencePenalty > 0)
                {
                    objectiveVariables.Add(variable);
                    objectiveWeights.Add(preferencePenalty);
                }
            }

            model.AddExactlyOne(allowed);
        }

        AddClassAndTeacherConflictConstraints(model, variables, lessons, fixedPlacements);
        AddClassSessionCompletenessConstraints(model, variables, lessons);
        AddSubjectDailyConstraints(model, variables, lessons, objectiveVariables, objectiveWeights);

        if (objectiveVariables.Count > 0)
        {
            model.Minimize(LinearExpr.WeightedSum(objectiveVariables, objectiveWeights));
        }

        var solver = new CpSolver
        {
            StringParameters = "max_time_in_seconds:45 num_search_workers:8 random_seed:20260715"
        };
        var status = solver.Solve(model);
        if (status is not (CpSolverStatus.Optimal or CpSolverStatus.Feasible))
        {
            return new TimetableGenerationResult(false, "Không thể thỏa đồng thời quota môn, slot lớp và lịch giáo viên. Hãy bổ sung giáo viên hoặc nới soft constraints.", []);
        }

        var placements = new List<TimetableGenerationPlacement>(fixedPlacements);
        for (var lessonIndex = 0; lessonIndex < lessons.Count; lessonIndex++)
        {
            for (var slotIndex = 0; slotIndex < Slots.Count; slotIndex++)
            {
                if (!variables.TryGetValue((lessonIndex, slotIndex), out var variable) || !solver.BooleanValue(variable)) continue;
                var lesson = lessons[lessonIndex];
                var slot = Slots[slotIndex];
                placements.Add(new TimetableGenerationPlacement(
                    lesson.Requirement.ClassRoomId,
                    lesson.Requirement.SubjectId,
                    lesson.Requirement.TeacherId,
                    lesson.Requirement.WeekNumber,
                    slot.DayOfWeek,
                    slot.Session,
                    slot.PeriodNumber,
                    false));
                break;
            }
        }

        return new TimetableGenerationResult(
            true,
            null,
            placements.OrderBy(placement => placement.ClassRoomId).ThenBy(placement => placement.WeekNumber)
                .ThenBy(placement => placement.DayOfWeek).ThenBy(placement => placement.PeriodNumber).ToList());
    }

    /// <summary>
    /// Ghi chú: AddClassAndTeacherConflictConstraints ngăn hai môn chiếm cùng slot của lớp hoặc cùng slot của giáo viên.
    /// </summary>
    private static void AddClassAndTeacherConflictConstraints(
        CpModel model,
        Dictionary<(int LessonIndex, int SlotIndex), BoolVar> variables,
        IReadOnlyList<Lesson> lessons,
        IReadOnlyList<TimetableGenerationPlacement> fixedPlacements)
    {
        foreach (var weekNumber in lessons.Select(lesson => lesson.Requirement.WeekNumber)
                     .Concat(fixedPlacements.Select(placement => placement.WeekNumber)).Distinct().Order())
        {
            for (var slotIndex = 0; slotIndex < Slots.Count; slotIndex++)
            {
                var slot = Slots[slotIndex];
                foreach (var classRoomId in lessons.Where(lesson => lesson.Requirement.WeekNumber == weekNumber).Select(lesson => lesson.Requirement.ClassRoomId).Distinct())
                {
                    var literals = GetSlotVariables(variables, lessons, slotIndex, lesson => lesson.Requirement.WeekNumber == weekNumber && lesson.Requirement.ClassRoomId == classRoomId);
                    if (literals.Count > 1) model.AddAtMostOne(literals);
                }

                foreach (var teacherId in lessons.Where(lesson => lesson.Requirement.WeekNumber == weekNumber && lesson.Requirement.TeacherId.HasValue)
                             .Select(lesson => lesson.Requirement.TeacherId!.Value).Distinct())
                {
                    var literals = GetSlotVariables(variables, lessons, slotIndex, lesson => lesson.Requirement.WeekNumber == weekNumber && lesson.Requirement.TeacherId == teacherId);
                    if (literals.Count > 1) model.AddAtMostOne(literals);
                }

                var fixedSlot = fixedPlacements.Any(placement => placement.WeekNumber == weekNumber && placement.DayOfWeek == slot.DayOfWeek && placement.Session == slot.Session && placement.PeriodNumber == slot.PeriodNumber);
                if (fixedSlot)
                {
                    foreach (var fixedPlacement in fixedPlacements.Where(placement => placement.WeekNumber == weekNumber && placement.DayOfWeek == slot.DayOfWeek && placement.Session == slot.Session && placement.PeriodNumber == slot.PeriodNumber))
                    {
                        var classLiterals = GetSlotVariables(variables, lessons, slotIndex, lesson => lesson.Requirement.WeekNumber == weekNumber && lesson.Requirement.ClassRoomId == fixedPlacement.ClassRoomId);
                        foreach (var literal in classLiterals) model.AddBoolOr([literal.Not()]);
                        if (fixedPlacement.TeacherId.HasValue)
                        {
                            var teacherLiterals = GetSlotVariables(variables, lessons, slotIndex, lesson => lesson.Requirement.WeekNumber == weekNumber && lesson.Requirement.TeacherId == fixedPlacement.TeacherId);
                            foreach (var literal in teacherLiterals) model.AddBoolOr([literal.Not()]);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Ghi chú: AddClassSessionCompletenessConstraints bắt buộc lớp học đủ mọi tiết sáng và nếu mở buổi chiều thì phải học đủ năm tiết liên tục.
    /// </summary>
    private static void AddClassSessionCompletenessConstraints(
        CpModel model,
        Dictionary<(int LessonIndex, int SlotIndex), BoolVar> variables,
        IReadOnlyList<Lesson> lessons)
    {
        foreach (var group in lessons.Select((lesson, index) => new { Lesson = lesson, Index = index })
                     .GroupBy(item => new { item.Lesson.Requirement.ClassRoomId, item.Lesson.Requirement.WeekNumber }))
        {
            foreach (var slotIndex in Enumerable.Range(0, Slots.Count).Where(index => Slots[index].Session == TimetableSession.Morning))
            {
                var literals = group.Select(item => (ILiteral)variables[(item.Index, slotIndex)]).ToList();
                model.AddExactlyOne(literals);
            }

            foreach (var day in Enumerable.Range(1, 5))
            {
                var firstSlotIndex = Slots.FindIndex(slot => slot.DayOfWeek == day && slot.Session == TimetableSession.Afternoon && slot.PeriodNumber == 1);
                var firstPeriod = group.Select(item => (IntVar)variables[(item.Index, firstSlotIndex)]).ToList();
                for (var periodNumber = 2; periodNumber <= 5; periodNumber++)
                {
                    var slotIndex = Slots.FindIndex(slot => slot.DayOfWeek == day && slot.Session == TimetableSession.Afternoon && slot.PeriodNumber == periodNumber);
                    var currentPeriod = group.Select(item => (IntVar)variables[(item.Index, slotIndex)]).ToList();
                    model.Add(LinearExpr.Sum(currentPeriod) == LinearExpr.Sum(firstPeriod));
                }
            }
        }
    }

    /// <summary>
    /// Ghi chú: AddSubjectDailyConstraints giới hạn hai tiết/môn/ngày, bắt buộc tiết đôi liền nhau và phạt lịch bị dồn.
    /// </summary>
    private static void AddSubjectDailyConstraints(
        CpModel model,
        Dictionary<(int LessonIndex, int SlotIndex), BoolVar> variables,
        IReadOnlyList<Lesson> lessons,
        List<IntVar> objectiveVariables,
        List<long> objectiveWeights)
    {
        foreach (var group in lessons.Select((lesson, index) => new { Lesson = lesson, Index = index })
                     .GroupBy(item => new { item.Lesson.Requirement.ClassRoomId, item.Lesson.Requirement.SubjectId, item.Lesson.Requirement.WeekNumber }))
        {
            var requirement = group.First().Lesson.Requirement;
            foreach (var day in Enumerable.Range(1, 6))
            {
                var dayVariables = new List<IntVar>();
                foreach (var item in group)
                {
                    foreach (var slotIndex in Enumerable.Range(0, Slots.Count).Where(index => Slots[index].DayOfWeek == day))
                    {
                        dayVariables.Add(variables[(item.Index, slotIndex)]);
                    }
                }

                if (dayVariables.Count == 0) continue;
                var maxPerDay = requirement.CanDoublePeriod ? requirement.MaxPeriodsPerDay : 1;
                model.Add(LinearExpr.Sum(dayVariables) <= maxPerDay);
                if (maxPerDay > 1)
                {
                    var doubled = model.NewBoolVar($"double_{group.Key.ClassRoomId}_{group.Key.SubjectId}_{group.Key.WeekNumber}_{day}");
                    model.Add(LinearExpr.Sum(dayVariables) >= 2).OnlyEnforceIf(doubled);
                    model.Add(LinearExpr.Sum(dayVariables) <= 1).OnlyEnforceIf(doubled.Not());
                    objectiveVariables.Add(doubled);
                    objectiveWeights.Add(2);
                }
            }

            var indexed = group.ToList();
            for (var left = 0; left < indexed.Count; left++)
            {
                for (var right = left + 1; right < indexed.Count; right++)
                {
                    for (var leftSlot = 0; leftSlot < Slots.Count; leftSlot++)
                    {
                        for (var rightSlot = 0; rightSlot < Slots.Count; rightSlot++)
                        {
                            var first = Slots[leftSlot];
                            var second = Slots[rightSlot];
                            if (first.DayOfWeek != second.DayOfWeek) continue;
                            var adjacent = first.Session == second.Session && Math.Abs(first.PeriodNumber - second.PeriodNumber) == 1;
                            if (!adjacent)
                            {
                                model.AddBoolOr([
                                    variables[(indexed[left].Index, leftSlot)].Not(),
                                    variables[(indexed[right].Index, rightSlot)].Not()
                                ]);
                            }
                        }
                    }
                }
            }
        }

        foreach (var teacherGroup in lessons.Select((lesson, index) => new { Lesson = lesson, Index = index })
                     .Where(item => item.Lesson.Requirement.TeacherId.HasValue)
                     .GroupBy(item => new { TeacherId = item.Lesson.Requirement.TeacherId!.Value, item.Lesson.Requirement.WeekNumber }))
        {
            foreach (var day in Enumerable.Range(1, 6))
            {
                var variablesForDay = teacherGroup.SelectMany(item => Enumerable.Range(0, Slots.Count)
                        .Where(slotIndex => Slots[slotIndex].DayOfWeek == day)
                        .Select(slotIndex => (IntVar)variables[(item.Index, slotIndex)]))
                    .ToList();
                if (variablesForDay.Count == 0) continue;
                var dailyLoad = LinearExpr.Sum(variablesForDay);
                model.Add(dailyLoad <= 5);
                var heavyDay = model.NewBoolVar($"teacher_heavy_{teacherGroup.Key.TeacherId}_{teacherGroup.Key.WeekNumber}_{day}");
                model.Add(dailyLoad >= 4).OnlyEnforceIf(heavyDay);
                model.Add(dailyLoad <= 3).OnlyEnforceIf(heavyDay.Not());
                objectiveVariables.Add(heavyDay);
                objectiveWeights.Add(4);
            }
        }
    }

    /// <summary>
    /// Ghi chú: GetSlotVariables lấy các biến lesson có thể chiếm một slot theo điều kiện lớp hoặc giáo viên.
    /// </summary>
    private static List<ILiteral> GetSlotVariables(
        Dictionary<(int LessonIndex, int SlotIndex), BoolVar> variables,
        IReadOnlyList<Lesson> lessons,
        int slotIndex,
        Func<Lesson, bool> predicate) =>
        lessons.Select((lesson, index) => new { lesson, index })
            .Where(item => predicate(item.lesson))
            .Select(item => (ILiteral)variables[(item.index, slotIndex)])
            .ToList();

    /// <summary>
    /// Ghi chú: BuildSlots tạo slot sáng/chiều, trong đó thứ Tư sáng bốn tiết và thứ Bảy không có buổi chiều.
    /// </summary>
    private static List<ScheduleSlot> BuildSlots()
    {
        var result = new List<ScheduleSlot>();
        for (var day = 1; day <= 6; day++)
        {
            var morningPeriods = day is 3 or 6 ? 4 : 5;
            for (var period = 1; period <= morningPeriods; period++) result.Add(new ScheduleSlot(day, TimetableSession.Morning, period));
            if (day <= 5)
            {
                for (var period = 1; period <= 5; period++) result.Add(new ScheduleSlot(day, TimetableSession.Afternoon, period));
            }
        }

        return result;
    }

    private sealed record Lesson(int RequirementIndex, int Occurrence, TimetableGenerationRequirement Requirement);
    private sealed record ScheduleSlot(int DayOfWeek, TimetableSession Session, int PeriodNumber);
}
