namespace Akiles.Api.Schedules;

public class WeekdayList<T> : List<T>
    where T : new()
{
    public T this[DayOfWeek weekday] => this[GetIndex(weekday)];

    private static int GetIndex(DayOfWeek weekday) =>
        weekday switch
        {
            DayOfWeek.Monday => 0,
            DayOfWeek.Tuesday => 1,
            DayOfWeek.Wednesday => 2,
            DayOfWeek.Thursday => 3,
            DayOfWeek.Friday => 4,
            DayOfWeek.Saturday => 5,
            DayOfWeek.Sunday => 6,
            _ => throw new ArgumentException()
        };
}
