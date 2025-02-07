using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public interface IAppAccessGadget : IGadget
{
    Task CheckInAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
    Task CheckOutAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
}
