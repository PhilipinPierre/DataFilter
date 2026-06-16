using System.Globalization;
using System.Windows.Data;
using DataFilter.PlatformShared.FilterValues;

namespace DataFilter.Wpf.Converters;

/// <summary>
/// Converts between invariant date strings and <see cref="DateTime"/> for <see cref="System.Windows.Controls.DatePicker"/>.
/// </summary>
public sealed class StringToDateConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string text || !FilterCustomValueFormats.TryParseDate(text, out var date))
            return null;

        return date;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
            return FilterCustomValueFormats.FormatDate(date);

        return string.Empty;
    }
}
