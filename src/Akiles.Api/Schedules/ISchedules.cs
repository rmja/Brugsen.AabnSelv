using Refit;

namespace Akiles.Api.Schedules;

public interface ISchedules
{
    [Get("/schedules")]
    Task<PagedList<Schedule>> ListSchedulesAsync(
        string? cursor,
        int? limit,
        string? sort,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<Schedule> ListSchedulesAsync(string? sort = null) =>
        new PaginationEnumerable<Schedule>(
            (cursor, cancellationToken) =>
                ListSchedulesAsync(
                    cursor,
                    Constants.DefaultPaginationLimit,
                    sort,
                    cancellationToken
                )
        );

    [Get("/schedules/{scheduleId}")]
    public Task<Schedule> GetScheduleAsync(
        string scheduleId,
        CancellationToken cancellationToken = default
    );
}
