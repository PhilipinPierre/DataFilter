using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataFilter.Core.Enums;
using DataFilter.Core.Engine;
using DataFilter.Filtering.ExcelLike.Models;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace DataFilter.Blazor.ViewModels;

/// <summary>
/// ViewModel managing the UI state of a Blazor filter popup.
/// </summary>
public partial class BlazorColumnFilterViewModel : ObservableObject, IBlazorColumnFilterViewModel
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

    /// <inheritdoc />
    public ICommand AddSubSortAscendingCommand { get; }

    /// <inheritdoc />
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
    /// </summary>
    public bool IsFilterActive => FilterState != null &&
        (!FilterState.SelectAll || !string.IsNullOrEmpty(FilterState.SearchText) || FilterState.CustomOperator != null
         || FilterState.AdditionalCustomCriteria.Count > 0);

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

    private readonly IFilterEvaluator _filterEvaluator;

    public BlazorColumnFilterViewModel(
        Func<string, System.Threading.Tasks.Task<IEnumerable<object>>> distinctValuesProvider,
        Action<ExcelFilterState> onApply,
        Action onClear,
        Action<bool>? onSort = null,
        Action<bool>? onAddSubSort = null,
        Type? propertyType = null,
        IFilterEvaluator? filterEvaluator = null)
    {
        _distinctValuesProvider = distinctValuesProvider;
        _onApplyAction = onApply;
        _onClearAction = onClear;
        _filterEvaluator = filterEvaluator ?? new FilterEvaluator();
        DataType = DetermineDataType(propertyType);

        ApplyCommand = new RelayCommand(() =>
        {
            var selectedValuesSnapshot = new HashSet<object>();
            foreach (var item in FilterValues)
            {
                item.GetSelectedValues(selectedValuesSnapshot);
            }

            bool effectiveAddToExisting = AddToExistingFilter && (_initialFilterActive || IsFilterActive);

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
                    FilterState.SelectedValues.IntersectWith(selectedValuesSnapshot);
                }
                else
                {
                    foreach (var val in selectedValuesSnapshot)
                        FilterState.SelectedValues.Add(val);
                }
                FilterState.SelectAll = false;

                SelectedCustomOperator = null;
                CustomValue1 = string.Empty;
                CustomValue2 = string.Empty;
                IsCustomFilterExpanded = false;
            }
            else
            {
                FilterState.SelectedValues.Clear();
                foreach (var val in selectedValuesSnapshot)
                {
                    FilterState.SelectedValues.Add(val);
                }
                FilterState.SelectAll = string.IsNullOrEmpty(SearchText) && SelectAll == true;
            }

            FilterState.SearchText = string.Empty;
            FilterState.CustomOperator = SelectedCustomOperator;
            FilterState.CustomValue1 = string.IsNullOrEmpty(CustomValue1) ? null : CustomValue1;
            FilterState.CustomValue2 = string.IsNullOrEmpty(CustomValue2) ? null : CustomValue2;

            OnPropertyChanged(nameof(IsFilterActive));
            _onApplyAction?.Invoke(FilterState);
            OnApply?.Invoke(this, EventArgs.Empty);
        });

        ClearCommand = new RelayCommand(ClearFilter);

        SortAscendingCommand = new RelayCommand(() =>
        {
            onSort?.Invoke(false);
            OnApply?.Invoke(this, EventArgs.Empty);
        });

        SortDescendingCommand = new RelayCommand(() =>
        {
            onSort?.Invoke(true);
            OnApply?.Invoke(this, EventArgs.Empty);
        });

        AddSubSortAscendingCommand = new RelayCommand(() =>
        {
            onAddSubSort?.Invoke(false);
            OnApply?.Invoke(this, EventArgs.Empty);
        });

        AddSubSortDescendingCommand = new RelayCommand(() =>
        {
            onAddSubSort?.Invoke(true);
            OnApply?.Invoke(this, EventArgs.Empty);
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

    private void InitializeAvailableOperators()
    {
        AvailableOperators.Clear();
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

    public async System.Threading.Tasks.Task InitializeAsync(IEnumerable<object> distinctValues)
    {
        _internalUpdate = true;

        foreach (var item in FilterValues)
        {
            item.PropertyChanged -= Item_PropertyChanged;
        }

        FilterState.DistinctValues.Clear();
        var newFilterValues = new List<FilterValueItem>();

        if (DataType == FilterDataType.Date)
        {
            InitializeDateTree(distinctValues, newFilterValues);
        }
        else
        {
            InitializeFlatList(distinctValues, newFilterValues);
        }

        FilterValues = new ObservableCollection<FilterValueItem>(newFilterValues);

        foreach (var item in FilterValues)
        {
            item.PropertyChanged += Item_PropertyChanged;
        }

        UpdateSelectAllState();
        SyncFilterStateSelectedValues();
        _internalUpdate = false;
    }

    private void InitializeFlatList(IEnumerable<object> distinctValues, List<FilterValueItem> newFilterValues)
    {
        foreach (var val in distinctValues)
        {
            var isSelected = FilterState.SelectAll || (val != null && FilterState.SelectedValues.Contains(val));
            var display = val?.ToString() ?? "(Blanks)";
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
                blankItem = new FilterValueItem("(Blanks)", null, null, isSelected);
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
        if (blankItem != null) newFilterValues.Add(blankItem);
    }

    public async System.Threading.Tasks.Task LoadStateAsync(ExcelFilterState state)
    {
        _internalUpdate = true;

        FilterState.SearchText = state.SearchText;
        FilterState.UseWildcards = state.UseWildcards;
        FilterState.SelectAll = state.SelectAll;

        if (!ReferenceEquals(FilterState, state))
        {
            FilterState.SelectedValues.Clear();
            foreach (var val in state.SelectedValues)
                FilterState.SelectedValues.Add(val);

            FilterState.DistinctValues.Clear();
            foreach (var val in state.DistinctValues)
                FilterState.DistinctValues.Add(val);

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

        _initialFilterActive = state != null &&
            (!state.SelectAll || !string.IsNullOrEmpty(state.SearchText) || state.CustomOperator != null
             || state.AdditionalCustomCriteria.Count > 0);

        SelectedCustomOperator = state.CustomOperator;
        CustomValue1 = state.CustomValue1?.ToString() ?? string.Empty;
        CustomValue2 = state.CustomValue2?.ToString() ?? string.Empty;
        IsCustomFilterExpanded = state.CustomOperator != null || state.AdditionalCustomCriteria.Count > 0;

        SearchText = FilterState.SearchText;
        
        foreach (var item in FilterValues)
        {
            ApplySelectionStateToItemsRecursive(item, state.SelectedValues);
        }
        UpdateSelectAllState();
        UpdateSelectionSnapshot();

        _internalUpdate = false;
        OnPropertyChanged(nameof(IsFilterActive));
    }

    partial void OnSearchTextChanged(string value)
    {
        if (_internalUpdate) return;
        FilterState.SearchText = value;

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
        if (_internalUpdate || SelectedCustomOperator == null) return;

        object? v1 = CustomValue1;
        object? v2 = CustomValue2;

        if (AddToExistingFilter && AccumulationMode == AccumulationMode.Intersection && string.IsNullOrEmpty(CustomValue1) && string.IsNullOrEmpty(CustomValue2))
        {
            return;
        }

        _internalUpdate = true;
        try
        {
            FilterOperator op = SelectedCustomOperator.Value;
            bool effectiveAddToExisting = AddToExistingFilter && (_initialFilterActive || IsFilterActive);

            foreach (var item in FilterValues)
            {
                UpdateItemMatchRecursive(item, op, v1, v2, effectiveAddToExisting);
            }

            UpdateSelectAllState();
            SyncFilterStateSelectedValues();
        }
        finally
        {
            _internalUpdate = false;
        }
    }

    private void UpdateItemMatchRecursive(FilterValueItem item, FilterOperator op, object? v1, object? v2, bool effectiveAddToExisting)
    {
        if (item.Children.Count > 0)
        {
            foreach (var child in item.Children)
            {
                UpdateItemMatchRecursive(child, op, v1, v2, effectiveAddToExisting);
            }
            item.UpdateStateFromChildren();
        }
        else
        {
            bool matches = _filterEvaluator.EvaluateOperator(item.Value, op, v1, v2);

            if (effectiveAddToExisting)
            {
                bool wasSelected = item.Value != null && _selectionSnapshot.Contains(item.Value);
                if (AccumulationMode == AccumulationMode.Intersection)
                {
                    item.IsSelected = wasSelected && matches;
                }
                else
                {
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
        _internalUpdate = true;
        bool targetValue = value ?? false;

        foreach (var item in FilterValues)
        {
            item.IsSelected = targetValue;
        }

        FilterState.SelectAll = targetValue;
        SyncFilterStateSelectedValues();
        _internalUpdate = false;
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
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
        if (FilterValues.Count == 0) return;

        bool allSelected = FilterValues.All(x => x.IsSelected == true);
        bool allUnselected = FilterValues.All(x => x.IsSelected == false);

        if (allSelected) SelectAll = true;
        else if (allUnselected) SelectAll = false;
        else SelectAll = null;

        FilterState.SelectAll = SelectAll == true;
    }

    private void SyncFilterStateSelectedValues()
    {
        foreach (var item in FilterValues)
        {
            SyncItemSelectionRecursive(item);
        }
    }

    private void SyncItemSelectionRecursive(FilterValueItem item)
    {
        if (item.Children.Count > 0)
        {
            foreach (var child in item.Children) SyncItemSelectionRecursive(child);
        }
        else
        {
            if (item.Value == null) return;
            if (item.IsSelected == true) FilterState.SelectedValues.Add(item.Value);
            else if (item.IsSelected == false) FilterState.SelectedValues.Remove(item.Value);
        }
    }

    private void ApplySelectionStateToItemsRecursive(FilterValueItem item, ICollection<object> selectedValues)
    {
        if (item.Children.Count > 0)
        {
            foreach (var child in item.Children) ApplySelectionStateToItemsRecursive(child, selectedValues);
            item.UpdateStateFromChildren();
        }
        else
        {
            if (item.Value != null) item.IsSelected = selectedValues.Contains(item.Value);
        }
    }

    /// <inheritdoc />
    public void ClearFilter()
    {
        FilterState.Clear();
        SearchText = string.Empty;
        _internalUpdate = true;
        foreach (var item in FilterValues) item.IsSelected = true;
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
