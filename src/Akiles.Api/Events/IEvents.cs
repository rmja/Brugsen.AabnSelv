using Refit;

namespace Akiles.Api.Events;

public interface IEvents
{
    [Get("/events")]
    Task<PagedList<Event>> ListEventsAsync(
        string? cursor,
        int? limit,
        string? sort,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<Event> ListEventsAsync(string? sort = null) =>
        new PaginationEnumerable<Event>(
            (cursor, cancellationToken) =>
                ListEventsAsync(cursor, Constants.DefaultPaginationLimit, sort, cancellationToken)
        );
}
