namespace Brugsen.AabnSelv.Endpoints;

public record FrontDoorActivityDto
{
    public required string MemberId { get; set; }
    public required string MemberName { get; set; }
    public DateTime? EnteredAt { get; set; }
    public DateTime? ExitedAt { get; set; }
}
