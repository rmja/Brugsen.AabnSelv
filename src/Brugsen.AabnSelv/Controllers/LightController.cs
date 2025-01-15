using Akiles.Api;
using Akiles.Api.Events;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Controllers;

public class LightController(
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    TimeProvider timeProvider,
    ILogger<LightController> logger,
    IOptions<BrugsenAabnSelvOptions> options
) : BackgroundService
{
    private string _lightGadgetId = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options.Value.LightGadgetId is null)
        {
            logger.LogWarning("Light controller is disabled: No light gadget configured");
            return;
        }
        _lightGadgetId = options.Value.LightGadgetId;

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5), timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await TickAsync(stoppingToken);
        }
    }

    public async Task TickAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetLocalNow().DateTime;
        var recentOpenEvents = await GetRecentFrontDoorOpenEventsAsync(
            notBefore: now.AddHours(-1),
            cancellationToken
        );

        var anyEntries = false;
        var membersInStore = new HashSet<string>();
        foreach (var evnt in recentOpenEvents.OrderBy(x => x.CreatedAt))
        {
            switch (evnt.Object.GadgetActionId)
            {
                case GadgetActions.OpenEntry:
                    anyEntries = true;
                    membersInStore.Add(evnt.Subject.MemberId!);
                    break;
                case GadgetActions.OpenExit:
                    membersInStore.Remove(evnt.Subject.MemberId!);
                    break;
            }
        }

        if (anyEntries && membersInStore.Count == 0)
        {
            await TurnLightOffAsync(cancellationToken);
        }
    }

    private async Task<List<Event>> GetRecentFrontDoorOpenEventsAsync(
        DateTime notBefore,
        CancellationToken cancellationToken
    )
    {
        var events = new List<Event>();
        await foreach (
            var evnt in client
                .Events.ListEventsAsync(sort: "created_at:desc")
                .WithCancellation(cancellationToken)
        )
        {
            if (evnt.CreatedAt < notBefore)
            {
                break;
            }
            if (
                evnt.Object.GadgetId == options.Value.FrontDoorGadgetId
                && (
                    evnt.Object.GadgetActionId == GadgetActions.OpenEntry
                    || evnt.Object.GadgetActionId == GadgetActions.OpenExit
                )
            )
            {
                events.Add(evnt);
            }
        }
        return events;
    }

    private Task TurnLightOffAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Turning off the light");

        return client.Gadgets.DoGadgetActionAsync(
            _lightGadgetId,
            GadgetActions.LightOff,
            cancellationToken
        );
    }
}
