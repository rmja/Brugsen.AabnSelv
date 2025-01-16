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
    public DoorGadget FrontDoorGadget { get; set; } =
        new DoorGadget(options.Value.FrontDoorGadgetId, client);

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
        var now = timeProvider.GetLocalNow();
        var recentEvents = await FrontDoorGadget
            .GetRecentEventsAsync(notBefore: now.AddHours(-1), EventsExpand.None, cancellationToken)
            .ToListAsync(cancellationToken);

        // Change event order to ascending
        recentEvents.Reverse();

        var anyEntries = false;
        var membersInStore = new HashSet<string>();
        foreach (var evnt in recentEvents)
        {
            switch (evnt.Object.GadgetActionId)
            {
                case DoorGadget.Actions.OpenEntry:
                    anyEntries = true;
                    membersInStore.Add(evnt.Object.MemberId!);
                    break;
                case DoorGadget.Actions.OpenExit:
                    membersInStore.Remove(evnt.Object.MemberId!);
                    break;
            }
        }

        if (anyEntries && membersInStore.Count == 0)
        {
            await LightGadget!.TurnLightOffAsync(cancellationToken);
        }
    }
}
