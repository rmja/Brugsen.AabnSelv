namespace Brugsen.AabnSelv;

public static class DateTimeEx
{
    public static DateTime Min(DateTime val1, DateTime val2) => val1 < val2 ? val1 : val2;
}
