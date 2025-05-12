namespace Brugsen.AabnSelv.Coop;

public record Bon
{
    public required DateTime Purchased { get; init; }
    public int Number { get; init; }
    public required List<BonLine> Lines { get; init; }
}
