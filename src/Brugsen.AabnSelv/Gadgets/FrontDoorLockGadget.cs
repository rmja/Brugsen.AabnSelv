using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class FrontDoorLockGadget(string gadgetId, ILogger<FrontDoorLockGadget> logger)
    : IFrontDoorLockGadget
{
    public Task LockAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Locking front door");
        return client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.Lock, cancellationToken);
    }

    public Task UnlockAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Unlocking front door");
        return client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.Unlock, cancellationToken);
    }

    public static class Actions
    {
        public const string Lock = "lock";
        public const string Unlock = "unlock";
    }
}
