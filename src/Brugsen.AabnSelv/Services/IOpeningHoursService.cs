using Akiles.ApiClient.Schedules;

namespace Brugsen.AabnSelv.Services;

public interface IOpeningHoursService
{
    Schedule ExtendedSchedule { get; }
}
