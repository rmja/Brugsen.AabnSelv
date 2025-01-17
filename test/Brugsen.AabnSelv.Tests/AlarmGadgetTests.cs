using Akiles.Api;
using Akiles.Api.Gadgets;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Brugsen.AabnSelv.Tests;

public class AlarmGadgetTests
{
    private readonly Mock<IAkilesApiClient> clientMock = new();
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public async Task CanGetLastArmed_WhenNotPreviouslyArmed()
    {
        // Given
        var alarm = new AlarmGadget("alarm", _fakeTime);

        clientMock
            .Setup(m => m.Gadgets.GetGadgetAsync("alarm", CancellationToken.None))
            .ReturnsAsync(
                new Gadget
                {
                    OrganizationId = "",
                    SiteId = "",
                    DeviceId = "",
                    Name = "",
                    Metadata = { }
                }
            );

        // When
        var lastArmed = await alarm.GetLastArmedAsync(clientMock.Object, CancellationToken.None);

        // Then
        Assert.Null(lastArmed);
    }

    [Fact]
    public async Task CanGetLastArmed_AfterArm()
    {
        // Given
        var alarm = new AlarmGadget("alarm", _fakeTime);
        var gadget = new Gadget
        {
            OrganizationId = "",
            SiteId = "",
            DeviceId = "",
            Name = "",
            Metadata = { }
        };

        clientMock
            .Setup(m =>
                m.Gadgets.EditGadgetAsync("alarm", It.IsAny<GadgetPatch>(), CancellationToken.None)
            )
            .Callback(
                (string gadgetId, GadgetPatch patch, CancellationToken cancellationToken) =>
                {
                    foreach (var (key, value) in patch.Metadata ?? [])
                    {
                        gadget.Metadata[key] = value;
                    }
                }
            )
            .ReturnsAsync(gadget);

        clientMock
            .Setup(m => m.Gadgets.GetGadgetAsync("alarm", CancellationToken.None))
            .ReturnsAsync(gadget);

        // When
        await alarm.ArmAsync(clientMock.Object, CancellationToken.None);
        var lastArmed = await alarm.GetLastArmedAsync(clientMock.Object, CancellationToken.None);

        // Then
        Assert.Equal(_fakeTime.GetUtcNow().UtcDateTime, lastArmed);
    }
}
