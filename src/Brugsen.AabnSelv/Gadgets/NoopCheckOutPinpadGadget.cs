namespace Brugsen.AabnSelv.Gadgets;

public class NoopCheckOutPinpadGadget : ICheckOutPinpadGadget
{
    public string GadgetId { get; } = "noop-check-out-pinpad";
    public GadgetEntity GadgetEntity => GadgetEntity.CheckOutPinpad;
}
