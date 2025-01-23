using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public interface IFrontDoorGadget
{
    Task OpenOnceAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
    Task<bool> IsClosedAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken = default
    );
}
