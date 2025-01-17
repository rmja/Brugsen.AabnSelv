using Akiles.Api;
using Brugsen.AabnSelv.Controllers;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Brugsen.AabnSelv.Tests;

public class FinalLockdownControllerTests
{
    private readonly NoopLightGadget _lightGadget = new();
    private readonly NoopFrontDoorLockGadget _lockGadget = new();
    private readonly NoopAlarmGadget _alarmGadget = new();
    private readonly Mock<IAkilesApiClient> _clientMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly FinalLockdownController _controller;

    public FinalLockdownControllerTests()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<ILightGadget>(_lightGadget)
            .AddSingleton<IFrontDoorLockGadget>(_lockGadget)
            .AddSingleton<IAlarmGadget>(_alarmGadget)
            .AddKeyedSingleton(ServiceKeys.ApiKeyClient, _clientMock.Object)
            .AddSingleton<TimeProvider>(_fakeTime)
            .BuildServiceProvider();

        _fakeTime.SetLocalTimeZone(DanishTimeProvider.EuropeCopenhagen);
        _controller = ActivatorUtilities.CreateInstance<FinalLockdownController>(services);
    }

    [Fact]
    public async Task TickSchedulesAndPerformsFinalShutdown()
    {
        // Given
        var extendedSchedule = TestSchedules.GetExtendedOpeningHoursSchedule();
        Assert.Equal(LightState.Unknown, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);

        // Morning

        // When
        _fakeTime.SetLocalNow(new DateTime(2025, 01, 16, 06, 00, 00));
        var nextTick = await _controller.TickAsync(extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Unknown, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 16, 23, 10, 00), nextTick);

        // When
        _fakeTime.SetLocalNow(nextTick.Value); // Time to turn off the light
        nextTick = await _controller.TickAsync(extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 16, 23, 15, 00), nextTick);

        // When
        _fakeTime.SetLocalNow(nextTick.Value); // Time to arm the alarm
        nextTick = await _controller.TickAsync(extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Locked, _lockGadget.State);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 17, 23, 10, 00), nextTick);

        // Next day
        _lightGadget.State = LightState.Unknown;
        _lockGadget.State = LockState.Unknown;
        _alarmGadget.State = AlarmState.Unknown;

        // When
        _fakeTime.SetLocalNow(nextTick.Value); // Time to turn off the light
        nextTick = await _controller.TickAsync(extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 17, 23, 15, 00), nextTick);

        // When
        _fakeTime.SetLocalNow(nextTick.Value); // Time to arm the alarm
        nextTick = await _controller.TickAsync(extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Locked, _lockGadget.State);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 18, 23, 10, 00), nextTick);
    }
}
