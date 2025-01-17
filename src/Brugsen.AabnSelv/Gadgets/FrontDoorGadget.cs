using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public class FrontDoorGadget(string gadgetId) : IFrontDoorGadget
{
    public IAsyncEnumerable<Event> GetRecentEventsAsync(
        IAkilesApiClient client,
        DateTimeOffset notBefore,
        EventsExpand expand,
        CancellationToken cancellationToken
    ) => client.Events.ListRecentEventsAsync(gadgetId, notBefore, expand, cancellationToken);

    public static class Actions
    {
        public const string OpenEntry = "open_entry";
        public const string OpenExit = "open_exit";
    }
}
