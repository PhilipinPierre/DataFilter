using System.Globalization;
using DataFilter.Core.Enums;
using DataFilter.PlatformShared.FilterValues;

namespace DataFilter.Maui.Controls;

public sealed class FilterDataTypeToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not FilterDataType dataType || parameter is not string target)
            return false;

        return target switch
        {
            "Date" => dataType == FilterDataType.Date,
            "Time" => dataType == FilterDataType.Time,
            "Text" => dataType is not FilterDataType.Date and not FilterDataType.Time,
            _ => false
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class StringToDateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text || !FilterCustomValueFormats.TryParseDate(text, out var date))
            return DateTime.Today;

        return date;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
            return FilterCustomValueFormats.FormatDate(date);

        return string.Empty;
    }
}

public sealed class StringToTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text || !FilterCustomValueFormats.TryParseTime(text, out var time))
            return TimeSpan.Zero;

        return time;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan time)
            return FilterCustomValueFormats.FormatTime(time);

        return string.Empty;
    }
}
