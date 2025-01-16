using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class AlarmGadget(string gadgetId, IAkilesApiClient client, ILogger<AlarmGadget> logger)
    : GadgetBase(gadgetId, client)
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
        await Client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.AlarmArm, cancellationToken);
        State = AlarmGadgetState.Armed;
    }

    public async Task DisarmAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Disarming alarm");
        await Client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.AlarmDisarm, cancellationToken);
        State = AlarmGadgetState.Disarmed;
    }
}
