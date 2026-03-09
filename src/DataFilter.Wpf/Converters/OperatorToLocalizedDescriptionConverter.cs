using System;
using System.Globalization;
using System.Windows.Data;
using DataFilter.Core.Enums;
using DataFilter.Wpf.Resources;

namespace DataFilter.Wpf.Converters;

/// <summary>
/// Converts a FilterOperator enum value to its localized description.
/// </summary>
public class OperatorToLocalizedDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FilterOperator operatorValue)
        {
            var key = $"FilterOperator_{operatorValue}";
            var localizedString = FilterResources.ResourceManager.GetString(key, FilterResources.Culture) ?? operatorValue.ToString();
            return localizedString;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
