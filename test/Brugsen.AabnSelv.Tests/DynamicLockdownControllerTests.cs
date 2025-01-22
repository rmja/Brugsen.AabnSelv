using Akiles.Api;
using Akiles.Api.Events;
using Brugsen.AabnSelv.Controllers;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Brugsen.AabnSelv.Tests;

public class DynamicLockdownControllerTests
{
    private readonly Mock<IAccessGadget> _accessGadgetMock = new();
    private readonly Mock<ILightGadget> _lightGadgetMock = new();
    private readonly Mock<IFrontDoorLockGadget> _lockGadgetMock = new();
    private readonly Mock<IAlarmGadget> _alarmGadgetMock = new();
    private readonly Mock<IAkilesApiClient> _clientMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly DynamicLockdownController _controller;

    public DynamicLockdownControllerTests()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(_accessGadgetMock.Object)
            .AddSingleton(_lightGadgetMock.Object)
            .AddSingleton(_lockGadgetMock.Object)
            .AddSingleton(_alarmGadgetMock.Object)
            .AddKeyedSingleton(ServiceKeys.ApiKeyClient, _clientMock.Object)
            .AddSingleton<TimeProvider>(_fakeTime)
            .BuildServiceProvider();

        _fakeTime.SetLocalTimeZone(DanishTimeProvider.EuropeCopenhagen);
        _controller = ActivatorUtilities.CreateInstance<DynamicLockdownController>(services);
    }

    [Fact]
    public async Task TickPerformsShutdownWhenWhenTheNumberOfEntriesEqualsTheNumberOfExits()
    {
        // Given
        _fakeTime.AutoAdvanceAmount = TimeSpan.FromMinutes(1);
        _fakeTime.SetLocalNow(new DateTime(2025, 01, 17, 22, 00, 00)); // Late night
        var regularSchedule = TestSchedules.GetRegularOpeningHoursSchedule();
        var extendedSchedule = TestSchedules.GetExtendedOpeningHoursSchedule();
        var activity = new List<AccessActivity>
        {
            new()
            {
                MemberId = "member1",
                CheckInEvent = new()
                {
                    Subject = new() { },
                    Verb = EventVerb.Use,
                    Object = new()
                    {
                        MemberId = "member1",
                        GadgetId = "access",
                        GadgetActionId = AccessGadget.Actions.CheckIn
                    },
                    CreatedAt = _fakeTime.GetUtcNow().UtcDateTime
                },
                CheckOutEvent = new()
                {
                    Subject = new() { },
                    Verb = EventVerb.Use,
                    Object = new()
                    {
                        MemberId = "member1",
                        GadgetId = "access",
                        GadgetActionId = AccessGadget.Actions.CheckOut
                    },
                    CreatedAt = _fakeTime.GetUtcNow().UtcDateTime
                }
            }
        };
        _accessGadgetMock
            .Setup(m =>
                m.GetActivityAsync(
                    _clientMock.Object,
                    null,
                    It.IsAny<DateTimeOffset>(),
                    EventsExpand.None,
                    CancellationToken.None
                )
            )
            .ReturnsAsync(activity);
        _lightGadgetMock
            .Setup(m => m.TurnOffAsync(_clientMock.Object, CancellationToken.None))
            .Verifiable();
        _lockGadgetMock
            .Setup(m => m.LockAsync(_clientMock.Object, CancellationToken.None))
            .Verifiable();
        _alarmGadgetMock
            .Setup(m => m.ArmAsync(_clientMock.Object, CancellationToken.None))
            .Verifiable();

        // When
        await _controller.TickAsync(regularSchedule, extendedSchedule, CancellationToken.None);

        // Then
        _clientMock.VerifyAll();
    }
}
