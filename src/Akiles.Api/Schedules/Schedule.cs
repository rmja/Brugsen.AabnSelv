namespace Akiles.Api.Schedules;

public record Schedule
{
    public string Id { get; set; } = null!;
    public required string OrganizationId { get; set; }
    public required string Name { get; set; }
    public WeekdayList<ScheduleWeekday> Weekdays { get; set; } = [];
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
}
