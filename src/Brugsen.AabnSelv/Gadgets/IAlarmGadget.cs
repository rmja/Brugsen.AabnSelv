using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public interface IAlarmGadget
{
    AlarmState State { get; }
    Task<DateTime?> GetLastArmedAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken = default
    );
    Task ArmAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
    Task DisarmAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Event> GetRecentEventsAsync(
        IAkilesApiClient client,
        DateTimeOffset notBefore,
        EventsExpand expand = EventsExpand.None,
        CancellationToken cancellationToken = default
    );
}
