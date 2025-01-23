using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public interface IAccessGadget
{
    string GadgetId { get; }
    Task CheckInAsync(IAkilesApiClient client, CancellationToken cancellationToken);
    Task CheckOutAsync(IAkilesApiClient client, CancellationToken cancellationToken);
    Task<bool> IsMemberCheckedInAsync(
        IAkilesApiClient client,
        string memberId,
        DateTimeOffset notBefore,
        string? ignoreEventId = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> IsAnyCheckedInAsync(
        IAkilesApiClient client,
        DateTimeOffset notBefore,
        CancellationToken cancellationToken = default
    );
    Task<List<AccessActivity>> GetActivityAsync(
        IAkilesApiClient client,
        string? memberId,
        DateTimeOffset notBefore,
        EventsExpand expand = EventsExpand.None,
        CancellationToken cancellationToken = default
    );
}
