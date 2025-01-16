using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public class DoorGadget(string gadgetId, IAkilesApiClient client) : GadgetBase(gadgetId, client)
{
    public static class Actions
    {
        public const string OpenEntry = "open_entry";
        public const string OpenExit = "open_exit";
    }
}
