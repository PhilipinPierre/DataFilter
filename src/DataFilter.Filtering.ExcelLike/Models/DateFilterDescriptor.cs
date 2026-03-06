using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;
using DataFilter.Filtering.ExcelLike.Enums;

namespace DataFilter.Filtering.ExcelLike.Models;

/// <summary>
/// A filter descriptor designed for DateTime columns.
/// Supports before/after, date ranges, and dynamic period shortcuts (Today, LastMonth, etc.).
/// </summary>
public sealed class DateFilterDescriptor : IFilterDescriptor
{
    /// <inheritdoc />
    public string PropertyName { get; }

    /// <inheritdoc />
    public FilterOperator Operator { get; }

    /// <inheritdoc />
    public object? Value { get; }

    /// <summary>
    /// Gets the optional end date used with <see cref="FilterOperator.Between"/>.
    /// </summary>
    public DateTime? EndDate { get; }

    /// <summary>
    /// Gets the dynamic period to filter by, if specified.
    /// When set, <see cref="Value"/> and <see cref="EndDate"/> are computed at evaluation time.
    /// </summary>
    public DatePeriod? Period { get; }

    /// <summary>
    /// Initializes a new <see cref="DateFilterDescriptor"/> with a static date and operator.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="filterOperator">The operator (use <see cref="FilterOperator.LessThan"/> for Before, <see cref="FilterOperator.GreaterThan"/> for After).</param>
    /// <param name="date">The date value.</param>
    public DateFilterDescriptor(string propertyName, FilterOperator filterOperator, DateTime date)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        Operator = filterOperator;
        Value = date;
    }

    /// <summary>
    /// Initializes a new <see cref="DateFilterDescriptor"/> for a Between date range.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="startDate">The inclusive start of the range.</param>
    /// <param name="endDate">The inclusive end of the range.</param>
    public DateFilterDescriptor(string propertyName, DateTime startDate, DateTime endDate)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        Operator = FilterOperator.Between;
        Value = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Initializes a new <see cref="DateFilterDescriptor"/> using a dynamic period.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="period">The dynamic period to evaluate at filter time.</param>
    public DateFilterDescriptor(string propertyName, DatePeriod period)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        Operator = FilterOperator.Between;
        Period = period;
        Value = null;
    }

    /// <inheritdoc />
    public bool IsMatch(object item)
    {
        if (item == null) return false;

        Type type = item.GetType();
        System.Reflection.PropertyInfo? prop = type.GetProperty(PropertyName);
        if (prop == null) return false;

        object? rawValue = prop.GetValue(item);

        if (rawValue == null)
        {
            return Operator == FilterOperator.IsNull;
        }

        if (Operator == FilterOperator.IsNotNull) return true;

        DateTime propDate = Convert.ToDateTime(rawValue).Date;

        if (Period.HasValue)
        {
            (DateTime start, DateTime end) = ResolvePeriod(Period.Value, DateTime.Today);
            return propDate >= start && propDate <= end;
        }

        if (Operator == FilterOperator.Between)
        {
            DateTime startDate = Convert.ToDateTime(Value).Date;
            DateTime endDate = EndDate.HasValue ? EndDate.Value.Date : startDate;
            return propDate >= startDate && propDate <= endDate;
        }

        DateTime filterDate = Convert.ToDateTime(Value).Date;

        return Operator switch
        {
            FilterOperator.Equals => propDate == filterDate,
            FilterOperator.NotEquals => propDate != filterDate,
            FilterOperator.LessThan => propDate < filterDate,
            FilterOperator.LessThanOrEqual => propDate <= filterDate,
            FilterOperator.GreaterThan => propDate > filterDate,
            FilterOperator.GreaterThanOrEqual => propDate >= filterDate,
            _ => throw new NotSupportedException($"Operator {Operator} is not supported for date filters.")
        };
    }

    private static (DateTime start, DateTime end) ResolvePeriod(DatePeriod period, DateTime today)
    {
        return period switch
        {
            DatePeriod.Today => (today, today),
            DatePeriod.Yesterday => (today.AddDays(-1), today.AddDays(-1)),
            DatePeriod.Tomorrow => (today.AddDays(1), today.AddDays(1)),
            DatePeriod.ThisWeek => (StartOfWeek(today), StartOfWeek(today).AddDays(6)),
            DatePeriod.LastWeek => (StartOfWeek(today).AddDays(-7), StartOfWeek(today).AddDays(-1)),
            DatePeriod.NextWeek => (StartOfWeek(today).AddDays(7), StartOfWeek(today).AddDays(13)),
            DatePeriod.ThisMonth => (new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month))),
            DatePeriod.LastMonth => (new DateTime(today.Year, today.Month, 1).AddMonths(-1), new DateTime(today.Year, today.Month, 1).AddDays(-1)),
            DatePeriod.NextMonth => (new DateTime(today.Year, today.Month, 1).AddMonths(1), new DateTime(today.Year, today.Month, 1).AddMonths(2).AddDays(-1)),
            DatePeriod.ThisQuarter => QuarterRange(today, 0),
            DatePeriod.LastQuarter => QuarterRange(today, -1),
            DatePeriod.ThisYear => (new DateTime(today.Year, 1, 1), new DateTime(today.Year, 12, 31)),
            DatePeriod.LastYear => (new DateTime(today.Year - 1, 1, 1), new DateTime(today.Year - 1, 12, 31)),
            DatePeriod.NextYear => (new DateTime(today.Year + 1, 1, 1), new DateTime(today.Year + 1, 12, 31)),
            _ => throw new NotSupportedException($"Date period {period} is not supported.")
        };
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private static (DateTime start, DateTime end) QuarterRange(DateTime today, int quarterOffset)
    {
        int currentQuarter = (today.Month - 1) / 3;
        int targetQuarter = currentQuarter + quarterOffset;

        int yearsOffset = 0;
        while (targetQuarter < 0)
        {
            targetQuarter += 4;
            yearsOffset--;
        }
        while (targetQuarter >= 4)
        {
            targetQuarter -= 4;
            yearsOffset++;
        }

        int targetYear = today.Year + yearsOffset;
        int startMonth = targetQuarter * 3 + 1;
        DateTime start = new(targetYear, startMonth, 1);
        DateTime end = start.AddMonths(3).AddDays(-1);
        return (start, end);
    }
}
