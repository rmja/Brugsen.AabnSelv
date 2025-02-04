using Akiles.Api.Members;

namespace Akiles.Api.Tests;

public class SchedulesTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly IAkilesApiClient _client = fixture.Client;

    [Fact]
    public async Task CanListSchedules()
    {
        // Given

        // When
        var schedules = await _client.Schedules.ListSchedulesAsync().ToListAsync();

        // Then
        Assert.Equal(2, schedules.Count);
    }

    [Fact]
    public async Task CanGetSchedule()
    {
        // Given

        // When
        var schedule = await _client.Schedules.GetScheduleAsync("sch_41fm7t77cmm31j8jdd91");

        // Then
        Assert.Equal(7, schedule.Weekdays.Count);
    }
}
