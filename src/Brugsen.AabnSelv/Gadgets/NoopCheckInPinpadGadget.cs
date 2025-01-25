namespace Brugsen.AabnSelv.Gadgets;

public class NoopCheckInPinpadGadget : ICheckInPinpadGadget
{
    public string GadgetId { get; } = "noop-check-in-pinpad";
    public GadgetEntity GadgetEntity => GadgetEntity.CheckInPinpad;
}
