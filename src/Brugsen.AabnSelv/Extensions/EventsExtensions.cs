using System.Runtime.CompilerServices;
using Akiles.ApiClient.Events;

namespace Brugsen.AabnSelv;

public static class EventsExtensions
{
    /// <summary>
    /// Get the recent events, starting with the newest event
    /// </summary>
    /// <param name="notBefore"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static IAsyncEnumerable<Event> ListRecentGadgetEventsAsync(
        this IEvents events,
        string gadgetId,
        DateTimeOffset notBefore,
        EventsExpand expand,
        CancellationToken cancellationToken
    ) =>
        events.ListRecentEventsAsync(
            notBefore,
            new ListEventsFilter() { Object = new() { GadgetId = gadgetId } },
            expand,
            cancellationToken
        );

    public static async IAsyncEnumerable<Event> ListRecentEventsAsync(
        this IEvents events,
        DateTimeOffset notBefore,
        ListEventsFilter filter,
        EventsExpand expand,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (
            var evnt in events
                .ListEventsAsync(sort: "created_at:desc", filter, expand)
                .WithCancellation(cancellationToken)
        )
        {
            if (evnt.CreatedAt < notBefore)
            {
                break;
            }

            yield return evnt;
        }
    }
}
