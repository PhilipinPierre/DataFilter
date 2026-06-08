using System.Globalization;
using System.Windows.Data;

namespace DataFilter.Wpf.Converters;

/// <summary>
/// Clamps numeric values to >= 0 for DataGrid CellsPanelHorizontalOffset width bindings.
/// </summary>
public sealed class NonNegativeDoubleConverter : IValueConverter
{
    public static readonly NonNegativeDoubleConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            double d => Math.Max(0, d),
            float f => Math.Max(0, f),
            int i => Math.Max(0, i),
            _ => 0d,
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
