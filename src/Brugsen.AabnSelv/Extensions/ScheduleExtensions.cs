using Akiles.ApiClient.Schedules;

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

    public static ScheduleRangePeriod? GetCurrentPeriod(this Schedule schedule, DateTime time) =>
        schedule.GetCurrentRange(time)?.ToPeriod(DateOnly.FromDateTime(time.Date));

    /// <summary>
    /// Get all periods that correspond to the schedule ranges, where the first returned
    /// period starts no earlier than <paramref name="startNotBefore"/>.
    /// </summary>
    public static IEnumerable<ScheduleRangePeriod> GetLaterPeriods(
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

    /// <summary>
    /// Get all periods that correspond to the schedule ranges, where the first returned
    /// period ends before <paramref name="endNotAfter"/>.
    /// </summary>
    public static IEnumerable<ScheduleRangePeriod> GetEarlierPeriods(
        this Schedule schedule,
        DateTime endNotAfter
    )
    {
        var date = DateOnly.FromDateTime(endNotAfter.Date);
        var timeOfDay = TimeOnly.FromTimeSpan(endNotAfter.TimeOfDay);

        if (!schedule.Weekdays.SelectMany(x => x.Ranges).Any())
        {
            yield break;
        }

        // The beginning of present day
        foreach (
            var range in schedule
                .Weekdays[endNotAfter.DayOfWeek]
                .Ranges.OrderByDescending(x => x.End)
                .Where(x => x.End <= timeOfDay)
        )
        {
            yield return range.ToPeriod(date);
        }

        // Earlier days
        while (true)
        {
            date = date.AddDays(-1);
            foreach (
                var range in schedule.Weekdays[date.DayOfWeek].Ranges.OrderByDescending(x => x.End)
            )
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
