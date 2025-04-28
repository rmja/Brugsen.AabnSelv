using Akiles.ApiClient;

namespace Brugsen.AabnSelv.Gadgets;

public class AppAccessGadget(string gadgetId, ILogger<AppAccessGadget>? logger = null)
    : IAppAccessGadget
{
    public string GadgetId { get; } = gadgetId;
    public GadgetEntity GadgetEntity => GadgetEntity.Access;

    public Task CheckInAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Check-in");
        return client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.CheckIn, cancellationToken);
    }

    public Task CheckOutAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Check-out");
        return client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.CheckOut, cancellationToken);
    }

    public static class Actions
    {
        public const string CheckIn = "check_in";
        public const string CheckOut = "check_out";
    }
}
