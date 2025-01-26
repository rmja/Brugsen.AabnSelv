using System.ComponentModel.DataAnnotations;

namespace Brugsen.AabnSelv.Models;

public record MemberDto
{
    public string Id { get; set; } = null!;
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required string Address { get; init; }

    [RegularExpression("^\\+[0-9]+$")]
    public required string Phone { get; init; }

    [RegularExpression("^[0-9]{8}$")]
    public required string CoopMembershipNumber { get; init; }

    [RegularExpression("^[0-9]{10}$")]
    public required string LaesoeCardNumber { get; init; }
    public LaesoeCardColor LaesoeCardColor { get; init; }
    public bool IsApproved { get; init; }
}
