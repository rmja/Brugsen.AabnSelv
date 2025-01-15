using TimeZoneConverter;

namespace Brugsen.AabnSelv;

public class DanishTimeProvider : TimeProvider
{
    private static readonly TimeZoneInfo _timezone = TZConvert.GetTimeZoneInfo("Europe/Copenhagen");

    public override TimeZoneInfo LocalTimeZone => _timezone;
}
