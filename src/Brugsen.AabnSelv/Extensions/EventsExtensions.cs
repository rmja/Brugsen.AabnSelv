using System.Runtime.CompilerServices;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv;

public static class EventsExtensions
{
    /// <summary>
    /// Get the recent events, starting with the newest event
    /// </summary>
    /// <param name="notBefore"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<Event> ListRecentEventsAsync(
        this IEvents events,
        string gadgetId,
        DateTimeOffset notBefore,
        EventsExpand expand,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (
            var evnt in events
                .ListEventsAsync(sort: "created_at:desc", expand)
                .WithCancellation(cancellationToken)
        )
        {
            if (evnt.CreatedAt < notBefore)
            {
                break;
            }
            if (evnt.Object.GadgetId == gadgetId)
            {
                yield return evnt;
            }
        }
    }
}
