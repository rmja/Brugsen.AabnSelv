using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public interface IAlarmGadget
{
    Task<DateTime?> GetLastArmedAsync(IAkilesApiClient client, CancellationToken cancellationToken);
    Task ArmAsync(IAkilesApiClient client, CancellationToken cancellationToken);
    Task DisarmAsync(IAkilesApiClient client, CancellationToken cancellationToken);
    IAsyncEnumerable<Event> GetRecentEventsAsync(
        IAkilesApiClient client,
        DateTimeOffset notBefore,
        EventsExpand expand,
        CancellationToken cancellationToken
    );
}
