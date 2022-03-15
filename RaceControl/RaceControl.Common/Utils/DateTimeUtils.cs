namespace RaceControl.Common.Utils;

public static class DateTimeUtils
{
    public static DateTime? GetDateTimeFromEpoch(this long seconds)
    {
        if (seconds != default)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(seconds).DateTime;
        }

        return null;
    }
}