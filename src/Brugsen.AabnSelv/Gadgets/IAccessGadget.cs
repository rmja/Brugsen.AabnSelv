using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public interface IAccessGadget : IGadget
{
    Task CheckInAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
    Task CheckOutAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
}
