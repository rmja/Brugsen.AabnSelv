using Akiles.ApiClient;
using Akiles.ApiClient.Events;

namespace Brugsen.AabnSelv.Gadgets;

public class AlarmGadget(string gadgetId, ILogger<AlarmGadget>? logger = null) : IAlarmGadget
{
    private readonly SemaphoreSlim _lock = new(1);

    public string GadgetId { get; } = gadgetId;
    public GadgetEntity GadgetEntity => GadgetEntity.Alarm;

    public AlarmState State { get; private set; } = AlarmState.Unknown;

    public async Task<DateTime?> GetLastArmedAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken
    )
    {
        var events = await client.Events.ListEventsAsync(
            sort: "created_at:desc",
            new ListEventsFilter()
            {
                Object = new() { GadgetId = GadgetId, GadgetActionId = Actions.AlarmArm },
            },
            EventsExpand.None,
            cursor: null,
            limit: 1,
            cancellationToken
        );

        return events.Data.SingleOrDefault()?.CreatedAt;
    }

    public async Task ArmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await DoArmActionAsync(client, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DisarmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await DoDisarmActionAsync(client, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask<bool> ArmIfNotAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken = default
    )
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (State == AlarmState.Armed)
            {
                return false;
            }

            await DoArmActionAsync(client, cancellationToken);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask<bool> DisarmIfNotAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken = default
    )
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (State == AlarmState.Disarmed)
            {
                return false;
            }
            await DoDisarmActionAsync(client, cancellationToken);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task DoArmActionAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken
    )
    {
        logger?.LogInformation("Arming alarm");
        await client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.AlarmArm, cancellationToken);
        State = AlarmState.Armed;
    }

    private async Task DoDisarmActionAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken
    )
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
