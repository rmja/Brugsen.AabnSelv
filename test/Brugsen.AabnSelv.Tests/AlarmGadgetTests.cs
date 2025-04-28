using Akiles.ApiClient;
using Akiles.ApiClient.Events;
using Brugsen.AabnSelv.Gadgets;
using Moq;

namespace Brugsen.AabnSelv.Tests;

public class AlarmGadgetTests
{
    private readonly Mock<IAkilesApiClient> clientMock = new();
    private readonly IAlarmGadget _alarmGadget = new AlarmGadget("alarm");

    [Fact]
    public async Task CanGetLastArmed_WhenNotPreviouslyArmed()
    {
        // Given
        clientMock
            .Setup(m =>
                m.Events.ListEventsAsync(
                    null,
                    1,
                    "created_at:desc",
                    It.IsAny<ListEventsFilter>(),
                    EventsExpand.None,
                    CancellationToken.None
                )
            )
            .ReturnsAsync([]);

        // When
        var lastArmed = await _alarmGadget.GetLastArmedAsync(clientMock.Object);

        // Then
        Assert.Null(lastArmed);
    }

    [Fact]
    public async Task CanGetLastArmed()
    {
        // Given
        var armEventCreated = DateTime.UtcNow;
        var events = new PagedList<Event>()
        {
            new()
            {
                Subject = new() { },
                Verb = EventVerb.Use,
                Object = new()
                {
                    GadgetId = "alarm",
                    GadgetActionId = AlarmGadget.Actions.AlarmArm
                },
                CreatedAt = armEventCreated
            },
        };
        clientMock
            .Setup(m =>
                m.Events.ListEventsAsync(
                    null,
                    1,
                    "created_at:desc",
                    It.IsAny<ListEventsFilter>(),
                    EventsExpand.None,
                    CancellationToken.None
                )
            )
            .ReturnsAsync(events);

        // When
        var lastArmed = await _alarmGadget.GetLastArmedAsync(clientMock.Object);

        // Then
        Assert.Equal(armEventCreated, lastArmed);
    }
}
