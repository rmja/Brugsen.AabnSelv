namespace Akiles.Api.Schedules;

public class FakeSchedules : ISchedules
{
    private static readonly Dictionary<string, Schedule> _schedules = GetSchedules()
        .ToDictionary(x => x.Id, x => x);

    public Task<Schedule> GetScheduleAsync(
        string scheduleId,
        CancellationToken cancellationToken = default
    )
    {
        var schedule = _schedules[scheduleId];
        return Task.FromResult(schedule);
    }

    private static IEnumerable<Schedule> GetSchedules()
    {
        var schedule = new Schedule
        {
            Id = "sch_41fm7t77cmm31j8jdd91",
            OrganizationId = "",
            Name = "Normale åbningstider"
        };
        schedule
            .Weekdays[DayOfWeek.Monday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(17, 30)));
        schedule
            .Weekdays[DayOfWeek.Tuesday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(17, 30)));
        schedule
            .Weekdays[DayOfWeek.Wednesday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(17, 30)));
        schedule
            .Weekdays[DayOfWeek.Thursday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(19, 00)));
        schedule
            .Weekdays[DayOfWeek.Friday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(19, 00)));
        schedule
            .Weekdays[DayOfWeek.Saturday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(13, 30)));
        schedule
            .Weekdays[DayOfWeek.Sunday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(13, 30)));
        yield return schedule;

        schedule = new Schedule
        {
            Id = "sch_41djhl5mae8q65bysuxh",
            OrganizationId = "",
            Name = "ÅBN SELV åbningstider"
        };
        foreach (var weekday in Enum.GetValues<DayOfWeek>())
        {
            schedule.Weekdays[weekday].Ranges.Add(new(new TimeOnly(05, 00), new TimeOnly(23, 00)));
        }
        yield return schedule;
    }
}
