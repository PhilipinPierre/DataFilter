using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataFilter.Core.Engine;
using DataFilter.Core.Enums;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Filtering.ExcelLike.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DataFilter.PlatformShared.ViewModels;

/// <summary>
/// ViewModel managing the UI state of an Excel-like filter popup.
/// </summary>
public partial class ColumnFilterViewModel : ObservableObject, IColumnFilterViewModel
{
    private bool _internalUpdate;
    private bool _initialFilterActive;
    private HashSet<object> _selectionSnapshot = new();

    /// <inheritdoc />
    public ExcelFilterState FilterState { get; } = new();

    /// <inheritdoc />
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <inheritdoc />
    [ObservableProperty]
    private bool? _selectAll = true;

    /// <inheritdoc />
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Gets the generalized data type of the column.
    /// </summary>
    public FilterDataType DataType { get; }

    /// <inheritdoc />
    [ObservableProperty]
    private ObservableCollection<FilterValueItem> _filterValues = new();

    /// <inheritdoc />
    public ICommand ApplyCommand { get; }

    /// <inheritdoc />
    public ICommand ClearCommand { get; }

    /// <inheritdoc />
    public ICommand SortAscendingCommand { get; }

    /// <inheritdoc />
    public ICommand SortDescendingCommand { get; }

    /// <summary>
    /// Gets the command to add a sub-sort in ascending order.
    /// </summary>
    public ICommand AddSubSortAscendingCommand { get; }

    /// <summary>
    /// Gets the command to add a sub-sort in descending order.
    /// </summary>
    public ICommand AddSubSortDescendingCommand { get; }

    /// <summary>
    /// Gets or sets a value indicating whether to add the current selection to the existing filter instead of replacing it.
    /// </summary>
    [ObservableProperty]
    private bool _addToExistingFilter;

    partial void OnAddToExistingFilterChanged(bool value)
    {
        if (value)
        {
            UpdateSelectionSnapshot();
        }
    }

    /// <summary>
    /// Gets or sets the mode used to merge new criteria with the existing filter.
    /// Default is Union (Logical OR).
    /// </summary>
    [ObservableProperty]
    private AccumulationMode _accumulationMode = AccumulationMode.Union;

    /// <summary>
    /// Command to trigger a fast text search online update.
    /// </summary>
    public IAsyncRelayCommand<string> SearchCommand { get; }

    /// <summary>
    /// Indicates whether the filter is actively filtering data.
    /// Matches <see cref="DataFilter.Filtering.ExcelLike.Models.ExcelFilterDescriptor"/> so the column button stays
    /// highlighted whenever the context still applies a non-trivial filter (including partial list selection
    /// when SelectAll is true but selected count differs from distinct count).
    /// </summary>
    public bool IsFilterActive => HasActiveExcelFilter(FilterState);

    private static bool HasActiveExcelFilter(ExcelFilterState? state)
    {
        if (state == null) return false;
        return state.CustomOperator != null
            || state.AdditionalCustomCriteria.Count > 0
            || !string.IsNullOrEmpty(state.SearchText)
            || !state.SelectAll
            || state.DistinctValues.Count != state.SelectedValues.Count;
    }

    /// <summary>
    /// Gets the list of available custom operators for the current data type.
    /// </summary>
    public ObservableCollection<FilterOperator> AvailableOperators { get; } = new();

    /// <summary>
    /// Gets or sets the currently selected custom operator.
    /// </summary>
    [ObservableProperty]
    private FilterOperator? _selectedCustomOperator;

    /// <summary>
    /// Gets or sets the first value for custom filtering.
    /// </summary>
    [ObservableProperty]
    private string _customValue1 = string.Empty;

    /// <summary>
    /// Gets or sets the second value (e.g. for "Between") for custom filtering.
    /// </summary>
    [ObservableProperty]
    private string _customValue2 = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the custom filter section is expanded.
    /// </summary>
    [ObservableProperty]
    private bool _isCustomFilterExpanded;

    /// <summary>
    /// Action to fetch distinct values.
    /// </summary>
    private Func<string, System.Threading.Tasks.Task<IEnumerable<object>>>? _distinctValuesProvider;

    /// <summary>
    /// Action to invoke when apply is clicked.
    /// </summary>
    private Action<ExcelFilterState>? _onApplyAction;

    /// <summary>
    /// Action to invoke when clear is clicked.
    /// </summary>
    private Action? _onClearAction;

    /// <summary>
    /// Event triggered when the filter should be applied.
    /// </summary>
    public event EventHandler? OnApply;

    /// <summary>
    /// Event triggered when the filter should be cleared.
    /// </summary>
    public event EventHandler? OnClear;

    /// <summary>
    /// Event triggered when the search text changes to notify that distinct values should be re-fetched if async.
    /// </summary>
    public event EventHandler<string>? OnSearchTextChangedEvent;

    private readonly IFilterEvaluator _filterEvaluator;
    private readonly string _blanksDisplayText;

    public ColumnFilterViewModel(
        Func<string, System.Threading.Tasks.Task<IEnumerable<object>>> distinctValuesProvider,
        Action<ExcelFilterState> onApply,
        Action onClear,
        Action<bool>? onSort = null,
        Action<bool>? onAddSubSort = null,
        Type? propertyType = null,
        IFilterEvaluator? filterEvaluator = null,
        string blanksDisplayText = "(Blanks)")
    {
        _blanksDisplayText = blanksDisplayText;
        _distinctValuesProvider = distinctValuesProvider;
        _onApplyAction = onApply;
        _onClearAction = onClear;
        _filterEvaluator = filterEvaluator ?? new FilterEvaluator();
        DataType = DetermineDataType(propertyType);

        ApplyCommand = new RelayCommand(() =>
        {
            var selectedValuesSnapshot = new HashSet<object>();

            if (FilterValues.Count < 1000)
            {
                foreach (var item in FilterValues)
                {
                    item.GetSelectedValues(selectedValuesSnapshot);
                }
            }
            else
            {
                var concurrentBag = new ConcurrentBag<object>();
                var rangePartitioner = Partitioner.Create(0, FilterValues.Count, 1000);
                Parallel.ForEach(rangePartitioner, range =>
                {
                    var localSet = new HashSet<object>();
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        FilterValues[i].GetSelectedValues(localSet);
                    }
                    foreach (var val in localSet) concurrentBag.Add(val);
                });
                selectedValuesSnapshot = concurrentBag.ToHashSet();
            }


            bool effectiveAddToExisting = AddToExistingFilter && (_initialFilterActive || IsFilterActive);

            // Stack a second custom rule (AND) on the same column instead of degrading to In(selected floats)
            // from checkbox intersection — required for range filters to survive data regeneration.
            bool mergeCustomWithExistingCustom = effectiveAddToExisting
                && AccumulationMode == AccumulationMode.Intersection
                && SelectedCustomOperator != null
                && FilterState.CustomOperator != null;

            if (mergeCustomWithExistingCustom)
            {
                FilterState.AdditionalCustomCriteria.Add(new ExcelFilterAdditionalCriterion
                {
                    Operator = SelectedCustomOperator!.Value,
                    Value1 = string.IsNullOrEmpty(CustomValue1) ? null : CustomValue1,
                    Value2 = string.IsNullOrEmpty(CustomValue2) ? null : CustomValue2
                });

                SelectedCustomOperator = null;
                CustomValue1 = string.Empty;
                CustomValue2 = string.Empty;
                IsCustomFilterExpanded = false;

                FilterState.SearchText = string.Empty;
                OnPropertyChanged(nameof(IsFilterActive));
                _onApplyAction?.Invoke(FilterState);
                OnApply?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (effectiveAddToExisting)
            {
                if (AccumulationMode == AccumulationMode.Intersection)
                {
                    // INTERSECTION (AND): Result = Current ∩ Snapshot
                    FilterState.SelectedValues.IntersectWith(selectedValuesSnapshot);
                }
                else
                {
                    // UNION (OR): Result = Current ∪ Snapshot
                    foreach (var val in selectedValuesSnapshot)
                        FilterState.SelectedValues.Add(val);
                }

                FilterState.SelectAll = false; // Cannot be "Select All" if we are accumulating partial results

                // Clear operator UI only; FilterState custom criteria must stay (see Apply below).
                SelectedCustomOperator = null;
                CustomValue1 = string.Empty;
                CustomValue2 = string.Empty;
                IsCustomFilterExpanded = false;
            }
            else
            {
                // Normal mode: replace
                FilterState.SelectedValues.Clear();
                foreach (var val in selectedValuesSnapshot)
                {
                    FilterState.SelectedValues.Add(val);
                }
                FilterState.SelectAll = string.IsNullOrEmpty(SearchText) && SelectAll == true;
            }

            FilterState.SearchText = string.Empty; // Clear search text on apply as visible items were merged

            if (!effectiveAddToExisting)
            {
                FilterState.CustomOperator = SelectedCustomOperator;
                FilterState.CustomValue1 = string.IsNullOrEmpty(CustomValue1) ? null : CustomValue1;
                FilterState.CustomValue2 = string.IsNullOrEmpty(CustomValue2) ? null : CustomValue2;
            }

            OnPropertyChanged(nameof(IsFilterActive));
            _onApplyAction?.Invoke(FilterState);
            OnApply?.Invoke(this, EventArgs.Empty);
        });
        ClearCommand = new RelayCommand(ClearFilter);

        SortAscendingCommand = new RelayCommand(() =>
        {
            onSort?.Invoke(false);
            OnApply?.Invoke(this, EventArgs.Empty); // close popup
        });

        SortDescendingCommand = new RelayCommand(() =>
        {
            onSort?.Invoke(true);
            OnApply?.Invoke(this, EventArgs.Empty); // close popup
        });

        AddSubSortAscendingCommand = new RelayCommand(() =>
        {
            onAddSubSort?.Invoke(false);
            OnApply?.Invoke(this, EventArgs.Empty); // close popup
        });

        AddSubSortDescendingCommand = new RelayCommand(() =>
        {
            onAddSubSort?.Invoke(true);
            OnApply?.Invoke(this, EventArgs.Empty); // close popup
        });

        SearchCommand = new AsyncRelayCommand<string>(async (searchText) =>
        {
            if (_distinctValuesProvider != null && searchText != null)
            {
                IsLoading = true;
                try
                {
                    var vals = await _distinctValuesProvider.Invoke(searchText);
                    await InitializeAsync(vals);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        });

        InitializeAvailableOperators();
    }

    public ColumnFilterViewModel()
    {
        _blanksDisplayText = "(Blanks)";
        ApplyCommand = new RelayCommand(() => OnApply?.Invoke(this, EventArgs.Empty));
        ClearCommand = new RelayCommand(ClearFilter);
        SortAscendingCommand = new RelayCommand(() => { });
        SortDescendingCommand = new RelayCommand(() => { });
        AddSubSortAscendingCommand = new RelayCommand(() => { });
        AddSubSortDescendingCommand = new RelayCommand(() => { });
        SearchCommand = new AsyncRelayCommand<string>(async (txt) => await System.Threading.Tasks.Task.CompletedTask);
        _filterEvaluator = new FilterEvaluator();
        DataType = FilterDataType.Other;
    }

    private void InitializeAvailableOperators()
    {
        AvailableOperators.Clear();
        // AvailableOperators.Add(FilterOperator.None); // Represented by null/empty selection

        switch (DataType)
        {
            case FilterDataType.Text:
                AvailableOperators.Add(FilterOperator.Equals);
                AvailableOperators.Add(FilterOperator.NotEquals);
                AvailableOperators.Add(FilterOperator.Contains);
                AvailableOperators.Add(FilterOperator.NotContains);
                AvailableOperators.Add(FilterOperator.StartsWith);
                AvailableOperators.Add(FilterOperator.EndsWith);
                break;
            case FilterDataType.Number:
            case FilterDataType.Date:
            case FilterDataType.Time:
                AvailableOperators.Add(FilterOperator.Equals);
                AvailableOperators.Add(FilterOperator.NotEquals);
                AvailableOperators.Add(FilterOperator.GreaterThan);
                AvailableOperators.Add(FilterOperator.GreaterThanOrEqual);
                AvailableOperators.Add(FilterOperator.LessThan);
                AvailableOperators.Add(FilterOperator.LessThanOrEqual);
                AvailableOperators.Add(FilterOperator.Between);
                break;
        }
    }

    private FilterDataType DetermineDataType(Type? type)
    {
        if (type == null) return FilterDataType.Other;
        var t = Nullable.GetUnderlyingType(type) ?? type;
        if (t == typeof(string)) return FilterDataType.Text;
        if (t == typeof(int) || t == typeof(long) || t == typeof(double) || t == typeof(float) || t == typeof(decimal) || t == typeof(short)) return FilterDataType.Number;
        if (t == typeof(DateTime) || t == typeof(DateTimeOffset)) return FilterDataType.Date;
        if (t == typeof(TimeSpan)) return FilterDataType.Time;
        if (t == typeof(bool)) return FilterDataType.Boolean;
        return FilterDataType.Other;
    }

    /// <inheritdoc />
    public async System.Threading.Tasks.Task InitializeAsync(IEnumerable<object> distinctValues)
    {
        _internalUpdate = true;

        var distinctList = distinctValues as IList<object> ?? distinctValues.ToList();
        ExcelFilterSelectionReconciler.ReconcileSelectedValues(FilterState, distinctList, dropSelectionsNotInDistinct: false);

        foreach (var item in FilterValues)
        {
            item.PropertyChanged -= Item_PropertyChanged;
        }

        FilterState.DistinctValues.Clear();

        var newFilterValues = new List<FilterValueItem>();

        await System.Threading.Tasks.Task.Run(() =>
        {
            if (DataType == FilterDataType.Date)
            {
                InitializeDateTree(distinctList, newFilterValues);
            }
            else if (DataType == FilterDataType.Time)
            {
                // TODO: Fix TreeView for Time with 15-min intervals
                // InitializeTimeTree(distinctValues, newFilterValues);
                InitializeFlatList(distinctList, newFilterValues);
            }
            else
            {
                InitializeFlatList(distinctList, newFilterValues);
            }
        });

        FilterValues = new ObservableCollection<FilterValueItem>(newFilterValues);

        foreach (var item in FilterValues)
        {
            item.PropertyChanged += Item_PropertyChanged;
        }

        UpdateSelectAllState();
        SyncFilterStateSelectedValues();
        _internalUpdate = false;

        // After distincts are rebuilt: advanced filters drive checkboxes via the evaluator, not only SelectedValues.
        if (FilterState.CustomOperator != null)
        {
            SelectedCustomOperator = FilterState.CustomOperator;
            CustomValue1 = FilterState.CustomValue1?.ToString() ?? string.Empty;
            CustomValue2 = FilterState.CustomValue2?.ToString() ?? string.Empty;
            UpdateSelectionFromCustomFilter();
            UpdateSelectionSnapshot();
        }
    }

    private void InitializeFlatList(IEnumerable<object> distinctValues, List<FilterValueItem> newFilterValues)
    {
        foreach (var val in distinctValues)
        {
            var isSelected = FilterState.SelectAll || (val != null && FilterState.SelectedValues.Contains(val));
            var display = val?.ToString() ?? _blanksDisplayText;
            var item = new FilterValueItem(display, val, null, isSelected);
            newFilterValues.Add(item);

            if (val != null)
                FilterState.DistinctValues.Add(val);
        }
    }

    private void InitializeDateTree(IEnumerable<object> distinctValues, List<FilterValueItem> newFilterValues)
    {
        var validDates = new List<DateTime>();
        var blankItem = (FilterValueItem?)null;

        foreach (var val in distinctValues)
        {
            if (val is DateTime dt) validDates.Add(dt);
            else if (val is DateTimeOffset dto) validDates.Add(dto.DateTime);
            else if (val == null)
            {
                var isSelected = FilterState.SelectAll || FilterState.SelectedValues.Contains(val!);
                blankItem = new FilterValueItem(_blanksDisplayText, null, null, isSelected);
                FilterState.DistinctValues.Add(val!);
            }
        }

        var groupedByYear = validDates.GroupBy(d => d.Year).OrderBy(g => g.Key);
        foreach (var yearGrp in groupedByYear)
        {
            var yearNode = new FilterValueItem(yearGrp.Key.ToString(), null, null, false);

            var groupedByMonth = yearGrp.GroupBy(d => d.Month).OrderBy(g => g.Key);
            foreach (var monthGrp in groupedByMonth)
            {
                var monthName = new DateTime(2000, monthGrp.Key, 1).ToString("MMMM");
                var monthNode = new FilterValueItem(monthName, null, yearNode, false);

                var days = monthGrp.OrderBy(d => d.Day);
                foreach (var day in days)
                {
                    bool isSelected = FilterState.SelectAll || FilterState.SelectedValues.Contains(day);
                    var dayNode = new FilterValueItem(day.Day.ToString("D2"), day, monthNode, isSelected);
                    monthNode.AddChild(dayNode);
                    FilterState.DistinctValues.Add(day);
                }

                monthNode.UpdateStateFromChildren();
                yearNode.AddChild(monthNode);
            }

            yearNode.UpdateStateFromChildren();
            newFilterValues.Add(yearNode);
        }

        if (blankItem != null)
            newFilterValues.Add(blankItem);
    }

    private void InitializeTimeTree(IEnumerable<object> distinctValues, List<FilterValueItem> newFilterValues)
    {
        /*
        var validTimes = new List<TimeSpan>();
        bool tooPrecise = false;
        var blankItem = (FilterValueItem?)null;

        foreach (var val in distinctValues)
        {
            TimeSpan ts = default;
            if (val is TimeSpan timeSpan) ts = timeSpan;
            else if (val is DateTime dt) ts = dt.TimeOfDay;
            else if (val is DateTimeOffset dto) ts = dto.TimeOfDay;
            else if (val == null)
            {
                var isSelected = FilterState.SelectAll || FilterState.SelectedValues.Contains(val!);
                blankItem = new FilterValueItem(_blanksDisplayText, null, null, isSelected);
                FilterState.DistinctValues.Add(val!);
                continue;
            }

            if (ts.Seconds > 0 || ts.Milliseconds > 0 || ts.Minutes % 15 != 0)
            {
                tooPrecise = true;
            }
            validTimes.Add(ts);
        }

        if (tooPrecise)
        {
            // If too precise, just use Flat list or nothing? 
            // "ne fait pas de liste et utilise uniquement la gestion par texte libre" -> Empty FilterValues
            foreach (var ts in validTimes) FilterState.DistinctValues.Add(ts);
            if (blankItem != null) FilterState.DistinctValues.Add(null!);
            return;
        }

        var groupedByHour = validTimes.GroupBy(t => t.Hours).OrderBy(g => g.Key);
        foreach (var hourGrp in groupedByHour)
        {
            var hourStr = hourGrp.Key.ToString("D2") + "h";
            var hourNode = new FilterValueItem(hourStr, null, null, false);

            var groupedByQuarter = hourGrp.GroupBy(t => t.Minutes / 15).OrderBy(g => g.Key);
            foreach (var quarterGrp in groupedByQuarter)
            {
                var quarterNodes = quarterGrp.OrderBy(t => t).Distinct();
                foreach (var quarter in quarterNodes)
                {
                    bool isSelected = FilterState.SelectAll || FilterState.SelectedValues.Contains(quarter);
                    string display = $"{quarter.Hours:D2}:{quarter.Minutes:D2}";
                    var quarterNode = new FilterValueItem(display, quarter, hourNode, isSelected);
                    hourNode.AddChild(quarterNode);
                    FilterState.DistinctValues.Add(quarter);
                }
            }

            hourNode.UpdateStateFromChildren();
            newFilterValues.Add(hourNode);
        }

        if (blankItem != null)
            newFilterValues.Add(blankItem);
        */
    }

    /// <summary>
    /// Loads an existing filter state into this view model asynchronously.
    /// </summary>
    /// <param name="state">The state to copy.</param>
    public async System.Threading.Tasks.Task LoadStateAsync(ExcelFilterState state)
    {
        _internalUpdate = true;

        FilterState.SearchText = state.SearchText;
        FilterState.UseWildcards = state.UseWildcards;
        FilterState.SelectAll = state.SelectAll;

        await System.Threading.Tasks.Task.Run(() =>
        {
            if (ReferenceEquals(FilterState, state)) return;

            FilterState.SelectedValues.Clear();
            foreach (var val in state.SelectedValues)
                FilterState.SelectedValues.Add(val);

            FilterState.DistinctValues.Clear();
            foreach (var val in state.DistinctValues)
                FilterState.DistinctValues.Add(val);
        });

        _initialFilterActive = HasActiveExcelFilter(state);

        if (!ReferenceEquals(FilterState, state))
        {
            FilterState.AdditionalCustomCriteria.Clear();
            foreach (var c in state.AdditionalCustomCriteria)
            {
                FilterState.AdditionalCustomCriteria.Add(new ExcelFilterAdditionalCriterion
                {
                    Operator = c.Operator,
                    Value1 = c.Value1,
                    Value2 = c.Value2
                });
            }
        }

        FilterState.CustomOperator = state.CustomOperator;
        FilterState.CustomValue1 = state.CustomValue1;
        FilterState.CustomValue2 = state.CustomValue2;

        SelectedCustomOperator = state.CustomOperator;
        CustomValue1 = state.CustomValue1?.ToString() ?? string.Empty;
        CustomValue2 = state.CustomValue2?.ToString() ?? string.Empty;
        IsCustomFilterExpanded = state.CustomOperator != null || state.AdditionalCustomCriteria.Count > 0;

        SearchText = FilterState.SearchText;

        // List-only filters: map SelectedValues onto the current FilterValues (new distincts after item source change).
        // Custom / advanced filters: ApplySelectionStateToItemsRecursive uses In-list semantics and would overwrite
        // checkboxes incorrectly; the operator is reapplied after _internalUpdate is cleared.
        if (state.CustomOperator == null)
        {
            foreach (var item in FilterValues)
                ApplySelectionStateToItemsRecursive(item, state.SelectedValues);
            UpdateSelectAllState();
            UpdateSelectionSnapshot();
        }

        _internalUpdate = false;

        if (state.CustomOperator != null && FilterValues.Count > 0)
        {
            UpdateSelectionFromCustomFilter();
            UpdateSelectionSnapshot();
        }

        OnPropertyChanged(nameof(IsFilterActive));
    }

    partial void OnSearchTextChanged(string value)
    {
        if (_internalUpdate) return;
        FilterState.SearchText = value;
        OnSearchTextChangedEvent?.Invoke(this, value);

        if (SearchCommand.CanExecute(value))
        {
            SearchCommand.Execute(value);
        }
    }

    partial void OnSelectedCustomOperatorChanged(FilterOperator? value) => UpdateSelectionFromCustomFilter();
    partial void OnCustomValue1Changed(string value) => UpdateSelectionFromCustomFilter();
    partial void OnCustomValue2Changed(string value) => UpdateSelectionFromCustomFilter();

    private void UpdateSelectionFromCustomFilter()
    {
        if (_internalUpdate || SelectedCustomOperator == null || !FilterValues.Any()) return;

        object? v1 = ConvertCustomValue(CustomValue1);
        object? v2 = ConvertCustomValue(CustomValue2);

        // Optimization/Guard: In Intersection mode, don't wipe everything if the user hasn't typed anything yet
        if (AddToExistingFilter && AccumulationMode == AccumulationMode.Intersection && v1 == null && v2 == null)
        {
            // We skip the immediate UI update for intersection if no values are provided to avoid wiping the previous state
            // as the user starts to interact with the custom filter.
            return;
        }

        _internalUpdate = true;
        try
        {
            bool effectiveAddToExisting = AddToExistingFilter && (_initialFilterActive || IsFilterActive);

            foreach (var item in FilterValues)
            {
                UpdateItemMatchRecursive(item, effectiveAddToExisting);
            }

            UpdateSelectAllState();
            SyncFilterStateSelectedValues();
        }
        finally
        {
            _internalUpdate = false;
        }
    }

    private object? ConvertCustomValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        // We return the raw string for now and let the FilterEvaluator 
        // handle the culture-invariant conversion to the specific target type.
        return value;
    }

    private bool ValueMatchesAllStackedCustomColumnFilters(object? itemValue)
    {
        if (SelectedCustomOperator == null)
            return false;

        object? v1 = ConvertCustomValue(CustomValue1);
        object? v2 = ConvertCustomValue(CustomValue2);
        if (!_filterEvaluator.EvaluateOperator(itemValue, SelectedCustomOperator.Value, v1, v2))
            return false;

        foreach (var extra in FilterState.AdditionalCustomCriteria)
        {
            if (!_filterEvaluator.EvaluateOperator(itemValue, extra.Operator, extra.Value1, extra.Value2))
                return false;
        }

        return true;
    }

    private void UpdateItemMatchRecursive(FilterValueItem item, bool effectiveAddToExisting)
    {
        if (item.Children.Count > 0)
        {
            foreach (var child in item.Children)
            {
                UpdateItemMatchRecursive(child, effectiveAddToExisting);
            }
            item.UpdateStateFromChildren();
        }
        else
        {
            bool matches = ValueMatchesAllStackedCustomColumnFilters(item.Value);

            if (effectiveAddToExisting)
            {
                if (AccumulationMode == AccumulationMode.Intersection)
                {
                    bool wasSelected = item.Value != null && _selectionSnapshot.Contains(item.Value);
                    item.IsSelected = wasSelected && matches;
                }
                else
                {
                    bool wasSelected = item.Value != null && _selectionSnapshot.Contains(item.Value);
                    item.IsSelected = wasSelected || matches;
                }
            }
            else
            {
                item.IsSelected = matches;
            }
        }
    }

    private void UpdateSelectionSnapshot()
    {
        _selectionSnapshot.Clear();
        foreach (var item in FilterValues)
        {
            item.GetSelectedValues(_selectionSnapshot);
        }
    }


    partial void OnSelectAllChanged(bool? value)
    {
        if (_internalUpdate) return;

        bool targetValue = false;
        if (!value.HasValue)
        {
            _internalUpdate = true;
            SelectAll = false;
            targetValue = false;
        }
        else
        {
            _internalUpdate = true;
            targetValue = value.Value;
        }

        foreach (var item in FilterValues)
        {
            item.IsSelected = targetValue;
        }

        FilterState.SelectAll = targetValue;
        SyncFilterStateSelectedValues();
        _internalUpdate = false;
    }

    private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilterValueItem.IsSelected) && !_internalUpdate)
        {
            _internalUpdate = true;
            UpdateSelectAllState();
            SyncFilterStateSelectedValues();
            _internalUpdate = false;
        }
    }

    private void UpdateSelectAllState()
    {
        var selectedCount = FilterValues.Count;
        if (selectedCount == 0 && DataType == FilterDataType.Time)
        {
            // Time list hidden case
            SelectAll = FilterState.SelectAll;
            return;
        }

        bool allSelected = FilterValues.All(x => x.IsSelected == true);
        bool allUnselected = FilterValues.All(x => x.IsSelected == false);

        if (allSelected)
            SelectAll = true;
        else if (allUnselected)
            SelectAll = false;
        else
            SelectAll = null; // indeterminate

        FilterState.SelectAll = SelectAll == true;
    }

    private void SyncFilterStateSelectedValues()
    {
        // Always use sequential sync for SelectedValues to avoid thread-safety issues with HashSet.
        // The overhead is minimal compared to the UI update frequency.
        foreach (var item in FilterValues)
        {
            SyncItemSelectionRecursive(item);
        }
    }

    private void SyncItemSelectionRecursive(FilterValueItem item)
    {
        if (item.Children.Count > 0)
        {
            foreach (var child in item.Children)
                SyncItemSelectionRecursive(child);
        }
        else
        {
            if (item.Value == null) return;
            if (item.IsSelected == true)
                FilterState.SelectedValues.Add(item.Value);
            else if (item.IsSelected == false)
                FilterState.SelectedValues.Remove(item.Value);
        }
    }

    private void ApplySelectionStateToItemsRecursive(FilterValueItem item, ICollection<object> selectedValues)
    {
        if (item.Children.Count > 0)
        {
            foreach (var child in item.Children)
                ApplySelectionStateToItemsRecursive(child, selectedValues);
            
            item.UpdateStateFromChildren();
        }
        else
        {
            if (item.Value != null)
                item.IsSelected = selectedValues.Contains(item.Value);
        }
    }

    /// <summary>
    /// Notifies bindings that <see cref="IsFilterActive"/> should be re-evaluated (e.g. after Excel state reconciliation on data refresh).
    /// </summary>
    public void RaiseFilterActiveChanged() => OnPropertyChanged(nameof(IsFilterActive));

    /// <summary>
    /// Clears local filter UI state and reloads distinct values without invoking the parent clear callback.
    /// Use when the parent context no longer has a filter for this column.
    /// </summary>
    public async System.Threading.Tasks.Task SyncFromClearedContextAsync()
    {
        _internalUpdate = true;
        FilterState.Clear();
        SelectedCustomOperator = null;
        CustomValue1 = string.Empty;
        CustomValue2 = string.Empty;
        IsCustomFilterExpanded = false;
        AddToExistingFilter = false;
        SearchText = string.Empty;
        _internalUpdate = false;

        await SearchCommand.ExecuteAsync(string.Empty);
        OnPropertyChanged(nameof(IsFilterActive));
    }

    private void ClearFilter()
    {
        FilterState.Clear();
        SearchText = string.Empty;
        _internalUpdate = true;
        foreach (var item in FilterValues)
        {
            item.IsSelected = true;
        }
        SelectAll = true;
        AddToExistingFilter = false;
        SelectedCustomOperator = null;
        CustomValue1 = string.Empty;
        CustomValue2 = string.Empty;
        IsCustomFilterExpanded = false;
        _internalUpdate = false;

        OnPropertyChanged(nameof(IsFilterActive));
        OnClear?.Invoke(this, EventArgs.Empty);
        _onClearAction?.Invoke();
    }
}
