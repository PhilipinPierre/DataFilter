using System.Globalization;
using System.Reflection;

namespace DataFilter.Core.Engine;

/// <summary>
/// Time-of-day semantics for distinct time values and time-based filtering.
/// </summary>
public static class TimeDistinctHelper
{
    private const string TimeOnlyTypeName = "System.TimeOnly";

    /// <summary>
    /// Returns whether the CLR type represents a time-of-day column.
    /// </summary>
    public static bool IsTimeOfDayType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(TimeSpan) || IsTimeOnlyType(type);
    }

    /// <summary>
    /// Normalizes a value to a canonical instance for distinct lists.
    /// </summary>
    public static object CanonicalizeDistinctValue(object value, Type propertyValueType)
    {
        propertyValueType = Nullable.GetUnderlyingType(propertyValueType) ?? propertyValueType;

        if (propertyValueType == typeof(TimeSpan))
        {
            if (TryGetTimeSpan(value, out var timeSpan))
                return timeSpan;

            return value;
        }

        if (IsTimeOnlyType(propertyValueType))
            return NormalizeToTimeOnly(value);

        return value;
    }

    /// <summary>
    /// Returns whether two values represent the same time-of-day (including fractional seconds).
    /// </summary>
    public static bool AreSameTimeOfDay(object? a, object? b)
    {
        if (a == null || b == null)
            return a == null && b == null;

        if (!TryGetTimeSpan(a, out var left) || !TryGetTimeSpan(b, out var right))
            return Equals(a, b);

        return left == right;
    }

    /// <summary>
    /// Returns whether a property value matches a filter list entry by time-of-day.
    /// </summary>
    public static bool MatchesTimeOfDay(object? propertyValue, object? filterValue)
    {
        if (propertyValue == null || filterValue == null)
            return false;

        return AreSameTimeOfDay(propertyValue, filterValue);
    }

    /// <summary>
    /// Compares two time-of-day values for ordering.
    /// </summary>
    public static int CompareTimeOfDay(object a, object b)
    {
        if (!TryGetTimeSpan(a, out var left) || !TryGetTimeSpan(b, out var right))
            return Comparer<object>.Default.Compare(a, b);

        return left.CompareTo(right);
    }

    /// <summary>
    /// Converts a value to time-of-day parts when possible.
    /// </summary>
    public static bool TryGetTimeParts(object value, out int hour, out int minute, out int second, out int millisecond)
    {
        if (!TryGetTimeSpan(value, out var timeSpan))
        {
            hour = minute = second = millisecond = 0;
            return false;
        }

        hour = timeSpan.Hours;
        minute = timeSpan.Minutes;
        second = timeSpan.Seconds;
        millisecond = timeSpan.Milliseconds;
        return true;
    }

    /// <summary>
    /// Extracts a <see cref="TimeSpan"/> time-of-day representation when possible.
    /// </summary>
    public static bool TryGetTimeSpan(object value, out TimeSpan timeSpan)
    {
        switch (value)
        {
            case TimeSpan ts:
                timeSpan = ts;
                return true;
            case DateTime dt:
                timeSpan = dt.TimeOfDay;
                return true;
            case DateTimeOffset dto:
                timeSpan = dto.TimeOfDay;
                return true;
            default:
                return TryGetTimeOnlyAsTimeSpan(value, out timeSpan);
        }
    }

    private static bool IsTimeOnlyType(Type type)
    {
#if NET6_0_OR_GREATER
        return type == typeof(TimeOnly);
#else
        return type.FullName == TimeOnlyTypeName;
#endif
    }

    private static object NormalizeToTimeOnly(object value)
    {
#if NET6_0_OR_GREATER
        return value switch
        {
            TimeOnly timeOnly => timeOnly,
            TimeSpan ts => TimeOnly.FromTimeSpan(ts),
            DateTime dt => TimeOnly.FromDateTime(dt),
            DateTimeOffset dto => TimeOnly.FromDateTime(dto.DateTime),
            _ => TimeOnly.Parse(value.ToString()!, CultureInfo.InvariantCulture)
        };
#else
        var timeOnlyType = value.GetType().FullName == TimeOnlyTypeName
            ? value.GetType()
            : Type.GetType(TimeOnlyTypeName, throwOnError: false);

        if (timeOnlyType == null)
            return value;

        if (value.GetType().FullName == TimeOnlyTypeName)
            return value;

        if (value is TimeSpan ts)
            return timeOnlyType.GetMethod("FromTimeSpan", new[] { typeof(TimeSpan) })!.Invoke(null, new object[] { ts })!;

        if (value is DateTime dt)
            return timeOnlyType.GetMethod("FromDateTime", new[] { typeof(DateTime) })!.Invoke(null, new object[] { dt })!;

        if (value is DateTimeOffset dto)
            return timeOnlyType.GetMethod("FromDateTime", new[] { typeof(DateTime) })!.Invoke(null, new object[] { dto.DateTime })!;

        return value;
#endif
    }

    private static bool TryGetTimeOnlyAsTimeSpan(object value, out TimeSpan timeSpan)
    {
        if (value.GetType().FullName != TimeOnlyTypeName)
        {
            timeSpan = default;
            return false;
        }

        var ticks = (long)value.GetType()
            .GetProperty("Ticks", BindingFlags.Public | BindingFlags.Instance)!
            .GetValue(value)!;
        timeSpan = TimeSpan.FromTicks(ticks);
        return true;
    }
}
