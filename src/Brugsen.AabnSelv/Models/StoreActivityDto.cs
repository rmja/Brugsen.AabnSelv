namespace Brugsen.AabnSelv.Endpoints;

public record StoreActivityDto
{
    public required string MemberId { get; set; }
    public required string MemberName { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
}
