using Akiles.Api.Schedules;

namespace Brugsen.AabnSelv.Services;

public interface IOpeningHoursService
{
    Schedule ExtendedSchedule { get; }
}
