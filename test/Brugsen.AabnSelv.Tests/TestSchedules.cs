using Akiles.ApiClient.Schedules;

namespace Brugsen.AabnSelv.Tests;

internal static class TestSchedules
{
    public static Schedule GetRegularOpeningHoursSchedule()
    {
        var schedule = new Schedule
        {
            Id = "regular_opening_hours",
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
        return schedule;
    }

    public static Schedule GetExtendedOpeningHoursSchedule()
    {
        var schedule = new Schedule
        {
            Id = "extended_opening_hours",
            OrganizationId = "",
            Name = "ÅBN SELV åbningstider"
        };
        foreach (var weekday in Enum.GetValues<DayOfWeek>())
        {
            schedule.Weekdays[weekday].Ranges.Add(new(new TimeOnly(05, 00), new TimeOnly(23, 00)));
        }
        return schedule;
    }
}
