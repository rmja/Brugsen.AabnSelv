namespace Brugsen.AabnSelv.Gadgets;

public class NoopCheckInPinpadGadget : ICheckInPinpadGadget
{
    public string GadgetId { get; } = "noop-check-in-pinpad";
    public GadgetEntity GadgetEntity => GadgetEntity.CheckInPinpad;

    public NoopCheckInPinpadGadget(ILogger<NoopCheckInPinpadGadget>? logger = null)
    {
        logger?.LogWarning("Using fake noop check-in pinpad");
    }
}
