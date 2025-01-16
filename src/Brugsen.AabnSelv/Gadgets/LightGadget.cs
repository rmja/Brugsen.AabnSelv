using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class LightGadget(string gadgetId, IAkilesApiClient client, ILogger<LightGadget> logger)
    : GadgetBase(gadgetId, client)
{
    public static class Actions
    {
        public const string LightOn = "on";
        public const string LightOff = "off";
    }

    public Task TurnLightOffAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Turning off the light");
        return Client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.LightOff, cancellationToken);
    }
}
