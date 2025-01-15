using Akiles.Api;
using Akiles.Api.Events;
using Brugsen.AabnSelv.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Brugsen.AabnSelv.Tests;

public class LightControllerTests
{
    private readonly Mock<IAkilesApiClient> _clientMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly LightController _controller;

    public LightControllerTests()
    {
        var services = new ServiceCollection()
            .Configure<BrugsenAabnSelvOptions>(options =>
            {
                options.FrontDoorGadgetId = "front_door";
                options.LightGadgetId = "light";
            })
            .AddLogging()
            .AddKeyedSingleton(ServiceKeys.ApiKeyClient, _clientMock.Object)
            .AddSingleton<TimeProvider>(_fakeTime)
            .BuildServiceProvider();

        _controller = ActivatorUtilities.CreateInstance<LightController>(services);
    }

    [Fact]
    public async Task TickTurnsLightOffWhenTheNumberOfEntriesEqualsTheNumberOfExits()
    {
        // Given
        var events = new List<Event>
        {
            new()
            {
                Subject = new() { MemberId = "member1" },
                Verb = EventVerb.Use,
                Object = new()
                {
                    GadgetId = "front_door",
                    GadgetActionId = GadgetActions.OpenEntry
                },
                CreatedAt = _fakeTime.GetUtcNow().AddMinutes(-10).UtcDateTime
            },
            new()
            {
                Subject = new() { MemberId = "member1" },
                Verb = EventVerb.Use,
                Object = new() { GadgetId = "front_door", GadgetActionId = GadgetActions.OpenExit },
                CreatedAt = _fakeTime.GetUtcNow().AddMinutes(-5).UtcDateTime
            }
        };
        _clientMock
            .Setup(m => m.Events.ListEventsAsync("created_at:desc"))
            .Returns(events.ToAsyncEnumerable());
        _clientMock
            .Setup(m =>
                m.Gadgets.DoGadgetActionAsync(
                    "light",
                    GadgetActions.LightOff,
                    CancellationToken.None
                )
            )
            .Verifiable();

        // When
        await _controller.TickAsync(CancellationToken.None);

        // Then
        _clientMock.VerifyAll();
    }
}
