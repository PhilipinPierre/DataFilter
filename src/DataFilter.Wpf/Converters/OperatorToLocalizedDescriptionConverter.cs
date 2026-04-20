using System;
using System.Globalization;
using System.Windows.Data;
using DataFilter.Core.Enums;
using DataFilter.Localization;
using DataFilter.Localization.Resources;

namespace DataFilter.Wpf.Converters;

/// <summary>
/// Converts a FilterOperator enum value to its localized description.
/// </summary>
public class OperatorToLocalizedDescriptionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length > 0 && values[0] is FilterOperator operatorValue)
        {
            var key = $"FilterOperator_{operatorValue}";
            // values[1] is typically LocalizationManager.Instance.Version (a "tick" to force re-evaluation)
            var localizedString = FilterResources.ResourceManager.GetString(key, LocalizationManager.Instance.Culture) ?? operatorValue.ToString();
            return localizedString;
        }

        return values.Length > 0 ? values[0] : Binding.DoNothing;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
