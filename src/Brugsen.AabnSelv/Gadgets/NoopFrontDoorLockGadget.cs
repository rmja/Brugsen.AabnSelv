using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class NoopFrontDoorLockGadget : IFrontDoorLockGadget
{
    private readonly ILogger<NoopLightGadget>? _logger;

    public string GadgetId { get; } = "noop-front-door";
    public LockState State { get; set; } = LockState.Unknown;
    public TimeSpan LockOperationDuration { get; set; } = TimeSpan.Zero;
    public TimeSpan UnlockOperationDuration { get; set; } = TimeSpan.Zero;

    public NoopFrontDoorLockGadget(ILogger<NoopLightGadget>? logger = null)
    {
        logger?.LogWarning("Using fake noop lock gadget");
        _logger = logger;
    }

    public Task LockAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        _logger?.LogInformation("FAKE: Locking front door");
        State = LockState.Locked;
        return Task.CompletedTask;
    }

    public Task UnlockAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        _logger?.LogInformation("FAKE: Unlocking front door");
        State = LockState.Unlocked;
        return Task.CompletedTask;
    }
}
