using Akiles.ApiClient;

namespace Brugsen.AabnSelv.Gadgets;

public interface IFrontDoorGadget : IGadget
{
    Task OpenOnceAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
    Task<bool> IsClosedAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken = default
    );
}
