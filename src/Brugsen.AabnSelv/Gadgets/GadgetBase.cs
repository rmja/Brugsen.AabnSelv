using System.Runtime.CompilerServices;
using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public abstract class GadgetBase(string gadgetId, IAkilesApiClient client)
{
    public string GadgetId { get; } = gadgetId;
    protected IAkilesApiClient Client { get; } = client;

    /// <summary>
    /// Get the recent events, starting with the newest event
    /// </summary>
    /// <param name="notBefore"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<Event> GetRecentEventsAsync(
        DateTimeOffset notBefore,
        EventsExpand expand,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (
            var evnt in Client
                .Events.ListEventsAsync(sort: "created_at:desc", expand)
                .WithCancellation(cancellationToken)
        )
        {
            if (evnt.CreatedAt < notBefore)
            {
                break;
            }
            if (evnt.Object.GadgetId == GadgetId)
            {
                yield return evnt;
            }
        }
    }
}
