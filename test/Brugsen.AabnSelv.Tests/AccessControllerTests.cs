using Akiles.Api;
using Brugsen.AabnSelv.Controllers;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Brugsen.AabnSelv.Tests;

public class AccessControllerTests
{
    private readonly Mock<IAccessGadget> _accessGadgetMock = new();
    private readonly NoopAlarmGadget _alarmGadget = new();
    private readonly NoopLightGadget _lightGadget = new();
    private readonly NoopFrontDoorLockGadget _lockGadget = new();
    private readonly Mock<IFrontDoorGadget> _doorGadgetMock = new();
    private readonly Mock<IAkilesApiClient> _clientMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly AccessController _controller;

    public AccessControllerTests()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(_accessGadgetMock.Object)
            .AddSingleton<IAlarmGadget>(_alarmGadget)
            .AddSingleton<ILightGadget>(_lightGadget)
            .AddSingleton<IFrontDoorLockGadget>(_lockGadget)
            .AddSingleton(_doorGadgetMock.Object)
            .AddKeyedSingleton(ServiceKeys.ApiKeyClient, _clientMock.Object)
            .AddSingleton<TimeProvider>(_fakeTime)
            .BuildServiceProvider();

        _fakeTime.SetLocalTimeZone(DanishTimeProvider.EuropeCopenhagen);
        _controller = ActivatorUtilities.CreateInstance<AccessController>(services);
    }

    [Fact]
    public async Task CanProcessCheckIn()
    {
        // Given
        _doorGadgetMock
            .Setup(m => m.OpenOnceAsync(_clientMock.Object, CancellationToken.None))
            .Verifiable();

        // When
        await _controller.StartAsync(CancellationToken.None);

        await _controller.ProcessCheckInAsync("check_in", "member1");
        Assert.Equal(AlarmState.Disarmed, _alarmGadget.State);
        Assert.Equal(LightState.On, _lightGadget.State);
        Assert.Equal(LockState.Unlocked, _lockGadget.State);

        await _controller.StopAsync(CancellationToken.None);

        // Then
        _doorGadgetMock.Verify();
    }

    [Fact]
    public async Task CanProcessCheckOut()
    {
        // Given
        _doorGadgetMock
            .Setup(m => m.OpenOnceAsync(_clientMock.Object, CancellationToken.None))
            .Verifiable();

        _accessGadgetMock
            .Setup(m =>
                m.IsMemberCheckedInAsync(
                    _clientMock.Object,
                    "member1",
                    It.IsAny<DateTimeOffset>(),
                    "check_out",
                    CancellationToken.None
                )
            )
            .ReturnsAsync(true);

        var checkedOut = new DateTime(2025, 01, 23, 20, 00, 00);
        _fakeTime.SetLocalNow(checkedOut);

        // When
        await _controller.StartAsync(CancellationToken.None);

        await _controller.ProcessCheckOutAsync("check_out", "member1");
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(LightState.Unknown, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);

        _fakeTime.Advance(_controller.BlackoutDelay);
        await WaitForAsync(() => _lightGadget.State == LightState.Off);
        Assert.Equal(AlarmState.Unknown, _alarmGadget.State);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);

        _fakeTime.AdvanceToLocal(checkedOut.Add(_controller.LockdownDelay));
        await WaitForAsync(() => _alarmGadget.State == AlarmState.Armed);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(LightState.Off, _lightGadget.State);
        Assert.Equal(LockState.Unknown, _lockGadget.State);

        await _controller.StopAsync(CancellationToken.None);

        // Then
        _doorGadgetMock.Verify();
    }

    private async Task WaitForAsync(Func<bool> condition)
    {
        while (!condition())
        {
            await Task.Yield();
        }
    }
}
