using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public interface IFrontDoorGadget
{
    Task OpenOnceAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
    Task<bool> IsClosedAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken = default
    );
}
