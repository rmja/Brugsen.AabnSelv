using Refit;

namespace Akiles.Api.Events;

public interface IEvents
{
    [Get("/events")]
    Task<PagedList<Event>> ListEventsAsync(
        string? cursor,
        int? limit,
        string? sort,
        EventsExpand expand = EventsExpand.None,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<Event> ListEventsAsync(
        string? sort = null,
        EventsExpand expand = EventsExpand.None
    ) =>
        new PaginationEnumerable<Event>(
            (cursor, cancellationToken) =>
                ListEventsAsync(
                    cursor,
                    Constants.DefaultPaginationLimit,
                    sort,
                    expand,
                    cancellationToken
                )
        );
}
