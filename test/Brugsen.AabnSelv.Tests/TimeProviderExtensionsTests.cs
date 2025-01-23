using Microsoft.Extensions.Time.Testing;

namespace Brugsen.AabnSelv.Tests;

public class TimeProviderExtensionsTests
{
    [Fact]
    public void CanGetDateTimeOffset()
    {
        // Given
        var fakeTime = new FakeTimeProvider();
        fakeTime.SetLocalTimeZone(DanishTimeProvider.EuropeCopenhagen);

        // When
        var dtoFromUnspecified = fakeTime.GetDateTimeOffset(
            new DateTime(2025, 01, 23, 11, 55, 00, DateTimeKind.Unspecified)
        );
        var dtoFromLocal = fakeTime.GetDateTimeOffset(
            new DateTime(2025, 01, 23, 11, 55, 00, DateTimeKind.Local)
        );
        var dtoFromUtcWithZeroOffset = fakeTime.GetDateTimeOffset(
            new DateTime(2025, 01, 23, 10, 55, 00, DateTimeKind.Utc)
        );
        var dtoFromUtcWithLocalOffset = fakeTime.GetDateTimeOffset(
            new DateTime(2025, 01, 23, 10, 55, 00, DateTimeKind.Utc),
            forceLocalOffset: true
        );

        // Then
        Assert.Equal(
            new DateTimeOffset(2025, 01, 23, 11, 55, 00, TimeSpan.FromHours(1)),
            dtoFromUnspecified
        );
        Assert.Equal(
            new DateTimeOffset(2025, 01, 23, 11, 55, 00, TimeSpan.FromHours(1)),
            dtoFromLocal
        );
        Assert.Equal(
            new DateTimeOffset(2025, 01, 23, 10, 55, 00, TimeSpan.Zero),
            dtoFromUtcWithZeroOffset
        );
        Assert.Equal(
            new DateTimeOffset(2025, 01, 23, 11, 55, 00, TimeSpan.FromHours(1)),
            dtoFromUtcWithLocalOffset
        );
    }
}
