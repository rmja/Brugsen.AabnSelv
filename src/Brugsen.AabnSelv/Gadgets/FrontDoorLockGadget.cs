using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class FrontDoorLockGadget(string gadgetId, ILogger<FrontDoorLockGadget> logger)
    : IFrontDoorLockGadget
{
    public LockState State { get; private set; } = LockState.Unknown;

    public TimeSpan LockOperationDuration { get; } = TimeSpan.FromSeconds(3);
    public TimeSpan UnlockOperationDuration { get; } = TimeSpan.FromSeconds(3);

    public async Task LockAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Locking front door");
        await client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.Lock, cancellationToken);
        State = LockState.Locked;
    }

    public async Task UnlockAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Unlocking front door");
        await client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.Unlock, cancellationToken);
        State = LockState.Unlocked;
    }

    public static class Actions
    {
        public const string Lock = "lock";
        public const string Unlock = "unlock";
    }
}
