using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public class AlarmGadget(string gadgetId, ILogger<AlarmGadget>? logger = null) : IAlarmGadget
{
    public async Task<DateTime?> GetLastArmedAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken
    )
    {
        await foreach (
            var evnt in client
                .Events.ListEventsAsync(sort: "created_at:desc")
                .WithCancellation(cancellationToken)
        )
        {
            if (evnt.Object.GadgetId == gadgetId)
            {
                if (evnt.Object.GadgetActionId == Actions.AlarmArm)
                {
                    return evnt.CreatedAt;
                }
            }
        }

        return null;
    }

    public async Task ArmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Arming alarm");
        await client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.AlarmArm, cancellationToken);
    }

    public async Task DisarmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Disarming alarm");
        await client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.AlarmDisarm, cancellationToken);
    }

    public IAsyncEnumerable<Event> GetRecentEventsAsync(
        IAkilesApiClient client,
        DateTimeOffset notBefore,
        EventsExpand expand,
        CancellationToken cancellationToken
    ) => client.Events.ListRecentEventsAsync(gadgetId, notBefore, expand, cancellationToken);

    public static class Actions
    {
        public const string AlarmArm = "arm";
        public const string AlarmDisarm = "disarm";
    }
}
