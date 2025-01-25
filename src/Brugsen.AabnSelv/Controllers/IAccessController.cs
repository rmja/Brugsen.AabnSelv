namespace Brugsen.AabnSelv.Controllers;

public interface IAccessController
{
    Task ProcessCheckInAsync(string eventId, string memberId);
    Task ProcessCheckOutAsync(string eventId, string memberId, bool enforceCheckedIn);
}
