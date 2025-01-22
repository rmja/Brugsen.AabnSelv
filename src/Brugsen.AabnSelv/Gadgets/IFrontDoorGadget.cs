using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public interface IFrontDoorGadget
{
    Task OpenOnceAsync(IAkilesApiClient client, CancellationToken cancellationToken);
    Task<bool> IsClosedAsync(IAkilesApiClient client, CancellationToken cancellationToken);

    IAsyncEnumerable<Event> GetRecentEventsAsync(
        IAkilesApiClient client,
        DateTimeOffset notBefore,
        EventsExpand expand,
        CancellationToken cancellationToken
    );
}
