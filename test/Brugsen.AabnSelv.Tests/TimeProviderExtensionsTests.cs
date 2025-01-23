using Microsoft.Extensions.Time.Testing;

namespace Brugsen.AabnSelv.Tests;

public class TimeProviderExtensionsTests
{
    [Fact]
    public void CanGetLocalDateTimeOffset_FromDateTime()
    {
        // Given
        var fakeTime = new FakeTimeProvider();
        fakeTime.SetLocalTimeZone(DanishTimeProvider.EuropeCopenhagen);

        // When
        var dtoFromUnspecified = fakeTime.GetLocalDateTimeOffset(
            new DateTime(2025, 01, 23, 11, 55, 00, DateTimeKind.Unspecified)
        );
        var dtoFromLocal = fakeTime.GetLocalDateTimeOffset(
            new DateTime(2025, 01, 23, 11, 55, 00, DateTimeKind.Local)
        );
        var dtoFromUtc = fakeTime.GetLocalDateTimeOffset(
            new DateTime(2025, 01, 23, 10, 55, 00, DateTimeKind.Utc)
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
            new DateTimeOffset(2025, 01, 23, 11, 55, 00, TimeSpan.FromHours(1)),
            dtoFromUtc
        );
    }

    [Fact]
    public void CanGetLocalDateTimeOffset_FromDateTimeOffset()
    {
        // Given
        var fakeTime = new FakeTimeProvider();
        fakeTime.SetLocalTimeZone(DanishTimeProvider.EuropeCopenhagen);

        // When
        var dtoFromLocal = fakeTime.GetLocalDateTimeOffset(
            new DateTimeOffset(2025, 01, 23, 11, 55, 00, TimeSpan.FromHours(1))
        );
        var dtoFromUtc = fakeTime.GetLocalDateTimeOffset(
            new DateTimeOffset(2025, 01, 23, 10, 55, 00, TimeSpan.Zero)
        );

        // Then
        Assert.Equal(
            new DateTimeOffset(2025, 01, 23, 11, 55, 00, TimeSpan.FromHours(1)),
            dtoFromLocal
        );
        Assert.Equal(
            new DateTimeOffset(2025, 01, 23, 11, 55, 00, TimeSpan.FromHours(1)),
            dtoFromUtc
        );
    }
}
