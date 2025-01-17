using Akiles.Api.Schedules;

namespace Brugsen.AabnSelv;

public static class ScheduleExtensions
{
    /// <summary>
    /// Get the schedule range, if any, that corresponds to <paramref name="time"/>.
    /// </summary>
    public static ScheduleRange? GetCurrentRange(this Schedule schedule, DateTime time)
    {
        var timeOfDay = TimeOnly.FromTimeSpan(time.TimeOfDay);
        return schedule
            .Weekdays[time.DayOfWeek]
            .Ranges.OrderBy(x => x.Start)
            .FirstOrDefault(x => timeOfDay >= x.Start && timeOfDay < x.End);
    }

    public static ScheduleRangePeriod? GetCurrentPeriod(this Schedule schedule, DateTime time)
    {
        var range = schedule.GetCurrentRange(time);
        return range?.ToPeriod(DateOnly.FromDateTime(time.Date));
    }

    /// <summary>
    /// Get all periods that correspond to the schedule ranges, where the first returned
    /// period starts no earlier than <paramref name="startNotBefore"/>.
    /// </summary>
    /// <param name="schedule"></param>
    /// <param name="startNotBefore"></param>
    /// <returns></returns>
    public static IEnumerable<ScheduleRangePeriod> GetPeriods(
        this Schedule schedule,
        DateTime startNotBefore
    )
    {
        var date = DateOnly.FromDateTime(startNotBefore.Date);
        var timeOfDay = TimeOnly.FromTimeSpan(startNotBefore.TimeOfDay);

        if (!schedule.Weekdays.SelectMany(x => x.Ranges).Any())
        {
            yield break;
        }

        // The reminder of present day
        foreach (
            var range in schedule
                .Weekdays[startNotBefore.DayOfWeek]
                .Ranges.OrderBy(x => x.Start)
                .Where(x => x.Start >= timeOfDay)
        )
        {
            yield return range.ToPeriod(date);
        }

        // Later days
        while (true)
        {
            date = date.AddDays(1);
            foreach (var range in schedule.Weekdays[date.DayOfWeek].Ranges.OrderBy(x => x.Start))
            {
                yield return range.ToPeriod(date);
            }
        }
    }

    private static ScheduleRangePeriod ToPeriod(this ScheduleRange range, DateOnly date)
    {
        var start = date.ToDateTime(range.Start);
        var end = date.ToDateTime(range.End);
        return new(start, end);
    }
}

public record ScheduleRangePeriod(DateTime Start, DateTime End);
