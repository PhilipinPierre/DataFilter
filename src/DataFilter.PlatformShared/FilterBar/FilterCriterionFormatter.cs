using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;
using DataFilter.Localization;

namespace DataFilter.PlatformShared.FilterBar;

/// <summary>
/// Formats pipeline criteria for filter bar chip labels.
/// </summary>
public static class FilterCriterionFormatter
{
    /// <summary>
    /// Formats a criterion for display (column title + operator + value).
    /// </summary>
    public static string Format(
        CriterionPipelineNode criterion,
        Func<string, string>? resolveColumnTitle = null,
        IFormatProvider? formatProvider = null)
    {
        if (criterion == null) throw new ArgumentNullException(nameof(criterion));

        string column = FormatColumnLabel(criterion.PropertyName, resolveColumnTitle);

        if (string.IsNullOrEmpty(column) && string.IsNullOrEmpty(criterion.Operator))
            return LocalizationManager.Instance["FilterBar_NewFilter"];

        string opText = FormatOperatorForBar(criterion.Operator);
        string valueText = FormatValue(criterion.Operator, criterion.Value, formatProvider);

        if (string.IsNullOrEmpty(valueText)
            && criterion.Operator is nameof(FilterOperator.IsNull) or nameof(FilterOperator.IsNotNull))
            return $"{column} {opText}".Trim();

        return string.IsNullOrEmpty(valueText)
            ? $"{column} {opText}".Trim()
            : $"{column} {opText} {valueText}".Trim();
    }

    private static string FormatColumnLabel(string propertyName, Func<string, string>? resolveColumnTitle)
    {
        if (!string.IsNullOrEmpty(propertyName) && resolveColumnTitle != null)
        {
            string resolved = resolveColumnTitle(propertyName);
            if (!string.IsNullOrEmpty(resolved))
                return resolved;
        }

        return string.IsNullOrEmpty(propertyName)
            ? string.Empty
            : ToBarInlinePhrase(HumanizePascalCase(propertyName));
    }

    private static string FormatOperatorForBar(string operatorName)
    {
        if (string.IsNullOrEmpty(operatorName))
            return string.Empty;

        string key = $"FilterOperator_{operatorName}";
        string localized = LocalizationManager.Instance[key];
        string text = localized == key ? HumanizePascalCase(operatorName) : localized;
        return ToBarInlinePhrase(text);
    }

    /// <summary>
    /// Lowercases text for chip labels where the operator appears after the column name.
    /// </summary>
    internal static string ToBarInlinePhrase(string phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase))
            return phrase;

        CultureInfo culture = LocalizationManager.Instance.Culture;
        return phrase.ToLower(culture);
    }

    private static string HumanizePascalCase(string name) =>
        Regex.Replace(name, "([a-z])([A-Z])", "$1 $2", RegexOptions.CultureInvariant);

    private static string FormatValue(string operatorName, object? value, IFormatProvider? formatProvider)
    {
        if (value == null)
            return string.Empty;

        if (operatorName == nameof(FilterOperator.Between) && value is RangeValue range)
        {
            string a = Convert.ToString(range.Min, formatProvider ?? CultureInfo.CurrentCulture) ?? string.Empty;
            string b = Convert.ToString(range.Max, formatProvider ?? CultureInfo.CurrentCulture) ?? string.Empty;
            return $"{a} – {b}";
        }

        if (operatorName is nameof(FilterOperator.In) or nameof(FilterOperator.NotIn))
        {
            if (value is IEnumerable enumerable and not string)
            {
                var parts = enumerable.Cast<object?>()
                    .Select(v => Convert.ToString(v, formatProvider ?? CultureInfo.CurrentCulture) ?? string.Empty)
                    .Where(s => s.Length > 0)
                    .Take(4)
                    .ToList();
                if (parts.Count == 0)
                    return string.Empty;
                string joined = string.Join(", ", parts);
                if (enumerable.Cast<object?>().Count() > parts.Count)
                    joined += ", …";
                return $"({joined})";
            }
        }

        return Convert.ToString(value, formatProvider ?? CultureInfo.CurrentCulture) ?? string.Empty;
    }
}
