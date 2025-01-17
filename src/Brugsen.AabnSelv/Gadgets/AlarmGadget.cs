using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public class AlarmGadget(string gadgetId, ILogger<AlarmGadget> logger) : IAlarmGadget
{
    private readonly SemaphoreSlim _lock = new(1);

    public AlarmGadgetArmState ArmState { get; private set; } = AlarmGadgetArmState.Unknown;

    public async Task ArmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            logger.LogInformation("Arming alarm");
            await client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.AlarmArm, cancellationToken);
            ArmState = AlarmGadgetArmState.Armed;
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
            logger.LogInformation("Disarming alarm");
            await client.Gadgets.DoGadgetActionAsync(
                gadgetId,
                Actions.AlarmDisarm,
                cancellationToken
            );
            ArmState = AlarmGadgetArmState.Disarmed;
        }
        finally
        {
            _lock.Release();
        }
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
