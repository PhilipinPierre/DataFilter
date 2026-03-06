using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DataFilter.Wpf.Converters;

/// <summary>
/// Converts a null or empty string to Visibility.
/// </summary>
public class NullOrEmptyToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var str = value as string;
        bool isNullOrEmpty = string.IsNullOrEmpty(str);

        if (Invert) isNullOrEmpty = !isNullOrEmpty;

        return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
