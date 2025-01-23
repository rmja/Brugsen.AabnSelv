using Brugsen.AabnSelv.Gadgets;

namespace Brugsen.AabnSelv.Controllers;

public interface IAccessController
{
    IAccessGadget AccessGadget { get; }
    Task ProcessCheckInAsync(string eventId, string memberId);
    Task ProcessCheckOutAsync(string eventId, string memberId);
}
