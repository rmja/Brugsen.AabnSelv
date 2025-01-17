using System.Globalization;
using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public class AlarmGadget(
    string gadgetId,
    TimeProvider timeProvider,
    ILogger<AlarmGadget>? logger = null
) : IAlarmGadget
{
    public async Task<DateTime?> GetLastArmedAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken
    )
    {
        var gadget = await client.Gadgets.GetGadgetAsync(gadgetId, cancellationToken);
        var lastArmed = gadget.Metadata.GetValueOrDefault("last_armed_at");
        if (lastArmed is null)
        {
            return null;
        }
        return DateTime.ParseExact(
            lastArmed,
            "O",
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind
        );
    }

    public async Task ArmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Arming alarm");
        await client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.AlarmArm, cancellationToken);
        var lastArmed = timeProvider
            .GetUtcNow()
            .UtcDateTime.ToString("O", CultureInfo.InvariantCulture);
        await client.Gadgets.EditGadgetAsync(
            gadgetId,
            new() { Metadata = new() { ["last_armed_at"] = lastArmed } },
            CancellationToken.None
        );
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
