using DataFilter.Core.Enums;
using DataFilter.PlatformShared.FilterValues;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace DataFilter.WinUI3.Controls;

public sealed class FilterDataTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not FilterDataType dataType || parameter is not string target)
            return Visibility.Collapsed;

        return target switch
        {
            "Date" => dataType == FilterDataType.Date ? Visibility.Visible : Visibility.Collapsed,
            "Time" => dataType == FilterDataType.Time ? Visibility.Visible : Visibility.Collapsed,
            "Text" => dataType is not FilterDataType.Date and not FilterDataType.Time ? Visibility.Visible : Visibility.Collapsed,
            _ => Visibility.Collapsed
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}

public sealed class StringToDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string text || !FilterCustomValueFormats.TryParseDate(text, out var date))
            return null!;

        return date;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset dto)
            return FilterCustomValueFormats.FormatDate(dto.DateTime);

        if (value is DateTime date)
            return FilterCustomValueFormats.FormatDate(date);

        return string.Empty;
    }
}

public sealed class StringToTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string text || !FilterCustomValueFormats.TryParseTime(text, out var time))
            return new TimeSpan(0);

        return time;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is TimeSpan time)
            return FilterCustomValueFormats.FormatTime(time);

        return string.Empty;
    }
}
