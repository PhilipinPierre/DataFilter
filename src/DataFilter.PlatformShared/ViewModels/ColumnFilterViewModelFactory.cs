using DataFilter.Core.Engine;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Localization;

namespace DataFilter.PlatformShared.ViewModels;

/// <summary>
/// Shared factory for creating <see cref="ColumnFilterViewModel"/> instances consistently across UI frameworks.
/// </summary>
public static class ColumnFilterViewModelFactory
{
    /// <summary>
    /// Creates a column popup view model wired to a parent <see cref="IFilterableDataGridViewModel"/>.
    /// </summary>
    public static ColumnFilterViewModel Create(
        IFilterableDataGridViewModel parent,
        string propertyName,
        IFilterEvaluator? filterEvaluator = null,
        Func<string>? blanksDisplayTextProvider = null)
    {
        ArgumentNullException.ThrowIfNull(parent);
        if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException("Property name must be provided.", nameof(propertyName));

        return new ColumnFilterViewModel(
            distinctValuesProvider: search => parent.GetDistinctValuesAsync(propertyName, search),
            onApply: state => parent.ApplyColumnFilter(propertyName, state),
            onClear: () => parent.ClearColumnFilter(propertyName),
            onSort: isDesc => parent.ApplySort(propertyName, isDesc),
            onAddSubSort: isDesc => parent.AddSubSort(propertyName, isDesc),
            propertyType: parent.GetPropertyType(propertyName),
            filterEvaluator: filterEvaluator,
            blanksDisplayTextProvider: blanksDisplayTextProvider ?? (() => LocalizationManager.Instance["Blanks"]));
    }

    /// <summary>
    /// Creates a column popup view model wired to delegates (for UIs not backed by <see cref="IFilterableDataGridViewModel"/>).
    /// </summary>
    public static ColumnFilterViewModel Create(
        Func<string, Task<IEnumerable<object>>> distinctValuesProvider,
        Action<ExcelFilterState> onApply,
        Action onClear,
        Action<bool>? onSort = null,
        Action<bool>? onAddSubSort = null,
        Type? propertyType = null,
        IFilterEvaluator? filterEvaluator = null,
        Func<string>? blanksDisplayTextProvider = null)
    {
        ArgumentNullException.ThrowIfNull(distinctValuesProvider);
        ArgumentNullException.ThrowIfNull(onApply);
        ArgumentNullException.ThrowIfNull(onClear);

        return new ColumnFilterViewModel(
            distinctValuesProvider: distinctValuesProvider,
            onApply: onApply,
            onClear: onClear,
            onSort: onSort,
            onAddSubSort: onAddSubSort,
            propertyType: propertyType,
            filterEvaluator: filterEvaluator,
            blanksDisplayTextProvider: blanksDisplayTextProvider ?? (() => LocalizationManager.Instance["Blanks"]));
    }
}

