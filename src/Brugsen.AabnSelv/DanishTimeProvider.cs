using TimeZoneConverter;

namespace Brugsen.AabnSelv;

public class DanishTimeProvider : TimeProvider
{
    public static readonly TimeZoneInfo EuropeCopenhagen = TZConvert.GetTimeZoneInfo(
        "Europe/Copenhagen"
    );

    public override TimeZoneInfo LocalTimeZone => EuropeCopenhagen;
}
