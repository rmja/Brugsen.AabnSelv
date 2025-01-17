using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class FrontDoorLockGadget(string gadgetId) : IFrontDoorLockGadget
{
    public Task LockAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
