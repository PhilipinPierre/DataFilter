using System.Globalization;
using System.Reflection;

namespace DataFilter.Core.Engine;

/// <summary>
/// Calendar-day semantics for distinct date values and date-based filtering.
/// </summary>
public static class DateDistinctHelper
{
    private const string DateOnlyTypeName = "System.DateOnly";

    /// <summary>
    /// Returns whether the CLR type represents a calendar date column.
    /// </summary>
    public static bool IsCalendarDateType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || IsDateOnlyType(type);
    }

    /// <summary>
    /// Normalizes a value to a single canonical instance per calendar day for distinct lists.
    /// </summary>
    public static object CanonicalizeDistinctValue(object value, Type propertyValueType)
    {
        propertyValueType = Nullable.GetUnderlyingType(propertyValueType) ?? propertyValueType;

        if (propertyValueType == typeof(DateTime))
        {
            if (value is DateTime dt)
                return dt.Date;

            if (value is DateTimeOffset dto)
                return dto.Date;

            if (TryGetCalendarParts(value, out var year, out var month, out var day))
                return new DateTime(year, month, day);
        }

        if (propertyValueType == typeof(DateTimeOffset))
        {
            if (value is DateTimeOffset dto)
                return new DateTimeOffset(dto.Year, dto.Month, dto.Day, 0, 0, 0, dto.Offset);

            if (value is DateTime dt)
                return new DateTimeOffset(dt.Date);

            if (TryGetCalendarParts(value, out var year, out var month, out var day))
                return new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
        }

        if (IsDateOnlyType(propertyValueType))
            return NormalizeToDateOnly(value);

        return value;
    }

    /// <summary>
    /// Returns whether two values represent the same calendar day.
    /// </summary>
    public static bool AreSameCalendarDate(object? a, object? b)
    {
        if (a == null || b == null)
            return a == null && b == null;

        if (!TryGetCalendarParts(a, out var y1, out var m1, out var d1))
            return Equals(a, b);

        if (!TryGetCalendarParts(b, out var y2, out var m2, out var d2))
            return Equals(a, b);

        return y1 == y2 && m1 == m2 && d1 == d2;
    }

    /// <summary>
    /// Returns whether a property value matches a filter list entry by calendar day.
    /// </summary>
    public static bool MatchesCalendarDate(object? propertyValue, object? filterValue)
    {
        if (propertyValue == null || filterValue == null)
            return false;

        return AreSameCalendarDate(propertyValue, filterValue);
    }

    /// <summary>
    /// Compares two calendar date values for ordering (ignores time-of-day).
    /// </summary>
    public static int CompareCalendarDates(object a, object b)
    {
        if (!TryGetCalendarParts(a, out var y1, out var m1, out var d1)
            || !TryGetCalendarParts(b, out var y2, out var m2, out var d2))
        {
            return Comparer<object>.Default.Compare(a, b);
        }

        var left = new DateTime(y1, m1, d1).Ticks;
        var right = new DateTime(y2, m2, d2).Ticks;
        return left.CompareTo(right);
    }

    /// <summary>
    /// Converts a value to calendar date parts when possible.
    /// </summary>
    public static bool TryGetCalendarParts(object value, out int year, out int month, out int day)
    {
        switch (value)
        {
            case DateTime dt:
                year = dt.Year;
                month = dt.Month;
                day = dt.Day;
                return true;
            case DateTimeOffset dto:
                year = dto.Year;
                month = dto.Month;
                day = dto.Day;
                return true;
            default:
                return TryGetDateOnlyParts(value, out year, out month, out day);
        }
    }

    private static bool IsDateOnlyType(Type type)
    {
#if NET6_0_OR_GREATER
        return type == typeof(DateOnly);
#else
        return type.FullName == DateOnlyTypeName;
#endif
    }

    private static object NormalizeToDateOnly(object value)
    {
#if NET6_0_OR_GREATER
        return value switch
        {
            DateOnly dateOnly => dateOnly,
            DateTime dt => DateOnly.FromDateTime(dt),
            DateTimeOffset dto => DateOnly.FromDateTime(dto.Date),
            _ => DateOnly.Parse(value.ToString()!, CultureInfo.InvariantCulture)
        };
#else
        var dateOnlyType = value.GetType().FullName == DateOnlyTypeName
            ? value.GetType()
            : Type.GetType(DateOnlyTypeName, throwOnError: false);

        if (dateOnlyType == null)
            return value;

        if (value.GetType().FullName == DateOnlyTypeName)
            return value;

        if (value is DateTime dt)
            return dateOnlyType.GetMethod("FromDateTime", new[] { typeof(DateTime) })!.Invoke(null, new object[] { dt })!;

        if (value is DateTimeOffset dto)
            return dateOnlyType.GetMethod("FromDateTime", new[] { typeof(DateTime) })!.Invoke(null, new object[] { dto.Date })!;

        return value;
#endif
    }

    private static bool TryGetDateOnlyParts(object value, out int year, out int month, out int day)
    {
        if (value.GetType().FullName != DateOnlyTypeName)
        {
            year = month = day = 0;
            return false;
        }

        var type = value.GetType();
        year = (int)type.GetProperty("Year", BindingFlags.Public | BindingFlags.Instance)!.GetValue(value)!;
        month = (int)type.GetProperty("Month", BindingFlags.Public | BindingFlags.Instance)!.GetValue(value)!;
        day = (int)type.GetProperty("Day", BindingFlags.Public | BindingFlags.Instance)!.GetValue(value)!;
        return true;
    }
}
