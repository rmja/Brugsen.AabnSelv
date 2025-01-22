using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public class FrontDoorGadget(string gadgetId, ILogger<FrontDoorGadget>? logger) : IFrontDoorGadget
{
    public Task OpenOnceAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Opening front door for single entry");
        return client.Gadgets.DoGadgetActionAsync(gadgetId, Actions.OpenOnce, cancellationToken);
    }

    public Task<bool> IsClosedAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Event> GetRecentEventsAsync(
        IAkilesApiClient client,
        DateTimeOffset notBefore,
        EventsExpand expand,
        CancellationToken cancellationToken
    ) => client.Events.ListRecentGadgetEventsAsync(gadgetId, notBefore, expand, cancellationToken);

    public static class Actions
    {
        public const string OpenOnce = "open_once";
    }
}
