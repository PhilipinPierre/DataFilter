using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DataFilter.Wpf.Converters;

/// <summary>
/// Changes the color of the filter button depending on whether the filter is active.
/// </summary>
public class FilterActiveToColorConverter : IValueConverter
{
    public Brush? ActiveBrush { get; set; }
    public Brush? InactiveBrush { get; set; }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isActive = value is bool b && b;
        return isActive ? ActiveBrush : InactiveBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
