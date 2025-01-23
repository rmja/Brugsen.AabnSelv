using Akiles.Api;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Brugsen.AabnSelv.Tests;

public class LockdownControllerTests
{
    private readonly NoopLightGadget _lightGadget = new();
    private readonly NoopFrontDoorLockGadget _lockGadget = new();
    private readonly NoopAlarmGadget _alarmGadget = new();
    private readonly Mock<IAkilesApiClient> _clientMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly LockdownController _controller;

    public LockdownControllerTests()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<ILightGadget>(_lightGadget)
            .AddSingleton<IFrontDoorLockGadget>(_lockGadget)
            .AddSingleton<IAlarmGadget>(_alarmGadget)
            .AddKeyedSingleton(ServiceKeys.ApiKeyClient, _clientMock.Object)
            .AddSingleton<TimeProvider>(_fakeTime)
            .Configure<BrugsenAabnSelvOptions>(options =>
                options.ExtendedOpeningHoursScheduleId = "extended_opening_hours"
            )
            .BuildServiceProvider();

        _fakeTime.SetLocalTimeZone(DanishTimeProvider.EuropeCopenhagen);
        _controller = ActivatorUtilities.CreateInstance<LockdownController>(services);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);

        _clientMock
            .Setup(m =>
                m.Schedules.GetScheduleAsync(
                    "extended_opening_hours",
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(() => TestSchedules.GetExtendedOpeningHoursSchedule());
    }

    [Fact]
    public async Task CanLockdown_StartedWayOutsideExtendedOpeningHours()
    {
        // Given
        var startupTime = new DateTime(2025, 01, 16, 23, 30, 00); // Way outside extended opening hours
        var end = new DateTime(2025, 01, 17, 23, 00, 00);

        _fakeTime.SetLocalNow(startupTime);

        // When
        await _controller.StartAsync(CancellationToken.None);

        await ArmedAsync();
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Locked, _lockGadget.State);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(end.Add(_controller.BlackoutDelay), _controller.BlackoutAt);
        Assert.Equal(end.Add(_controller.LockdownDelay), _controller.LockdownAt);

        await _controller.StopAsync(CancellationToken.None);

        // Then
    }

    [Fact]
    public async Task CanLockdown_StartedJustAfterExtendedOpeningHours_BeforeBlackout()
    {
        // Given
        var startupTime = new DateTime(2025, 01, 16, 23, 03, 00); // Just after extended opening hours, but before light is off
        var end = new DateTime(2025, 01, 16, 23, 00, 00, 00);

        _fakeTime.SetLocalNow(startupTime);

        // When
        await _controller.StartAsync(CancellationToken.None);

        await ArmedAsync();
        Assert.Equal(LightState.Unknown, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(end.Add(_controller.BlackoutDelay), _controller.BlackoutAt);
        Assert.Equal(end.Add(_controller.LockdownDelay), _controller.LockdownAt);

        _fakeTime.AdvanceToLocal(end.Add(_controller.BlackoutDelay));
        await ArmedAsync();
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(end.Add(_controller.BlackoutDelay).AddDays(1), _controller.BlackoutAt);
        Assert.Equal(end.Add(_controller.LockdownDelay), _controller.LockdownAt);

        _fakeTime.AdvanceToLocal(end.Add(_controller.LockdownDelay));
        await ArmedAsync();
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Locked, _lockGadget.State);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(end.Add(_controller.BlackoutDelay).AddDays(1), _controller.BlackoutAt);
        Assert.Equal(end.Add(_controller.LockdownDelay).AddDays(1), _controller.LockdownAt);

        await _controller.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CanLockdown_StartedDuringExtendedOpeningHours()
    {
        // Given
        var startupTime = new DateTime(2025, 01, 16, 06, 00, 00); // Morning
        var end = new DateTime(2025, 01, 16, 23, 00, 00, 00);

        _fakeTime.SetLocalNow(startupTime);

        // When
        await _controller.StartAsync(CancellationToken.None);

        await ArmedAsync();
        Assert.Equal(LightState.Unknown, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(end.Add(_controller.BlackoutDelay), _controller.BlackoutAt);
        Assert.Equal(end.Add(_controller.LockdownDelay), _controller.LockdownAt);

        _fakeTime.AdvanceToLocal(end.Add(_controller.BlackoutDelay));
        await ArmedAsync();
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(end.Add(_controller.BlackoutDelay).AddDays(1), _controller.BlackoutAt);
        Assert.Equal(end.Add(_controller.LockdownDelay), _controller.LockdownAt);

        _fakeTime.AdvanceToLocal(end.Add(_controller.LockdownDelay));
        await ArmedAsync();
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Locked, _lockGadget.State);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(end.Add(_controller.BlackoutDelay).AddDays(1), _controller.BlackoutAt);
        Assert.Equal(end.Add(_controller.LockdownDelay).AddDays(1), _controller.LockdownAt);

        // Next day
        end = end.AddDays(1);
        _lightGadget.State = LightState.Unknown;
        _lockGadget.State = LockState.Unknown;
        _alarmGadget.State = AlarmState.Unknown;

        _fakeTime.AdvanceToLocal(end.Add(_controller.BlackoutDelay));
        await ArmedAsync();
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(end.Add(_controller.BlackoutDelay).AddDays(1), _controller.BlackoutAt);
        Assert.Equal(end.Add(_controller.LockdownDelay), _controller.LockdownAt);

        _fakeTime.AdvanceToLocal(end.Add(_controller.LockdownDelay));
        await ArmedAsync();
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Locked, _lockGadget.State);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(end.Add(_controller.BlackoutDelay).AddDays(1), _controller.BlackoutAt);
        Assert.Equal(end.Add(_controller.LockdownDelay).AddDays(1), _controller.LockdownAt);

        await _controller.StopAsync(CancellationToken.None);
    }

    async Task ArmedAsync()
    {
        while (
            _controller.ExecuteTask?.IsCompleted != true
            && (!_controller.LockdownAt.HasValue || !_controller.BlackoutAt.HasValue)
        )
        {
            await Task.Yield();
        }
    }
}
