﻿using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class NoopLightGadget : ILightGadget
{
    private readonly ILogger<NoopLightGadget>? _logger;

    public LightState State { get; set; } = LightState.Unknown;

    public NoopLightGadget(ILogger<NoopLightGadget>? logger = null)
    {
        logger?.LogWarning("Using fake noop light gadget");
        _logger = logger;
    }

    public Task TurnOffAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        _logger?.LogInformation("FAKE: Turning off the light");
        State = LightState.Off;
        return Task.CompletedTask;
    }
}
