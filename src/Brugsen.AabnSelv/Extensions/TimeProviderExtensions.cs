namespace Brugsen.AabnSelv;

public static class TimeProviderExtensions
{
    public static DateTimeOffset GetLocalDateTimeOffset(
        this TimeProvider timeProvider,
        DateTime value
    )
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            value = TimeZoneInfo.ConvertTimeFromUtc(value, timeProvider.LocalTimeZone);
        }

        var utcOffset = timeProvider.LocalTimeZone.GetUtcOffset(value);
        return new DateTimeOffset(value, utcOffset);
    }

    public static DateTimeOffset GetLocalDateTimeOffset(
        this TimeProvider timeProvider,
        DateTimeOffset value
    )
    {
        return TimeZoneInfo.ConvertTime(value, timeProvider.LocalTimeZone);
    }
}
