using Refit;

namespace Akiles.Api.Schedules;

public interface ISchedules
{
    [Get("/schedules/{scheduleId}")]
    public Task<Schedule> GetScheduleAsync(string scheduleId, CancellationToken cancellationToken);
}
