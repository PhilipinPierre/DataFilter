using System.Globalization;

namespace DataFilter.PlatformShared.FilterValues;

/// <summary>
/// Invariant string formats for advanced filter value fields bound to date/time pickers.
/// </summary>
public static class FilterCustomValueFormats
{
    public const string DateFormat = "yyyy-MM-dd";
    public const string TimeFormat = @"hh\:mm\:ss";

    public static bool TryParseDate(string? text, out DateTime date)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            date = default;
            return false;
        }

        return DateTime.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
            || DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    public static string FormatDate(DateTime date) =>
        date.Date.ToString(DateFormat, CultureInfo.InvariantCulture);

    public static bool TryParseTime(string? text, out TimeSpan time)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            time = default;
            return false;
        }

        return TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out time);
    }

    public static string FormatTime(TimeSpan time) =>
        time.ToString(TimeFormat, CultureInfo.InvariantCulture);
}
