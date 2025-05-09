using Akiles.ApiClient;
using Akiles.ApiClient.Events;

namespace Brugsen.AabnSelv.Services;

public interface IAccessService
{
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
