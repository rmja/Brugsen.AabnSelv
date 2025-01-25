namespace Brugsen.AabnSelv.Gadgets;

public class CheckInPinpadGadget(string gadgetId) : ICheckInPinpadGadget
{
    public string GadgetId { get; } = gadgetId;
    public GadgetEntity GadgetEntity => GadgetEntity.CheckInPinpad;

    public static class Actions
    {
        public const string CheckIn = "check_in";
    }
}
