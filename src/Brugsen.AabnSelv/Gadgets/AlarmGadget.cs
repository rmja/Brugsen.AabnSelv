using Akiles.ApiClient;
using Akiles.ApiClient.Events;

namespace Brugsen.AabnSelv.Gadgets;

public class AlarmGadget(string gadgetId, ILogger<AlarmGadget>? logger = null) : IAlarmGadget
{
    public string GadgetId { get; } = gadgetId;
    public GadgetEntity GadgetEntity => GadgetEntity.Alarm;

    public AlarmState State { get; private set; } = AlarmState.Unknown;

    public async Task<DateTime?> GetLastArmedAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken
    )
    {
        var events = await client.Events.ListEventsAsync(
            cursor: null,
            limit: 1,
            sort: "created_at:desc",
            new ListEventsFilter()
            {
                Object = new() { GadgetId = GadgetId, GadgetActionId = Actions.AlarmArm }
            },
            EventsExpand.None,
            cancellationToken
        );

        return events.Data.SingleOrDefault()?.CreatedAt;
    }

    public async Task ArmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Arming alarm");
        await client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.AlarmArm, cancellationToken);
        State = AlarmState.Armed;
    }

    public async Task DisarmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Disarming alarm");
        await client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.AlarmDisarm, cancellationToken);
        State = AlarmState.Disarmed;
    }

    public static class Actions
    {
        public const string AlarmArm = "arm";
        public const string AlarmDisarm = "disarm";
    }
}
