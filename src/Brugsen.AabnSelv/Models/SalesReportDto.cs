using Brugsen.AabnSelv.Coop;

namespace Brugsen.AabnSelv.Models;

public record SalesReportDto
{
    public required DateOnly FirstDate { get; init; }
    public required DateOnly LastDate { get; init; }
    public required List<SalesReportLineDto> Lines { get; init; }
}

public record SalesReportLineDto
{
    public required string MemberId { get; set; }
    public required string MemberName { get; set; }
    public string? CoopMembershipNumber { get; set; }
    public required DateTimeOffset CheckedInAt { get; init; }
    public required DateTimeOffset CheckedOutAt { get; init; }
    public List<Bon> Slips { get; init; } = [];
    public decimal TotalAmount { get; init; }
}
