using System.Globalization;
using System.Windows.Data;
using DataFilter.Core.Enums;
using DataFilter.Wpf.Resources;

namespace DataFilter.Wpf.Converters;

public class AccumulationModeToLocalizedDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AccumulationMode mode)
        {
            return mode switch
            {
                AccumulationMode.Union => FilterResources.ModeUnion,
                AccumulationMode.Intersection => FilterResources.ModeIntersection,
                _ => value.ToString() ?? string.Empty
            };
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
