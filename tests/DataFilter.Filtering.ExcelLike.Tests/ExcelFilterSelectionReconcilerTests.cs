using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Filtering.ExcelLike.Services;

namespace DataFilter.Filtering.ExcelLike.Tests;

public class ExcelFilterSelectionReconcilerTests
{
    [Fact]
    public void ReconcileSelectedValues_SameReference_KeepsCanonicalInstance()
    {
        var state = new ExcelFilterState();
        var a = new object();
        state.SelectedValues.Add(a);

        var distinct = new List<object> { a, new object() };

        ExcelFilterSelectionReconciler.ReconcileSelectedValues(state, distinct);

        Assert.Single(state.SelectedValues);
        Assert.Same(a, state.SelectedValues.First());
    }

    [Fact]
    public void ReconcileSelectedValues_ValueEquality_ReplacesWithDistinctInstance()
    {
        var state = new ExcelFilterState();
        state.SelectedValues.Add(5m);

        var canonical = 5m;
        var distinct = new List<object> { 1m, canonical, 9m };

        ExcelFilterSelectionReconciler.ReconcileSelectedValues(state, distinct);

        Assert.Single(state.SelectedValues);
        Assert.Equal(5m, state.SelectedValues.First());
        Assert.Same(distinct[1], state.SelectedValues.First());
    }

    [Fact]
    public void ReconcileSelectedValues_NoMatch_DropsStaleSelection()
    {
        var state = new ExcelFilterState();
        state.SelectedValues.Add(42m);

        ExcelFilterSelectionReconciler.ReconcileSelectedValues(state, new List<object> { 1m, 2m });

        Assert.Empty(state.SelectedValues);
    }

    [Fact]
    public void ReconcileSelectedValues_EmptyDistinct_ClearsSelection()
    {
        var state = new ExcelFilterState();
        state.SelectedValues.Add("x");

        ExcelFilterSelectionReconciler.ReconcileSelectedValues(state, Array.Empty<object>());

        Assert.Empty(state.SelectedValues);
    }

    [Fact]
    public void ReconcileSelectedValues_DropDisabled_PreservesOffListSelections()
    {
        var state = new ExcelFilterState();
        state.SelectedValues.Add("Alice");

        ExcelFilterSelectionReconciler.ReconcileSelectedValues(state, new List<object> { "Bob" }, dropSelectionsNotInDistinct: false);

        Assert.Single(state.SelectedValues);
        Assert.Contains("Alice", state.SelectedValues);
    }

    [Fact]
    public void ReconcileSelectedValues_NullSelection_KeepsNullWhenPresentInDistinct()
    {
        var state = new ExcelFilterState();
        state.SelectedValues.Add(null!);

        ExcelFilterSelectionReconciler.ReconcileSelectedValues(state, new List<object> { "A", null! });

        Assert.Single(state.SelectedValues);
        Assert.Null(state.SelectedValues.First());
    }

    [Fact]
    public void ReconcileSelectedValues_TimeOfDay_ReplacesWithDistinctInstance()
    {
        var state = new ExcelFilterState();
        state.SelectedValues.Add(new TimeSpan(8, 15, 30));

        var canonical = new TimeSpan(8, 15, 30);
        var distinct = new List<object> { canonical, new TimeSpan(14, 0, 0) };

        ExcelFilterSelectionReconciler.ReconcileSelectedValues(state, distinct);

        Assert.Single(state.SelectedValues);
        Assert.Equal(canonical, state.SelectedValues.First());
    }
}
