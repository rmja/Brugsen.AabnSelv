namespace Brugsen.AabnSelv.Tests;

public class DateTimeTests
{
    [Fact]
    public void DateTimeUtc_DateTimeOffset_CorrectlyCompares_UtcFirst()
    {
        // Given
        var dateTimeUtc = DanishTimeProvider.System.GetUtcNow().UtcDateTime;
        var dateTimeOffset = DanishTimeProvider.System.GetLocalNow();

        // When

        // Then
        Assert.True(dateTimeUtc < dateTimeOffset);
    }

    [Fact]
    public void DateTimeUtc_DateTimeOffset_CorrectlyCompares_LocalFirst()
    {
        // Given
        var dateTimeOffset = DanishTimeProvider.System.GetLocalNow();
        var dateTimeUtc = DanishTimeProvider.System.GetUtcNow().UtcDateTime;

        // When

        // Then
        Assert.True(dateTimeOffset < dateTimeUtc);
    }
}
