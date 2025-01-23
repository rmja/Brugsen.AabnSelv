using Akiles.Api.Schedules;

namespace Brugsen.AabnSelv.Services;

public interface IOpeningHoursService
{
    Schedule RegularSchedule { get; }
    Schedule ExtendedSchedule { get; }
    AccessMode GetAccessMode(DateTimeOffset? at = null);
}
