using Backend.DTOS.School.WeeklySchedule;
using Backend.Interfaces;

namespace Backend.Services;
public interface IAutomaticWeeklyScheduleService
{
    Task<GenerateWeeklyScheduleResultDTO> GenerateAsync(
        GenerateWeeklyScheduleRequestDTO request,
        CancellationToken cancellationToken = default);
}

public class AutomaticWeeklyScheduleService : IAutomaticWeeklyScheduleService
{
    private readonly IUnitOfWork _unitOfWork;

    private static readonly (int PeriodNumber, string Start, string End)[] DefaultPeriodTemplates =
    {
        (1, "08:00", "08:45"),
        (2, "08:45", "09:30"),
        (3, "09:30", "10:15"),
        (4, "10:30", "11:15"),
        (5, "11:15", "12:00"),
        (6, "12:00", "12:45"),
    };

    public AutomaticWeeklyScheduleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GenerateWeeklyScheduleResultDTO> GenerateAsync(
        GenerateWeeklyScheduleRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        var result = new GenerateWeeklyScheduleResultDTO();

        var classEntity = await _unitOfWork.Classes.GetByIdAsync(request.ClassID);
        if (classEntity == null)
            throw new InvalidOperationException("Class not found.");

        int yearId;
        if (classEntity.YearID.HasValue && classEntity.YearID.Value > 0)
            yearId = classEntity.YearID.Value;
        else
        {
            var activeYearId = await _unitOfWork.Years.GetActiveYearIdAsync(cancellationToken);
            if (!activeYearId.HasValue)
                throw new InvalidOperationException("No active academic year found. Activate a year or assign one to the class.");
            yearId = activeYearId.Value;
        }

        var plans = await _unitOfWork.CoursePlans.GetByClassTermYearForSchedulingAsync(
            yearId, request.ClassID, request.TermID, request.DivisionID, cancellationToken);

        var usablePlans = plans.Where(p => p.PeriodsPerWeek > 0).ToList();
        if (usablePlans.Count == 0)
            throw new InvalidOperationException("No course plans with PeriodsPerWeek greater than zero were found for this class, term, and division.");

        var required = usablePlans.Sum(p => p.PeriodsPerWeek);
        var days = Math.Clamp(request.DaysPerWeek, 1, 7);
        var periodsPerDay = Math.Clamp(request.PeriodsPerDay, 1, 14);
        var gridSlots = days * periodsPerDay;
        result.RequiredPeriods = required;
        result.GridSlots = gridSlots;

        if (required > gridSlots)
            throw new InvalidOperationException(
                $"Total weekly periods from course plans ({required}) exceed the grid ({gridSlots} = {days} days × {periodsPerDay} periods). Increase days/periods or lower PeriodsPerWeek values.");

        var periodDefs = BuildPeriodDefinitions(periodsPerDay);
        var maxPerDay = request.MaxSameSubjectPerDay;

        var teacherBusy = await _unitOfWork.WeeklySchedules.GetTeacherBusySlotsAsync(
            yearId, request.TermID, request.ClassID, request.DivisionID, cancellationToken);

        var teacherLoad = await _unitOfWork.CoursePlans.GetTeacherCoursePlanCountsAsync(yearId, request.TermID, cancellationToken);

        List<AddWeeklyScheduleDTO>? bestSchedules = null;
        var bestUnplaced = int.MaxValue;

        var seed = request.RandomSeed ?? Environment.TickCount;

        const int maxAttempts = 48;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var rng = new Random(seed + attempt);
            var busy = new HashSet<(int TeacherId, int Day, int Period)>(teacherBusy);
            var subjectDayCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var remaining = usablePlans
                .SelectMany(p => Enumerable.Repeat(new ScheduleNeed(p.SubjectID, p.TeacherID), p.PeriodsPerWeek))
                .ToList();

            remaining.Sort((a, b) =>
            {
                var loadCompare = teacherLoad.GetValueOrDefault(b.TeacherID).CompareTo(teacherLoad.GetValueOrDefault(a.TeacherID));
                if (loadCompare != 0) return loadCompare;
                return rng.Next(-1, 2);
            });

            var dayOrder = Enumerable.Range(0, days).OrderBy(_ => rng.Next()).ToList();
            var periodOrder = Enumerable.Range(1, periodsPerDay).OrderBy(_ => rng.Next()).ToList();

            var schedules = new List<AddWeeklyScheduleDTO>();
            var unplaced = new List<ScheduleNeed>(remaining);

            foreach (var day in dayOrder)
            {
                foreach (var period in periodOrder)
                {
                    var def = periodDefs.First(d => d.PeriodNumber == period);
                    var picks = unplaced
                        .Select((need, i) => (need, i))
                        .Where(x => !busy.Contains((x.need.TeacherID, day, period)) &&
                                    (maxPerDay <= 0 || SubjectDayCount(subjectDayCounts, x.need.SubjectID, day) < maxPerDay))
                        .OrderByDescending(x => teacherLoad.GetValueOrDefault(x.need.TeacherID))
                        .ThenBy(_ => rng.Next())
                        .ToList();

                    if (picks.Count == 0)
                        continue;

                    var idx = picks[0].i;
                    var chosen = unplaced[idx];
                    unplaced.RemoveAt(idx);
                    busy.Add((chosen.TeacherID, day, period));
                    IncrementSubjectDay(subjectDayCounts, chosen.SubjectID, day);

                    schedules.Add(new AddWeeklyScheduleDTO
                    {
                        DayOfWeek = day,
                        PeriodNumber = period,
                        StartTime = def.Start,
                        EndTime = def.End,
                        ClassID = request.ClassID,
                        TermID = request.TermID,
                        YearID = yearId,
                        SubjectID = chosen.SubjectID,
                        TeacherID = chosen.TeacherID,
                        DivisionID = request.DivisionID
                    });
                }
            }

            if (unplaced.Count < bestUnplaced)
            {
                bestUnplaced = unplaced.Count;
                bestSchedules = schedules;
            }

            if (unplaced.Count == 0)
            {
                bestSchedules = schedules;
                break;
            }
        }

        if (bestSchedules == null)
            throw new InvalidOperationException("Generation failed unexpectedly.");

        result.PlacedPeriods = bestSchedules.Count;
        result.Success = bestUnplaced == 0;

        if (bestUnplaced > 0)
        {
            result.Warnings.Add(
                $"Could not place {bestUnplaced} period(s) after {maxAttempts} attempts. Try more grid slots, relax MaxSameSubjectPerDay, resolve teacher clashes in other classes, or adjust PeriodsPerWeek. The timetable was not saved.");
            var grouped = usablePlans
                .GroupBy(p => new { p.SubjectID, p.TeacherID })
                .Select(g => new { g.Key.SubjectID, g.Key.TeacherID, Count = g.Sum(x => x.PeriodsPerWeek) })
                .ToList();
            foreach (var g in grouped)
                result.UnplacedLines.Add($"SubjectID {g.SubjectID}, TeacherID {g.TeacherID}: {g.Count} required (approximate; partial placement).");
            return result;
        }

        await _unitOfWork.WeeklySchedules.BulkUpdateAsync(bestSchedules);

        return result;
    }

    private static int SubjectDayCount(Dictionary<string, int> map, int subjectId, int day) =>
        map.TryGetValue($"{subjectId}_{day}", out var c) ? c : 0;

    private static void IncrementSubjectDay(Dictionary<string, int> map, int subjectId, int day)
    {
        var key = $"{subjectId}_{day}";
        map[key] = map.TryGetValue(key, out var c) ? c + 1 : 1;
    }

    private static List<(int PeriodNumber, string Start, string End)> BuildPeriodDefinitions(int periodsPerDay)
    {
        var list = new List<(int, string, string)>();
        for (var i = 1; i <= periodsPerDay; i++)
        {
            if (i <= DefaultPeriodTemplates.Length)
            {
                var t = DefaultPeriodTemplates[i - 1];
                list.Add((t.PeriodNumber, t.Start, t.End));
            }
            else
            {
                var startMin = 8 * 60 + (i - 1) * 45;
                var endMin = startMin + 45;
                list.Add((i, MinutesToHHmm(startMin), MinutesToHHmm(endMin)));
            }
        }

        return list;
    }

    private static string MinutesToHHmm(int totalMinutes)
    {
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        return $"{h:00}:{m:00}";
    }

    private sealed class ScheduleNeed
    {
        public ScheduleNeed(int subjectId, int teacherId)
        {
            SubjectID = subjectId;
            TeacherID = teacherId;
        }

        public int SubjectID { get; }
        public int TeacherID { get; }
    }
}
