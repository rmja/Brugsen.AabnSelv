using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class FrontDoorGadget(string gadgetId, ILogger<FrontDoorGadget>? logger) : IFrontDoorGadget
{
    public string GadgetId { get; } = gadgetId;
    public GadgetEntity GadgetEntity => GadgetEntity.FrontDoor;

    public Task OpenOnceAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Opening front door for single entry");
        return client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.OpenOnce, cancellationToken);
    }

    public Task<bool> IsClosedAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public static class Actions
    {
        public const string OpenOnce = "open_once";
    }
}
