using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DataFilter.Core.Enums;

namespace DataFilter.Wpf.Converters;

/// <summary>
/// Shows a panel when <see cref="FilterDataType"/> matches the converter parameter (<c>Date</c>, <c>Time</c>, or <c>Text</c>).
/// </summary>
public sealed class FilterDataTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
