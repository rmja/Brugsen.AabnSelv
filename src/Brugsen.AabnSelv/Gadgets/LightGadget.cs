using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class LightGadget(string gadgetId, ILogger<LightGadget>? logger = null) : ILightGadget
{
    public string GadgetId { get; } = gadgetId;
    public GadgetEntity GadgetEntity => GadgetEntity.Light;
    public LightState State { get; private set; } = LightState.Unknown;

    public async Task TurnOnAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Turning on the light");
        await client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.LightOn, cancellationToken);
        State = LightState.On;
    }

    public async Task TurnOffAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Turning off the light");
        await client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.LightOff, cancellationToken);
        State = LightState.Off;
    }

    public static class Actions
    {
        public const string LightOn = "on";
        public const string LightOff = "off";
    }
}
