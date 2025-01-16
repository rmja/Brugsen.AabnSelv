using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class AlarmGadget(string gadgetId, IAkilesApiClient client, ILogger<AlarmGadget> logger)
{
    public static class Actions
    {
        public const string AlarmArm = "arm";
        public const string AlarmDisarm = "disarm";
    }

    public AlarmGadgetState State { get; private set; } = AlarmGadgetState.Unknown;

    public async Task ArmAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Arming alarm");
        await client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.AlarmArm, cancellationToken);
        State = AlarmGadgetState.Armed;
    }

    public async Task DisarmAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Disarming alarm");
        await client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.AlarmDisarm, cancellationToken);
        State = AlarmGadgetState.Disarmed;
    }
}
