using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataFilter.Core.Enums;
using DataFilter.Filtering.ExcelLike.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DataFilter.Wpf.Enums;

namespace DataFilter.Wpf.ViewModels;

/// <summary>
/// ViewModel managing the UI state of an Excel-like filter popup.
/// </summary>
public partial class ColumnFilterViewModel : ObservableObject, IColumnFilterViewModel
{
    private bool _internalUpdate;

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
    public ObservableCollection<FilterValueItem> FilterValues { get; } = new();

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

    /// <summary>
    /// Command to trigger a fast text search online update.
    /// </summary>
    public IAsyncRelayCommand<string> SearchCommand { get; }

    /// <summary>
    /// Indicates whether the filter is actively filtering data.
    /// </summary>
    public bool IsFilterActive => FilterState != null &&
        (!FilterState.SelectAll || !string.IsNullOrEmpty(FilterState.SearchText) || FilterState.CustomOperator != null);

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

    public ColumnFilterViewModel(
        Func<string, System.Threading.Tasks.Task<IEnumerable<object>>> distinctValuesProvider,
        Action<ExcelFilterState> onApply,
        Action onClear,
        Action<bool>? onSort = null,
        Action<bool>? onAddSubSort = null,
        Type? propertyType = null)
    {
        _distinctValuesProvider = distinctValuesProvider;
        _onApplyAction = onApply;
        _onClearAction = onClear;
        DataType = DetermineDataType(propertyType);

        ApplyCommand = new RelayCommand(() =>
        {
            var selectedValuesSnapshot = new HashSet<object>();
            foreach (var item in FilterValues)
            {
                item.GetSelectedValues(selectedValuesSnapshot);
            }

            if (AddToExistingFilter)
            {
                // In accumulation mode, we merge current selection with previous one
                foreach (var val in selectedValuesSnapshot)
                {
                    FilterState.SelectedValues.Add(val);
                }
                FilterState.SelectAll = false; // Cannot be "Select All" if we are accumulating partial results
            }
            else
            {
                // Normal mode: replace
                FilterState.SelectedValues.Clear();
                foreach (var val in selectedValuesSnapshot)
                {
                    FilterState.SelectedValues.Add(val);
                }
                FilterState.SelectAll = SelectAll == true;
            }

            FilterState.SearchText = string.Empty; // Clear search text on apply as visible items were merged
            
            // Apply Custom Filter
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
                    Initialize(vals);
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
        ApplyCommand = new RelayCommand(() => OnApply?.Invoke(this, EventArgs.Empty));
        ClearCommand = new RelayCommand(ClearFilter);
        SortAscendingCommand = new RelayCommand(() => { });
        SortDescendingCommand = new RelayCommand(() => { });
        AddSubSortAscendingCommand = new RelayCommand(() => { });
        AddSubSortDescendingCommand = new RelayCommand(() => { });
        SearchCommand = new AsyncRelayCommand<string>(async (txt) => await System.Threading.Tasks.Task.CompletedTask);
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
    public void Initialize(IEnumerable<object> distinctValues)
    {
        _internalUpdate = true;

        foreach (var item in FilterValues)
        {
            item.PropertyChanged -= Item_PropertyChanged;
        }
        FilterValues.Clear();
        FilterState.DistinctValues.Clear();

        if (DataType == FilterDataType.Date)
        {
            InitializeDateTree(distinctValues);
        }
        else if (DataType == FilterDataType.Time)
        {
            InitializeTimeTree(distinctValues);
        }
        else
        {
            InitializeFlatList(distinctValues);
        }

        foreach (var item in FilterValues)
        {
            item.PropertyChanged += Item_PropertyChanged;
        }

        UpdateSelectAllState();
        SyncFilterStateSelectedValues();
        _internalUpdate = false;
    }

    private void InitializeFlatList(IEnumerable<object> distinctValues)
    {
        foreach (var val in distinctValues)
        {
            var isSelected = FilterState.SelectAll || (val != null && FilterState.SelectedValues.Contains(val));
            var display = val?.ToString() ?? DataFilter.Wpf.Resources.FilterResources.Blanks;
            var item = new FilterValueItem(display, val, null, isSelected);
            FilterValues.Add(item);
            
            if (val != null)
                FilterState.DistinctValues.Add(val);
        }
    }

    private void InitializeDateTree(IEnumerable<object> distinctValues)
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
                blankItem = new FilterValueItem(DataFilter.Wpf.Resources.FilterResources.Blanks, null, null, isSelected);
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
            FilterValues.Add(yearNode);
        }

        if (blankItem != null)
            FilterValues.Add(blankItem);
    }

    private void InitializeTimeTree(IEnumerable<object> distinctValues)
    {
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
                blankItem = new FilterValueItem(DataFilter.Wpf.Resources.FilterResources.Blanks, null, null, isSelected);
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
            FilterValues.Add(hourNode);
        }

        if (blankItem != null)
            FilterValues.Add(blankItem);
    }

    /// <summary>
    /// Loads an existing filter state into this view model.
    /// </summary>
    /// <param name="state">The state to copy.</param>
    public void LoadState(ExcelFilterState state)
    {
        _internalUpdate = true;
        
        FilterState.SearchText = state.SearchText;
        FilterState.UseWildcards = state.UseWildcards;
        FilterState.SelectAll = state.SelectAll;
        
        FilterState.SelectedValues.Clear();
        foreach (var val in state.SelectedValues)
            FilterState.SelectedValues.Add(val);
            
        FilterState.DistinctValues.Clear();
        foreach (var val in state.DistinctValues)
            FilterState.DistinctValues.Add(val);

        SelectedCustomOperator = state.CustomOperator;
        CustomValue1 = state.CustomValue1?.ToString() ?? string.Empty;
        CustomValue2 = state.CustomValue2?.ToString() ?? string.Empty;
        IsCustomFilterExpanded = state.CustomOperator != null;

        SearchText = FilterState.SearchText;
        SelectAll = FilterState.SelectAll;
        
        _internalUpdate = false;
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
        FilterState.SelectedValues.Clear();
        foreach (var item in FilterValues)
        {
            item.GetSelectedValues(FilterState.SelectedValues);
        }
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
