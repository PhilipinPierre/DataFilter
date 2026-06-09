using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.PlatformShared.ColumnFilter;

/// <summary>
/// Shared rules for whether an <see cref="ExcelFilterState"/> represents an applied column filter.
/// </summary>
public static class ExcelFilterActiveState
{
    public static bool IsActive(ExcelFilterState? state)
    {
        if (state == null)
            return false;

        return state.CustomOperator != null
            || state.AdditionalCustomCriteria.Count > 0
            || state.OrSearchPatterns.Count > 0
            || state.OrSelectedValues.Count > 0
            || !string.IsNullOrEmpty(state.SearchText)
            || !state.SelectAll
            || state.DistinctValues.Count != state.SelectedValues.Count;
    }
}
