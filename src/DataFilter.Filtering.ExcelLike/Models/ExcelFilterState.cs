using DataFilter.Core.Enums;

namespace DataFilter.Filtering.ExcelLike.Models;

/// <summary>
/// Additional custom criterion AND-combined with <see cref="ExcelFilterState.CustomOperator"/> on the same column.
/// </summary>
public sealed class ExcelFilterAdditionalCriterion
{
    public FilterOperator Operator { get; set; }
    public object? Value1 { get; set; }
    public object? Value2 { get; set; }
}

/// <summary>
/// Represents the state of an Excel-like filter for a specific column.
/// </summary>
public class ExcelFilterState
{
    /// <summary>
    /// Gets or sets the text used to search within distinct values.
    /// </summary>
    public string SearchText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether wildcards (* and ?) are allowed in the search.
    /// Default is false.
    /// </summary>
    public bool UseWildcards { get; set; }

    /// <summary>
    /// Gets the distinct values currently displayed in the filter panel.
    /// </summary>
    public List<object> DistinctValues { get; } = new();

    /// <summary>
    /// Gets the set of values that are explicitly selected by the user.
    /// </summary>
    public HashSet<object> SelectedValues { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether all currently visible distinct values are selected.
    /// </summary>
    public bool SelectAll { get; set; } = true;

    /// <summary>
    /// Gets or sets the custom operator for contextual filtering.
    /// </summary>
    public FilterOperator? CustomOperator { get; set; }

    /// <summary>
    /// Gets or sets the first custom value for contextual filtering.
    /// </summary>
    public object? CustomValue1 { get; set; }

    /// <summary>
    /// Gets or sets the second custom value (e.g. for "Between") for contextual filtering.
    /// </summary>
    public object? CustomValue2 { get; set; }

    /// <summary>
    /// Extra AND criteria on this column (e.g. a second custom rule applied with "add to existing" + intersection).
    /// </summary>
    public List<ExcelFilterAdditionalCriterion> AdditionalCustomCriteria { get; } = new();

    /// <summary>
    /// Search patterns that should be combined with OR on the same column (e.g. StartsWith("Alice") OR StartsWith("Henry")).
    /// This allows persisting "search + SelectAll" unions without materializing an In(list).
    /// </summary>
    public List<string> OrSearchPatterns { get; } = new();

    /// <summary>
    /// Explicit values that participate in the OR group (e.g. StartsWith("Alice") OR In(["Henry 1","Henry 2"])).
    /// Used when the user narrows via search but then selects only a subset of the visible results.
    /// </summary>
    public HashSet<object> OrSelectedValues { get; } = new();

    /// <summary>
    /// Clears the filter state.
    /// </summary>
    public void Clear()
    {
        SearchText = string.Empty;
        DistinctValues.Clear();
        SelectedValues.Clear();
        SelectAll = true;
        CustomOperator = null;
        CustomValue1 = null;
        CustomValue2 = null;
        AdditionalCustomCriteria.Clear();
        OrSearchPatterns.Clear();
        OrSelectedValues.Clear();
    }
}
