namespace Brugsen.AabnSelv.Controllers;

public interface IAccessController
{
    Task ProcessCheckInAsync(string eventId, string memberId, bool openDoor);
    Task ProcessCheckOutAsync(string eventId, string memberId, bool openDoor);
}
