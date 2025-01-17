using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public interface ILightGadget
{
    Task TurnOffAsync(IAkilesApiClient client, CancellationToken cancellationToken);
}
