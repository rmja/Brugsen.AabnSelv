using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public interface IFrontDoorLockGadget
{
    LockState State { get; }
    Task LockAsync(IAkilesApiClient client, CancellationToken cancellationToken);
    Task UnlockAsync(IAkilesApiClient client, CancellationToken cancellationToken);
}
