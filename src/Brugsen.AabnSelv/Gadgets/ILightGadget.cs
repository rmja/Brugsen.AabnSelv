using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public interface ILightGadget
{
    Task TurnOnAsync(IAkilesApiClient client, CancellationToken cancellationToken);
    Task TurnOffAsync(IAkilesApiClient client, CancellationToken cancellationToken);
}
