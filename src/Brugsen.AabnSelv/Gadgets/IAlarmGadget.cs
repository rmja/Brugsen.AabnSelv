using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public interface IAlarmGadget : IGadget
{
    AlarmState State { get; }
    Task<DateTime?> GetLastArmedAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken = default
    );
    Task ArmAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
    Task DisarmAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
}
