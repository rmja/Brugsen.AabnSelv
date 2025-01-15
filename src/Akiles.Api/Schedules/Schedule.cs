namespace Akiles.Api.Schedules;

public record Schedule
{
    public string Id { get; set; } = null!;
    public required string OrganizationId { get; set; }
    public required string Name { get; set; }
    public WeekdayList<ScheduleWeekday> Weekdays { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
