using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public class AccessGadget(string gadgetId, ILogger<AccessGadget>? logger = null) : IAccessGadget
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

public record AccessActivity
{
    public required string MemberId { get; init; }
    public Event? CheckInEvent { get; set; }
    public Event? CheckOutEvent { get; set; }
}
