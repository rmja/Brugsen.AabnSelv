using Akiles.Api;
using Akiles.Api.Schedules;
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
    private Schedule _extendedSchedule;

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

        _extendedSchedule = TestSchedules.GetExtendedOpeningHoursSchedule();
        Assert.Equal(LightState.Unknown, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
    }

    [Fact]
    public async Task TickPerformsFinalShutdown_StartedWayOutsideExtendedOpeningHours()
    {
        // Given
        var startupTime = new DateTime(2025, 01, 16, 23, 30, 00); // Way outside extended opening hours

        // When
        _fakeTime.SetLocalNow(startupTime);
        var nextTick = await _controller.TickAsync(_extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Locked, _lockGadget.State);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 17, 23, 10, 00), nextTick);
    }

    [Fact]
    public async Task TickSchedulesFinalShutdown_StartedJustAfterExtendedOpeningHours_BeforeLightOff()
    {
        // Given
        var startupTime = new DateTime(2025, 01, 16, 23, 08, 00); // Just after extended opening hours, but before light is off

        // When
        _fakeTime.SetLocalNow(startupTime);
        var nextTick = await _controller.TickAsync(_extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Unknown, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 16, 23, 10, 00), nextTick);

        // When
        _fakeTime.SetLocalNow(nextTick.Value);
        nextTick = await _controller.TickAsync(_extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 16, 23, 15, 00), nextTick);
    }

    [Fact]
    public async Task TickSchedulesAndPerformsFinalShutdown_StartedJustAfterExtendedOpeningHours_BeforeLightOff()
    {
        // Given
        var startupTime = new DateTime(2025, 01, 16, 23, 12, 00); // Just after extended opening hours, but after light is off

        // When
        _fakeTime.SetLocalNow(startupTime);
        var nextTick = await _controller.TickAsync(_extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 16, 23, 15, 00), nextTick);

        // When
        _fakeTime.SetLocalNow(nextTick.Value);
        nextTick = await _controller.TickAsync(_extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Locked, _lockGadget.State);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 17, 23, 10, 00), nextTick);
    }

    [Fact]
    public async Task TickSchedulesAndPerformsFinalShutdown_StartedWithinExtendedOpeningHours()
    {
        // Given
        var startupTime = new DateTime(2025, 01, 16, 06, 00, 00); // Morning

        // When
        _fakeTime.SetLocalNow(startupTime);
        var nextTick = await _controller.TickAsync(_extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Unknown, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 16, 23, 10, 00), nextTick);

        // When
        _fakeTime.SetLocalNow(nextTick.Value); // Time to turn off the light
        nextTick = await _controller.TickAsync(_extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 16, 23, 15, 00), nextTick);

        // When
        _fakeTime.SetLocalNow(nextTick.Value); // Time to arm the alarm
        nextTick = await _controller.TickAsync(_extendedSchedule, CancellationToken.None);
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
        nextTick = await _controller.TickAsync(_extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 17, 23, 15, 00), nextTick);

        // When
        _fakeTime.SetLocalNow(nextTick.Value); // Time to arm the alarm
        nextTick = await _controller.TickAsync(_extendedSchedule, CancellationToken.None);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Locked, _lockGadget.State);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(new DateTime(2025, 01, 18, 23, 10, 00), nextTick);
    }
}
