namespace Brugsen.AabnSelv.Gadgets;

public class CheckOutPinpadGadget(string gadgetId) : ICheckOutPinpadGadget
{
    public string GadgetId { get; } = gadgetId;
    public GadgetEntity GadgetEntity => GadgetEntity.CheckOutPinpad;

    public static class Actions
    {
        public const string CheckOut = "check_out";
    }
}
