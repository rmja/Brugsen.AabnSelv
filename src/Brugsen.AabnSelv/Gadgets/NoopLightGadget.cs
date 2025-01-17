using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class NoopLightGadget : ILightGadget
{
    private readonly ILogger<NoopLightGadget> _logger;

    public NoopLightGadget(ILogger<NoopLightGadget> logger)
    {
        logger.LogWarning("Using fake noop light gadget");

        _logger = logger;
    }

    public Task TurnLightOffAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        _logger.LogInformation("FAKE: Turning off the light");
        return Task.CompletedTask;
    }
}
