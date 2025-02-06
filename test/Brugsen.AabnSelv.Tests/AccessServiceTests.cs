using Akiles.Api;
using Akiles.Api.Events;
using Brugsen.AabnSelv.Gadgets;
using Brugsen.AabnSelv.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Brugsen.AabnSelv.Tests;

public class AccessServiceTests
{
    private readonly Mock<IAlarmGadget> _alarmGadgetMock = new();
    private readonly Mock<ILightGadget> _lightGadgetMock = new();
    private readonly Mock<IFrontDoorLockGadget> _lockGadgetMock = new();
    private readonly Mock<IFrontDoorGadget> _doorGadgetMock = new();
    private readonly Mock<IAkilesApiClient> _clientMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly IAccessService _accessService;

    public AccessServiceTests()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(_alarmGadgetMock.Object)
            .AddSingleton(_lightGadgetMock.Object)
            .AddSingleton(_lockGadgetMock.Object)
            .AddSingleton(_doorGadgetMock.Object)
            .AddSingleton<TimeProvider>(_fakeTime)
            .BuildServiceProvider();

        _fakeTime.SetLocalTimeZone(DanishTimeProvider.EuropeCopenhagen);
        _fakeTime.AutoAdvanceAmount = TimeSpan.FromMinutes(1);
        _accessService = ActivatorUtilities.CreateInstance<AccessService>(services);
    }

    [Fact]
    public async Task CanGetIsMemberCheckedIn_NotThereYet()
    {
        // Given
        var notBefore = _fakeTime.GetUtcNow().AddHours(-1);
        var events = new List<Event>();

        _clientMock
            .Setup(m =>
                m.Events.ListEventsAsync(
                    "created_at:desc",
                    It.IsAny<ListEventsFilter>(),
                    EventsExpand.None
                )
            )
            .Returns(events.AsEnumerable().Reverse().AsAsyncEnumerable());

        // When
        var isCheckedId = await _accessService.IsMemberCheckedInAsync(
            _clientMock.Object,
            "member",
            notBefore
        );

        // Then
        Assert.False(isCheckedId);
    }

    [Fact]
    public async Task CanGetIsMemberCheckedIn_IsCheckedIn()
    {
        // Given
        var notBefore = _fakeTime.GetUtcNow().AddHours(-1);
        var events = new List<Event>()
        {
            new()
            {
                Id = "check_in",
                Subject = new() { MemberId = "member", },
                Verb = EventVerb.Use,
                Object = new()
                {
                    GadgetId = "access",
                    GadgetActionId = AccessGadget.Actions.CheckIn
                },
                CreatedAt = _fakeTime.GetUtcNow().UtcDateTime
            },
        };

        _clientMock
            .Setup(m =>
                m.Events.ListEventsAsync(
                    "created_at:desc",
                    It.IsAny<ListEventsFilter>(),
                    EventsExpand.None
                )
            )
            .Returns(events.AsEnumerable().Reverse().AsAsyncEnumerable());

        // When
        var isCheckedId = await _accessService.IsMemberCheckedInAsync(
            _clientMock.Object,
            "member",
            notBefore
        );

        // Then
        Assert.True(isCheckedId);
    }

    [Fact]
    public async Task CanGetIsMemberCheckedIn_IsCheckedInAndJustCheckedOut()
    {
        // Given
        var notBefore = _fakeTime.GetUtcNow().AddHours(-1);
        var events = new List<Event>()
        {
            new()
            {
                Id = "check_in",
                Subject = new() { MemberId = "member", },
                Verb = EventVerb.Use,
                Object = new()
                {
                    GadgetId = "access",
                    GadgetActionId = AccessGadget.Actions.CheckIn
                },
                CreatedAt = _fakeTime.GetUtcNow().UtcDateTime
            },
            new()
            {
                Id = "check_out",
                Subject = new() { MemberId = "member", },
                Verb = EventVerb.Use,
                Object = new()
                {
                    GadgetId = "access",
                    GadgetActionId = AccessGadget.Actions.CheckOut
                },
                CreatedAt = _fakeTime.GetUtcNow().UtcDateTime
            },
        };

        _clientMock
            .Setup(m =>
                m.Events.ListEventsAsync(
                    "created_at:desc",
                    It.IsAny<ListEventsFilter>(),
                    EventsExpand.None
                )
            )
            .Returns(events.AsEnumerable().Reverse().AsAsyncEnumerable());

        // When
        var isCheckedId = await _accessService.IsMemberCheckedInAsync(
            _clientMock.Object,
            "member",
            notBefore,
            ignoreEventId: "check_out"
        );

        // Then
        Assert.True(isCheckedId);
    }

    [Fact]
    public async Task CanGetIsMemberCheckedIn_IsCheckedInAndOut()
    {
        // Given
        var notBefore = _fakeTime.GetUtcNow().AddHours(-1);
        var events = new List<Event>()
        {
            new()
            {
                Id = "check_in",
                Subject = new() { MemberId = "member", },
                Verb = EventVerb.Use,
                Object = new()
                {
                    GadgetId = "access",
                    GadgetActionId = AccessGadget.Actions.CheckIn
                },
                CreatedAt = _fakeTime.GetUtcNow().UtcDateTime
            },
            new()
            {
                Id = "check_out",
                Subject = new() { MemberId = "member", },
                Verb = EventVerb.Use,
                Object = new()
                {
                    GadgetId = "access",
                    GadgetActionId = AccessGadget.Actions.CheckOut
                },
                CreatedAt = _fakeTime.GetUtcNow().UtcDateTime
            },
        };

        _clientMock
            .Setup(m =>
                m.Events.ListEventsAsync(
                    "created_at:desc",
                    It.IsAny<ListEventsFilter>(),
                    EventsExpand.None
                )
            )
            .Returns(events.AsEnumerable().Reverse().AsAsyncEnumerable());

        // When
        var isCheckedId = await _accessService.IsMemberCheckedInAsync(
            _clientMock.Object,
            "member",
            notBefore
        );

        // Then
        Assert.False(isCheckedId);
    }

    [Fact]
    public async Task CanGetIsAnyCheckedIn()
    {
        // Given
        var notBefore = _fakeTime.GetUtcNow().AddHours(-1);
        var events = new List<Event>
        {
            new()
            {
                Id = "check_in",
                Subject = new() { MemberId = "member1", },
                Verb = EventVerb.Use,
                Object = new()
                {
                    GadgetId = "access",
                    GadgetActionId = AccessGadget.Actions.CheckIn
                },
                CreatedAt = _fakeTime.GetUtcNow().UtcDateTime
            },
            new()
            {
                Id = "check_in_again",
                Subject = new() { MemberId = "member1", },
                Verb = EventVerb.Use,
                Object = new()
                {
                    GadgetId = "access",
                    GadgetActionId = AccessGadget.Actions.CheckIn
                },
                CreatedAt = _fakeTime.GetUtcNow().UtcDateTime
            },
            new()
            {
                Id = "check_out",
                Subject = new() { MemberId = "member1", },
                Verb = EventVerb.Use,
                Object = new()
                {
                    GadgetId = "access",
                    GadgetActionId = AccessGadget.Actions.CheckOut
                },
                CreatedAt = _fakeTime.GetUtcNow().UtcDateTime
            }
        };
        _clientMock
            .Setup(m =>
                m.Events.ListEventsAsync(
                    "created_at:desc",
                    It.IsAny<ListEventsFilter>(),
                    EventsExpand.None
                )
            )
            .Returns(events.AsEnumerable().Reverse().AsAsyncEnumerable());

        // When
        var anyCheckedIn = await _accessService.IsAnyCheckedInAsync(_clientMock.Object, notBefore);

        // Then
        Assert.False(anyCheckedIn);
    }

    [Fact]
    public async Task CanGetActivity()
    {
        // Given
        var notBefore = _fakeTime.GetUtcNow().AddHours(-1);
        var events = new List<Event>
        {
            new()
            {
                Id = "check_in",
                Subject = new() { MemberId = "member1", },
                Verb = EventVerb.Use,
                Object = new()
                {
                    GadgetId = "access",
                    GadgetActionId = AccessGadget.Actions.CheckIn
                },
                CreatedAt = _fakeTime.GetUtcNow().UtcDateTime
            },
            new()
            {
                Id = "check_out",
                Subject = new() { MemberId = "member1", },
                Verb = EventVerb.Use,
                Object = new()
                {
                    GadgetId = "access",
                    GadgetActionId = AccessGadget.Actions.CheckOut
                },
                CreatedAt = _fakeTime.GetUtcNow().UtcDateTime
            }
        };
        _clientMock
            .Setup(m =>
                m.Events.ListEventsAsync(
                    "created_at:desc",
                    It.IsAny<ListEventsFilter>(),
                    EventsExpand.None
                )
            )
            .Returns(events.AsEnumerable().Reverse().AsAsyncEnumerable());

        // When
        var activities = await _accessService.GetActivityAsync(
            _clientMock.Object,
            memberId: null,
            notBefore
        );

        // Then
        var activity = Assert.Single(activities);
        Assert.Equal(events[0], activity.CheckInEvent);
        Assert.Equal(events[1], activity.CheckOutEvent);
    }
}
