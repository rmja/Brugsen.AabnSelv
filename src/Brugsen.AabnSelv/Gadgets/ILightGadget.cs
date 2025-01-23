using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public interface ILightGadget
{
    string GadgetId { get; }
    LightState State { get; }
    Task TurnOnAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
    Task TurnOffAsync(IAkilesApiClient client, CancellationToken cancellationToken = default);
}
