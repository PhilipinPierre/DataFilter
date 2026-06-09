using System.Collections.Generic;
using DataFilter.Core.Engine;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.Filtering.ExcelLike.Services;

/// <summary>
/// Keeps <see cref="ExcelFilterState.SelectedValues"/> in sync with the current distinct value instances
/// when the backing collection is replaced (new object identities for the same logical values).
/// </summary>
public static class ExcelFilterSelectionReconciler
{
    /// <summary>
    /// Replaces selections with canonical instances from <paramref name="currentDistinctValues"/> when
    /// <see cref="object.Equals(object?, object?)"/> matches; drops entries with no match (stale references).
    /// </summary>
    /// <param name="dropSelectionsNotInDistinct">
    /// When <see langword="true"/> (default for data source replacement), selections with no matching distinct
    /// value are removed. When <see langword="false"/> (e.g. search-narrowed distinct in the popup), those
    /// selections are kept so “add to existing” can still refer to values not in the current distinct list.
    /// </param>
    public static void ReconcileSelectedValues(
        ExcelFilterState state,
        IEnumerable<object> currentDistinctValues,
        bool dropSelectionsNotInDistinct = true)
    {
        if (state.SelectedValues.Count == 0)
            return;

        var distinctList = currentDistinctValues as IList<object> ?? currentDistinctValues.ToList();
        if (distinctList.Count == 0)
        {
            if (dropSelectionsNotInDistinct)
                state.SelectedValues.Clear();
            return;
        }

        // Fast path: same reference as a distinct value (O(1) per selection).
        var distinctByRef = new HashSet<object>(ReferenceEqualityComparer.Instance);
        foreach (var d in distinctList)
            distinctByRef.Add(d);

        var distinctArr = distinctList is object[] arr ? arr : distinctList.ToArray();

        var newSelection = new HashSet<object>();
        foreach (var sel in state.SelectedValues.ToList())
        {
            if (distinctByRef.Contains(sel))
            {
                newSelection.Add(sel);
                continue;
            }

            object? canonical = null;
            var found = false;
            for (int i = 0; i < distinctArr.Length; i++)
            {
                if (EqualsValue(distinctArr[i], sel))
                {
                    canonical = distinctArr[i];
                    found = true;
                    break;
                }
            }

            if (found)
                newSelection.Add(canonical!);
            else if (!dropSelectionsNotInDistinct)
                newSelection.Add(sel);
        }

        state.SelectedValues.Clear();
        foreach (var v in newSelection)
            state.SelectedValues.Add(v);
    }

    private static bool EqualsValue(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if (DateDistinctHelper.TryGetCalendarParts(a, out _, out _, out _)
            && DateDistinctHelper.TryGetCalendarParts(b, out _, out _, out _))
        {
            return DateDistinctHelper.AreSameCalendarDate(a, b);
        }

        return Equals(a, b);
    }
}
