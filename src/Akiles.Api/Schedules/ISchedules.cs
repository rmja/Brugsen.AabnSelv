using Refit;

namespace Akiles.Api.Schedules;

public interface ISchedules
{
    [Get("/not/yet/available")]
    public Task TodoAsync();
}
