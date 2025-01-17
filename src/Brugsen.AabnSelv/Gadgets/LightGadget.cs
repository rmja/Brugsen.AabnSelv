﻿using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class LightGadget(string gadgetId, ILogger<LightGadget> logger) : ILightGadget
{
    public static class Actions
    {
        public const string LightOn = "on";
        public const string LightOff = "off";
    }

    public Task TurnLightOffAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger.LogInformation("Turning off the light");
        return client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.LightOff, cancellationToken);
    }
}
