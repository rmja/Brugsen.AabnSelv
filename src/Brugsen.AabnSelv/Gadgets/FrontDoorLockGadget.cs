using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class FrontDoorLockGadget(string gadgetId) : IFrontDoorLockGadget
{
    public Task LockAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
