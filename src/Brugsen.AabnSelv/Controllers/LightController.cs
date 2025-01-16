using Akiles.Api;
using Akiles.Api.Events;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Controllers;

public class LightController(
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    TimeProvider timeProvider,
    ILogger<LightGadget> gadgetLogger,
    IOptions<BrugsenAabnSelvOptions> options
) : BackgroundService
{
    public LightGadget? LightGadget { get; set; } =
        options.Value.LightGadgetId is not null
            ? new LightGadget(options.Value.LightGadgetId, client, gadgetLogger)
            : null!;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        if (LightGadget is null)
        {
            return Task.CompletedTask;
        }

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5), timeProvider);
        do
        {
            await TickAsync(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
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
                case DoorGadget.Actions.OpenEntry:
                    anyEntries = true;
                    membersInStore.Add(evnt.Subject.MemberId!);
                    break;
                case DoorGadget.Actions.OpenExit:
                    membersInStore.Remove(evnt.Subject.MemberId!);
                    break;
            }
        }

        if (anyEntries && membersInStore.Count == 0)
        {
            await LightGadget!.TurnLightOffAsync(cancellationToken);
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
                    evnt.Object.GadgetActionId == DoorGadget.Actions.OpenEntry
                    || evnt.Object.GadgetActionId == DoorGadget.Actions.OpenExit
                )
            )
            {
                events.Add(evnt);
            }
        }
        return events;
    }
}
