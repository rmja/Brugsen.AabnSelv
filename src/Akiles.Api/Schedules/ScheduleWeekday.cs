namespace Akiles.Api.Schedules;

public record ScheduleWeekday
{
    public List<ScheduleRange> Ranges { get; set; } = [];
}
