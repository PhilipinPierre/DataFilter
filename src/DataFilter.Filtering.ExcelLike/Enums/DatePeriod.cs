namespace DataFilter.Filtering.ExcelLike.Enums;

/// <summary>
/// Defines dynamic date period shortcuts for date column filters.
/// </summary>
public enum DatePeriod
{
    /// <summary>Today only.</summary>
    Today,

    /// <summary>Yesterday only.</summary>
    Yesterday,

    /// <summary>Tomorrow only.</summary>
    Tomorrow,

    /// <summary>The current calendar week (Monday to Sunday).</summary>
    ThisWeek,

    /// <summary>The previous calendar week.</summary>
    LastWeek,

    /// <summary>The next calendar week.</summary>
    NextWeek,

    /// <summary>The current calendar month.</summary>
    ThisMonth,

    /// <summary>The previous calendar month.</summary>
    LastMonth,

    /// <summary>The next calendar month.</summary>
    NextMonth,

    /// <summary>The current calendar quarter (Q1–Q4).</summary>
    ThisQuarter,

    /// <summary>The previous calendar quarter.</summary>
    LastQuarter,

    /// <summary>The current calendar year.</summary>
    ThisYear,

    /// <summary>The previous calendar year.</summary>
    LastYear,

    /// <summary>The next calendar year.</summary>
    NextYear
}
