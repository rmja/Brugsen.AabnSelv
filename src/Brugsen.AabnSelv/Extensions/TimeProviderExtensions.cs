namespace Brugsen.AabnSelv;

public static class TimeProviderExtensions
{
    public static DateTimeOffset GetDateTimeOffset(
        this TimeProvider timeProvider,
        DateTime dateTime,
        bool forceLocalOffset = false
    )
    {
        if (dateTime.Kind == DateTimeKind.Utc)
        {
            if (!forceLocalOffset)
            {
                return new DateTimeOffset(dateTime, TimeSpan.Zero);
            }

            dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeProvider.LocalTimeZone);
        }

        var utcOffset = timeProvider.LocalTimeZone.GetUtcOffset(dateTime);
        return new DateTimeOffset(dateTime, utcOffset);
    }
}
