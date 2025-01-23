using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public interface ILightGadget
{
    LightState State { get; }
    Task TurnOnAsync(IAkilesApiClient client, CancellationToken cancellationToken);
    Task TurnOffAsync(IAkilesApiClient client, CancellationToken cancellationToken);
}
