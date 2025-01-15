using Akiles.Api.Schedules;

namespace Brugsen.AabnSelv.Tests;

public class ScheduleExtensionsTests
{
    [Fact]
    public void CanGetCurrentRange()
    {
        // Given
        var wednesday = new DateTime(2025, 01, 15, 19, 00, 00, DateTimeKind.Unspecified);
        var schedule = new Schedule() { Name = "", OrganizationId = "" };
        schedule
            .Weekdays[DayOfWeek.Wednesday]
            .Ranges.Add(new(new TimeOnly(19, 00), new TimeOnly(20, 00)));

        // When
        var withinRange00 = schedule.GetCurrentRange(wednesday);
        var withinRange59 = schedule.GetCurrentRange(wednesday.AddMinutes(59));
        var outsideRange60 = schedule.GetCurrentRange(wednesday.AddMinutes(60));

        // Then
        Assert.NotNull(withinRange00);
        Assert.NotNull(withinRange59);
        Assert.Null(outsideRange60);
    }

    [Fact]
    public void CanGetRangePeriods_Empty()
    {
        // Given
        var empty = new Schedule() { Name = "", OrganizationId = "" };

        // When
        Assert.Empty(empty.GetRangePeriods(DateTime.Now));

        // Then
    }

    [Fact]
    public void CanGetRangePeriods()
    {
        // Given
        var wednesday = new DateTime(2025, 01, 15, 19, 00, 00, DateTimeKind.Unspecified);
        var schedule = new Schedule() { Name = "", OrganizationId = "" };
        schedule
            .Weekdays[DayOfWeek.Wednesday]
            .Ranges.Add(new(new TimeOnly(19, 00), new TimeOnly(20, 00)));

        // When
        var startsThisWednesday = schedule
            .GetRangePeriods(startNotBefore: wednesday)
            .Take(10)
            .ToList();
        var startsNextWednesday = schedule
            .GetRangePeriods(startNotBefore: wednesday.AddSeconds(1))
            .Take(10)
            .ToList();

        // Then
        Assert.Equal(wednesday, startsThisWednesday.First().Start);
        Assert.Equal(wednesday.AddDays(9 * 7), startsThisWednesday.Last().Start);

        var nextWednesday = wednesday.AddDays(7);
        Assert.Equal(nextWednesday, startsNextWednesday.First().Start);
        Assert.Equal(nextWednesday.AddDays(9 * 7), startsNextWednesday.Last().Start);
    }
}
