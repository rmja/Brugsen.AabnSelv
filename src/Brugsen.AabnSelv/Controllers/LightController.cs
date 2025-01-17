using Akiles.Api;
using Akiles.Api.Events;
using Brugsen.AabnSelv.Gadgets;

namespace Brugsen.AabnSelv.Controllers;

public class LightController(
    ILightGadget lightGadget,
    IFrontDoorGadget frontDoorGadget,
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    TimeProvider timeProvider
) : BackgroundService
{
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
        var recentEvents = await frontDoorGadget
            .GetRecentEventsAsync(
                client,
                notBefore: now.AddHours(-1),
                EventsExpand.None,
                cancellationToken
            )
            .ToListAsync(cancellationToken);

        // Change event order to ascending
        recentEvents.Reverse();

        var anyEntries = false;
        var membersInStore = new HashSet<string>();
        foreach (var evnt in recentEvents)
        {
            switch (evnt.Object.GadgetActionId)
            {
                case FrontDoorGadget.Actions.OpenEntry:
                    anyEntries = true;
                    membersInStore.Add(evnt.Object.MemberId!);
                    break;
                case FrontDoorGadget.Actions.OpenExit:
                    membersInStore.Remove(evnt.Object.MemberId!);
                    break;
            }
        }

        if (anyEntries && membersInStore.Count == 0)
        {
            await lightGadget.TurnLightOffAsync(client, cancellationToken);
        }
    }
}
