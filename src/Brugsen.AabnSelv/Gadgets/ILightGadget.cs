using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public interface ILightGadget
{
    Task TurnLightOffAsync(IAkilesApiClient client, CancellationToken cancellationToken);
}
