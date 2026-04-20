using System.Globalization;
using System.Windows.Data;
using DataFilter.Core.Enums;
using DataFilter.Localization;

namespace DataFilter.Wpf.Converters;

public class AccumulationModeToLocalizedDescriptionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length > 0 && values[0] is AccumulationMode mode)
        {
            return mode switch
            {
                AccumulationMode.Union => LocalizationManager.Instance["ModeUnion"],
                AccumulationMode.Intersection => LocalizationManager.Instance["ModeIntersection"],
                _ => mode.ToString()
            };
        }
        return string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
