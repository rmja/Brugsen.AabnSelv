using Microsoft.Extensions.Time.Testing;

namespace Brugsen.AabnSelv.Tests;

public static class FakeTimeProviderExtensions
{
    public static void SetLocalNow(this FakeTimeProvider fakeTime, DateTime now)
    {
        var nowUtc = TimeZoneInfo.ConvertTimeToUtc(now, fakeTime.LocalTimeZone);
        fakeTime.SetUtcNow(new DateTimeOffset(nowUtc, TimeSpan.Zero));
    }
}