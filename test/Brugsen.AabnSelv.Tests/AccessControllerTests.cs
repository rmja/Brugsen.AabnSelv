using Akiles.ApiClient;
using Brugsen.AabnSelv.Controllers;
using Brugsen.AabnSelv.Gadgets;
using Brugsen.AabnSelv.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Brugsen.AabnSelv.Tests;

public class AccessControllerTests
{
    private readonly Mock<IAccessService> _accessServiceMock = new();
    private readonly NoopAlarmGadget _alarmGadget = new();
    private readonly NoopLightGadget _lightGadget = new();
    private readonly Mock<IFrontDoorGadget> _doorGadgetMock = new();
    private readonly Mock<IAkilesApiClient> _clientMock = new();
    private readonly Mock<IOpeningHoursService> _openingHoursMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly AccessController _controller;

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    public AccessControllerTests()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(_accessServiceMock.Object)
            .AddSingleton<IAlarmGadget>(_alarmGadget)
            .AddSingleton<ILightGadget>(_lightGadget)
            .AddSingleton(_doorGadgetMock.Object)
            .AddKeyedSingleton(ServiceKeys.ApiKeyClient, _clientMock.Object)
            .AddSingleton(_openingHoursMock.Object)
            .AddSingleton<TimeProvider>(_fakeTime)
            .BuildServiceProvider();

        _fakeTime.SetLocalTimeZone(DanishTimeProvider.EuropeCopenhagen);
        _controller = ActivatorUtilities.CreateInstance<AccessController>(services);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CanProcessCheckIn(bool openDoor)
    {
        // Given
        if (openDoor)
        {
            _doorGadgetMock
                .Setup(m => m.OpenOnceAsync(_clientMock.Object, CancellationToken.None))
                .Verifiable();
        }

        // When
        await _controller.StartAsync(TestCancellationToken);

        await _controller.ProcessCheckInAsync("check_in", "member1", openDoor);
        Assert.Equal(AlarmState.Disarmed, _alarmGadget.State);
        Assert.Equal(LightState.On, _lightGadget.State);

        await _controller.StopAsync(TestCancellationToken);

        // Then
        _doorGadgetMock.Verify();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task CanProcessCheckOut(bool openDoor, bool isCheckedIn)
    {
        // Given
        if (openDoor)
        {
            _doorGadgetMock
                .Setup(m => m.OpenOnceAsync(_clientMock.Object, CancellationToken.None))
                .Verifiable();
        }

        _accessServiceMock
            .Setup(m =>
                m.IsMemberCheckedInAsync(
                    _clientMock.Object,
                    "member1",
                    It.IsAny<DateTimeOffset>(),
                    "check_out",
                    CancellationToken.None
                )
            )
            .ReturnsAsync(isCheckedIn);

        var checkedOut = new DateTime(2025, 01, 23, 20, 00, 00);
        _fakeTime.SetLocalNow(checkedOut);

        // When
        await _controller.StartAsync(TestCancellationToken);
        await Task.Delay(100, TestCancellationToken);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(LightState.Off, _lightGadget.State);

        await _controller.ProcessCheckOutAsync("check_out", "member1", openDoor);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(LightState.Off, _lightGadget.State);

        _fakeTime.Advance(_controller.BlackoutDelay);
        await WaitForAsync(() => _lightGadget.State == LightState.Off);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(LightState.Off, _lightGadget.State);

        _fakeTime.AdvanceToLocal(checkedOut.Add(_controller.LockdownDelay));
        await WaitForAsync(() => _alarmGadget.State == AlarmState.Armed);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(LightState.Off, _lightGadget.State);

        await _controller.StopAsync(TestCancellationToken);

        // Then
        _doorGadgetMock.Verify();
    }

    [Fact]
    public async Task CannotProcessCheckOut_WhenCheckInIsInforced_NotCheckedIn()
    {
        // Given
        _accessServiceMock
            .Setup(m =>
                m.IsMemberCheckedInAsync(
                    _clientMock.Object,
                    "member1",
                    It.IsAny<DateTimeOffset>(),
                    "check_out",
                    TestCancellationToken
                )
            )
            .ReturnsAsync(false);

        var checkedOut = new DateTime(2025, 01, 23, 20, 00, 00);
        _fakeTime.SetLocalNow(checkedOut);

        // When
        await _controller.StartAsync(TestCancellationToken);
        await Task.Delay(100, TestContext.Current.CancellationToken);

        await _controller.ProcessCheckOutAsync("check_out", "member1", openDoor: true);

        await _controller.StopAsync(TestCancellationToken);

        // Then
    }

    [Fact]
    public async Task CheckInRightAfterCheckOut_DisarmsBlackoutAndLockdown()
    {
        // Given
        _accessServiceMock
            .Setup(m =>
                m.IsMemberCheckedInAsync(
                    _clientMock.Object,
                    "member1",
                    It.IsAny<DateTimeOffset>(),
                    "check_out",
                    TestCancellationToken
                )
            )
            .ReturnsAsync(false);

        var checkedOut = new DateTime(2025, 01, 23, 20, 00, 00);
        _fakeTime.SetLocalNow(checkedOut);

        // When
        await _controller.StartAsync(TestCancellationToken);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(LightState.Off, _lightGadget.State);

        await _controller.ProcessCheckOutAsync("check_out", "member1", openDoor: false);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(LightState.Off, _lightGadget.State);

        _fakeTime.Advance(_controller.BlackoutDelay / 2);
        await _controller.ProcessCheckInAsync("check_in", "member2", openDoor: false);
        Assert.Equal(AlarmState.Disarmed, _alarmGadget.State);
        Assert.Equal(LightState.On, _lightGadget.State);

        _fakeTime.Advance(_controller.BlackoutDelay); // Way past original configured blackout
        await Task.Delay(100, TestCancellationToken);
        Assert.Equal(AlarmState.Disarmed, _alarmGadget.State);
        Assert.Equal(LightState.On, _lightGadget.State);

        await _controller.StopAsync(TestCancellationToken);

        // Then
    }

    [Fact]
    public async Task CheckInWithoutCheckOut_BlackoutAndLockdownIsRunAfterTimeout()
    {
        // Given
        var checkedIn = new DateTime(2025, 02, 17, 13, 00, 00);
        _fakeTime.SetLocalNow(checkedIn);

        // When
        await _controller.StartAsync(TestCancellationToken);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(LightState.Off, _lightGadget.State);

        await _controller.ProcessCheckInAsync("check_in", "member1", openDoor: false);
        Assert.Equal(AlarmState.Disarmed, _alarmGadget.State);
        Assert.Equal(LightState.On, _lightGadget.State);

        _fakeTime.Advance(_controller.CheckoutTimeout);
        Assert.Equal(AlarmState.Disarmed, _alarmGadget.State);
        Assert.Equal(LightState.On, _lightGadget.State);

        _fakeTime.Advance(_controller.BlackoutDelay);
        await WaitForAsync(() => _lightGadget.State == LightState.Off);
        Assert.Equal(AlarmState.Disarmed, _alarmGadget.State);
        Assert.Equal(LightState.Off, _lightGadget.State);

        _fakeTime.Advance(_controller.LockdownDelay); // Way past configured lockdown
        await WaitForAsync(() => _alarmGadget.State == AlarmState.Armed);
        Assert.Equal(AlarmState.Armed, _alarmGadget.State);
        Assert.Equal(LightState.Off, _lightGadget.State);

        await _controller.StopAsync(TestCancellationToken);

        // Then
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        while (!condition())
        {
            await Task.Yield();
        }
    }
}
