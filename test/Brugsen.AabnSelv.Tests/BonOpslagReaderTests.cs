using Brugsen.AabnSelv.Coop;

namespace Brugsen.AabnSelv.Tests;

public class BonOpslagReaderTests
{
    private readonly List<Bon> _bons;

    public BonOpslagReaderTests()
    {
        // Given
        // https://mail.google.com/mail/u/0/#inbox/FMfcgzQbfBzPWMcdWTGRBhJccHnLTDfl
        using var file = File.OpenRead("Resources/bon_opslag.2025-april.pdf");
        var reader = new BonOpslagReader(file);

        // When
        _bons = reader.ReadAsync().ToList();
    }

    [Fact]
    public void CanReadFile()
    {
        Assert.Equal(915, _bons.Count);
        Assert.All(_bons, x => Assert.NotEqual(-1, x.Number));
    }

    [Fact]
    public void FirstBon()
    {
        var bon = _bons[0];
        Assert.Equal(new(2025, 04, 01, 08, 18, 00), bon.Purchased);
        Assert.Equal(2, bon.Number);
        Assert.Equal(5, bon.Lines.Count);
    }

    [Fact]
    public void SecondBon()
    {
        var bon = _bons[1];
        Assert.Equal(5, bon.Number);
        Assert.Equal(new(2025, 04, 01, 11, 34, 00), bon.Purchased);
        Assert.Equal(13, bon.Lines.Count);
    }

    [Fact]
    public void EmptyBon()
    {
        var bon = _bons[9];
        Assert.Equal(new(2025, 04, 01, 15, 37, 00), bon.Purchased);
        Assert.Equal(14, bon.Number);
        Assert.Equal(
            "COOP Plus - Stregkode: 2900040405200. Nummer: 31388020 ()",
            bon.Lines[0].Text
        );
    }

    [Fact]
    public void LongBon()
    {
        var bon = GetBon(new(2025, 04, 12), 13);
        Assert.NotNull(bon);
        Assert.Equal(new(2025, 04, 12, 12, 42, 00), bon.Purchased);
        Assert.Equal(68, bon.Lines.Count);
        Assert.Equal(
            new("VARER", "Salg: 8411030051962 (FAMILIA SERRANO) Antal: 2 á kr. 22,50", 45.00m),
            bon.Lines[48]
        );
        Assert.Equal(new("VARER", "Salg: 5707196234084 (PEBER SPEGEPØLSE)", 19.95m), bon.Lines[49]);
    }

    [Fact]
    public void RandomBon1()
    {
        var bon = GetBon(new(2025, 04, 29), 23);
        Assert.NotNull(bon);
        Assert.Equal(new(2025, 04, 29, 18, 25, 00), bon.Purchased);
        Assert.Equal(5, bon.Lines.Count);
        Assert.Equal(new("VARER", "Salg: 5760725222341 (BRYSTFILET 280 G)", 35.95m), bon.Lines[0]);
        Assert.Equal(new("PENGE", "Medlemskonto", 35.95m), bon.Lines[1]);
        Assert.Equal(new("TOTAL", "At betale 35,95"), bon.Lines[2]);
        Assert.Equal(new("TOTAL", "Medlemskøb 35,95"), bon.Lines[3]);
        Assert.Equal(
            new("KUNDEKORT", "COOP Plus - Stregkode: 2900106365677. Nummer: 31988333 ()"),
            bon.Lines[4]
        );
    }

    private Bon? GetBon(DateOnly date, int bonnr) =>
        _bons.FirstOrDefault(x => DateOnly.FromDateTime(x.Purchased) == date && x.Number == bonnr);
}
