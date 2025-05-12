using System.ComponentModel;
using Brugsen.AabnSelv.Coop;

namespace Brugsen.AabnSelv.Tests;

public class BonOpslagReaderTests
{
    [Fact]
    public void CanReadAprilFile()
    {
        // Given
        using var file = File.OpenRead("Resources/bon_opslag.2025-april.pdf");
        var reader = new BonOpslagReader(file);

        // When
        var bons = reader.ReadAsync().ToList();

        // Then
        Assert.Equal(915, bons.Count);

        var firstBon = bons[0];
        Assert.Equal(5, firstBon.Lines.Count);

        var secondBon = bons[1];
        Assert.Equal(13, secondBon.Lines.Count);

        var longBon = Assert.Single(
            bons,
            x => x.Purchased.Date == new DateTime(2025, 04, 12, 00, 00, 00) && x.Number == 13
        );
        Assert.Equal(68, longBon.Lines.Count);
        Assert.Equal(
            "Salg: 8411030051962 (FAMILIA SERRANO) Antal: 2 á kr. 22,50",
            longBon.Lines[48].Text
        );
        Assert.Equal("Salg: 5707196234084 (PEBER SPEGEPØLSE)", longBon.Lines[49].Text);
    }
}
