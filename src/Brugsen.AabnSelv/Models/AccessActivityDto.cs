namespace Brugsen.AabnSelv.Models;

public record AccessActivityDto
{
    public required string MemberId { get; set; }
    public required string MemberName { get; set; }
    public ActionEventDto? CheckInEvent { get; set; }
    public ActionEventDto? CheckOutEvent { get; set; }
}
