﻿using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class LightGadget(string gadgetId, ILogger<LightGadget>? logger = null) : ILightGadget
{
    public Task TurnOnAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Turning on the light");
        return client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.LightOn, cancellationToken);
    }

    public Task TurnOffAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Turning off the light");
        return client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.LightOff, cancellationToken);
    }

    public static class Actions
    {
        public const string LightOn = "on";
        public const string LightOff = "off";
    }
}
