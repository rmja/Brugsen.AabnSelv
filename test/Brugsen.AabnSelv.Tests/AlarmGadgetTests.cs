using Akiles.Api;
using Akiles.Api.Events;
using Brugsen.AabnSelv.Gadgets;
using Moq;

namespace Brugsen.AabnSelv.Tests;

public class AlarmGadgetTests
{
    private readonly Mock<IAkilesApiClient> clientMock = new();

    [Fact]
    public async Task CanGetLastArmed_WhenNotPreviouslyArmed()
    {
        // Given
        var alarm = new AlarmGadget("alarm");

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
            .ReturnsAsync(new PagedList<Event>());

        // When
        var lastArmed = await alarm.GetLastArmedAsync(clientMock.Object, CancellationToken.None);

        // Then
        Assert.Null(lastArmed);
    }

    [Fact]
    public async Task CanGetLastArmed()
    {
        // Given
        var alarm = new AlarmGadget("alarm");
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
        var lastArmed = await alarm.GetLastArmedAsync(clientMock.Object, CancellationToken.None);

        // Then
        Assert.Equal(armEventCreated, lastArmed);
    }
}
