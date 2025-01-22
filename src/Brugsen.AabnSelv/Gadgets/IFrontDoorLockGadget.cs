using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public interface IFrontDoorLockGadget
{
    Task LockAsync(IAkilesApiClient client, CancellationToken cancellationToken);
    Task UnlockAsync(IAkilesApiClient client, CancellationToken cancellationToken);
}
