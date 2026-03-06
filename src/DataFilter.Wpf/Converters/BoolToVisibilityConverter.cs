using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DataFilter.Wpf.Converters;

/// <summary>
/// Converts a boolean value to a Visibility value.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Indicates whether the logic is inverted.
    /// </summary>
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue;
        if (parameter != null && value != null)
        {
            boolValue = value.ToString() == parameter.ToString();
        }
        else
        {
            boolValue = value is bool b && b;
        }

        if (Invert) boolValue = !boolValue;

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            bool result = visibility == Visibility.Visible;
            return Invert ? !result : result;
        }
        return false;
    }
}
