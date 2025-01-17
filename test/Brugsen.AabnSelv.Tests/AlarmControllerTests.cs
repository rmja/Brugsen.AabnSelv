using Akiles.Api;
using Akiles.Api.Schedules;
using Brugsen.AabnSelv.Controllers;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Brugsen.AabnSelv.Tests;

public class AlarmControllerTests
{
    private readonly Mock<IAkilesApiClient> _clientMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly AlarmController _controller;
    private readonly IAlarmGadget _alarmGadget;

    public AlarmControllerTests()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IAlarmGadget, NoopAlarmGadget>()
            .AddKeyedSingleton(ServiceKeys.ApiKeyClient, _clientMock.Object)
            .AddSingleton<TimeProvider>(_fakeTime)
            .BuildServiceProvider();

        _fakeTime.SetLocalTimeZone(DanishTimeProvider.EuropeCopenhagen);
        _controller = ActivatorUtilities.CreateInstance<AlarmController>(services);
        _alarmGadget = services.GetRequiredService<IAlarmGadget>();
    }

    [Fact]
    public async Task TickDisarmsAndArmsAlarm()
    {
        // Given
        var schedule = new Schedule { OrganizationId = "", Name = "Alarm Frakoblet" };
        foreach (var weekday in Enum.GetValues<DayOfWeek>())
        {
            schedule.Weekdays[weekday].Ranges.Add(new(new TimeOnly(04, 50), new TimeOnly(23, 10)));
        }

        Assert.Equal(AlarmGadgetArmState.Unknown, _alarmGadget.ArmState);

        // When
        _fakeTime.SetLocalNow(new DateTime(2025, 01, 16, 04, 00, 00)); // Early morning
        var nextTick = await _controller.TickAsync(schedule, CancellationToken.None);

        // Then
        Assert.Equal(new DateTime(2025, 01, 16, 04, 50, 00), nextTick);
        Assert.Equal(AlarmGadgetArmState.Armed, _alarmGadget.ArmState);

        // When
        _fakeTime.SetLocalNow(nextTick.Value); // Early morning
        nextTick = await _controller.TickAsync(schedule, CancellationToken.None);

        // Then
        Assert.Equal(new DateTime(2025, 01, 16, 23, 10, 00), nextTick);
        Assert.Equal(AlarmGadgetArmState.Disarmed, _alarmGadget.ArmState);

        // When
        _fakeTime.SetLocalNow(nextTick.Value); // Late night
        nextTick = await _controller.TickAsync(schedule, CancellationToken.None);

        // Then
        Assert.Equal(new DateTime(2025, 01, 17, 04, 50, 00), nextTick);
        Assert.Equal(AlarmGadgetArmState.Armed, _alarmGadget.ArmState);

        // When
        _fakeTime.SetLocalNow(nextTick.Value); // Early morning
        nextTick = await _controller.TickAsync(schedule, CancellationToken.None);
        Assert.Equal(AlarmGadgetArmState.Disarmed, _alarmGadget.ArmState);
    }
}
