using Akiles.ApiClient;
using Akiles.ApiClient.Events;
using Brugsen.AabnSelv.Gadgets;
using Moq;

namespace Brugsen.AabnSelv.Tests;

public class AlarmGadgetTests
{
    private readonly Mock<IAkilesApiClient> clientMock = new();
    private readonly IAlarmGadget _alarmGadget = new AlarmGadget("alarm");

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CanGetLastArmed_WhenNotPreviouslyArmed()
    {
        // Given
        clientMock
            .Setup(m =>
                m.Events.ListEventsAsync(
                    "created_at:desc",
                    It.IsAny<ListEventsFilter>(),
                    EventsExpand.None,
                    null,
                    1,
                    TestCancellationToken
                )
            )
            .ReturnsAsync([]);

        // When
        var lastArmed = await _alarmGadget.GetLastArmedAsync(
            clientMock.Object,
            TestCancellationToken
        );

        // Then
        Assert.Null(lastArmed);
    }

    [Fact]
    public async Task CanGetLastArmed()
    {
        // Given
        var armEventCreated = DateTime.UtcNow;
        clientMock
            .Setup(m =>
                m.Events.ListEventsAsync(
                    "created_at:desc",
                    It.IsAny<ListEventsFilter>(),
                    EventsExpand.None,
                    null,
                    1,
                    TestCancellationToken
                )
            )
            .ReturnsAsync(
                [
                    new()
                    {
                        Subject = new() { },
                        Verb = EventVerb.Use,
                        Object = new()
                        {
                            GadgetId = "alarm",
                            GadgetActionId = AlarmGadget.Actions.AlarmArm,
                        },
                        CreatedAt = armEventCreated,
                    },
                ]
            );

        // When
        var lastArmed = await _alarmGadget.GetLastArmedAsync(
            clientMock.Object,
            TestCancellationToken
        );

        // Then
        Assert.Equal(armEventCreated, lastArmed);
    }
}
