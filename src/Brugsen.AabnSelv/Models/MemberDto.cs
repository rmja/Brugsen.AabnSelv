namespace Brugsen.AabnSelv.Models;

public record MemberDto
{
    public string Id { get; set; } = null!;
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required string Address { get; init; }
    public required string Phone { get; init; }
    public required int CoopMembershipNumber { get; init; }
    public required long LaesoeCardNumber { get; init; }
    public LaesoeCardColor LaesoeCardColor { get; init; }
    public bool IsApproved { get; init; }
}
