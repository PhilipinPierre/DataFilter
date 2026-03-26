using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataFilter.Core.Engine;
using DataFilter.Core.Enums;
using DataFilter.Filtering.ExcelLike.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DataFilter.PlatformShared.ViewModels;

public partial class ColumnFilterViewModel : ObservableObject
{
    private readonly IFilterEvaluator _filterEvaluator;
    private readonly Func<string, Task<IEnumerable<object>>> _distinctValuesProvider;
    private readonly Action<ExcelFilterState> _onApply;
    private readonly Action _onClear;
    private readonly Action<bool>? _onSort;
    private readonly Action<bool>? _onAddSubSort;

    public ExcelFilterState FilterState { get; } = new();
    public ObservableCollection<FilterValueItem> FilterValues { get; } = new();
    public ObservableCollection<FilterOperator> AvailableOperators { get; } = new();
    public FilterDataType DataType { get; }

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool? _selectAll = true;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _addToExistingFilter;
    [ObservableProperty] private AccumulationMode _accumulationMode = AccumulationMode.Union;
    [ObservableProperty] private FilterOperator? _selectedCustomOperator;
    [ObservableProperty] private string _customValue1 = string.Empty;
    [ObservableProperty] private string _customValue2 = string.Empty;
    [ObservableProperty] private bool _isCustomFilterExpanded;

    public bool IsFilterActive => !FilterState.SelectAll || FilterState.CustomOperator != null || !string.IsNullOrEmpty(FilterState.SearchText);

    public ICommand ApplyCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand SortAscendingCommand { get; }
    public ICommand SortDescendingCommand { get; }
    public ICommand AddSubSortAscendingCommand { get; }
    public ICommand AddSubSortDescendingCommand { get; }
    public IAsyncRelayCommand<string> SearchCommand { get; }

    public event EventHandler? OnApply;
    public event EventHandler? OnClear;

    partial void OnSelectAllChanged(bool? value)
    {
        if (!value.HasValue) return;
        foreach (var item in FilterValues)
        {
            item.IsSelected = value.Value;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = SearchCommand.ExecuteAsync(value);
    }

    public ColumnFilterViewModel(
        Func<string, Task<IEnumerable<object>>> distinctValuesProvider,
        Action<ExcelFilterState> onApply,
        Action onClear,
        Action<bool>? onSort = null,
        Action<bool>? onAddSubSort = null,
        Type? propertyType = null,
        IFilterEvaluator? filterEvaluator = null)
    {
        _distinctValuesProvider = distinctValuesProvider;
        _onApply = onApply;
        _onClear = onClear;
        _onSort = onSort;
        _onAddSubSort = onAddSubSort;
        _filterEvaluator = filterEvaluator ?? new FilterEvaluator();
        DataType = ResolveType(propertyType);

        ApplyCommand = new RelayCommand(ApplyFilter);
        ClearCommand = new RelayCommand(ClearFilter);
        SortAscendingCommand = new RelayCommand(() => { _onSort?.Invoke(false); OnApply?.Invoke(this, EventArgs.Empty); });
        SortDescendingCommand = new RelayCommand(() => { _onSort?.Invoke(true); OnApply?.Invoke(this, EventArgs.Empty); });
        AddSubSortAscendingCommand = new RelayCommand(() => { _onAddSubSort?.Invoke(false); OnApply?.Invoke(this, EventArgs.Empty); });
        AddSubSortDescendingCommand = new RelayCommand(() => { _onAddSubSort?.Invoke(true); OnApply?.Invoke(this, EventArgs.Empty); });
        SearchCommand = new AsyncRelayCommand<string>(RefreshDistinctValuesAsync);
        InitializeOperators();
    }

    public async Task InitializeAsync(IEnumerable<object> distinctValues)
    {
        FilterValues.Clear();
        if (DataType == FilterDataType.Date)
        {
            BuildDateTree(distinctValues);
        }
        else
        {
            foreach (var val in distinctValues)
            {
                var text = val?.ToString() ?? "(Blanks)";
                var isSelected = FilterState.SelectAll || FilterState.SelectedValues.Contains(val!);
                FilterValues.Add(new FilterValueItem(text, val, null, isSelected));
            }
        }
        await Task.CompletedTask;
    }

    public async Task LoadStateAsync(ExcelFilterState state)
    {
        FilterState.SearchText = state.SearchText;
        FilterState.SelectAll = state.SelectAll;
        FilterState.SelectedValues.Clear();
        foreach (var v in state.SelectedValues) FilterState.SelectedValues.Add(v);
        FilterState.CustomOperator = state.CustomOperator;
        FilterState.CustomValue1 = state.CustomValue1;
        FilterState.CustomValue2 = state.CustomValue2;

        SearchText = state.SearchText;
        SelectedCustomOperator = state.CustomOperator;
        CustomValue1 = state.CustomValue1?.ToString() ?? string.Empty;
        CustomValue2 = state.CustomValue2?.ToString() ?? string.Empty;
        await RefreshDistinctValuesAsync(SearchText);
    }

    private async Task RefreshDistinctValuesAsync(string? text)
    {
        IsLoading = true;
        try
        {
            var distincts = await _distinctValuesProvider(text ?? string.Empty);
            await InitializeAsync(distincts);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        var selected = new HashSet<object>();
        foreach (var item in FilterValues)
        {
            CollectSelectedValues(item, selected);
        }

        if (!AddToExistingFilter)
        {
            FilterState.SelectedValues.Clear();
            foreach (var s in selected) FilterState.SelectedValues.Add(s);
        }
        else if (AccumulationMode == AccumulationMode.Intersection)
        {
            FilterState.SelectedValues.IntersectWith(selected);
        }
        else
        {
            foreach (var s in selected) FilterState.SelectedValues.Add(s);
        }

        FilterState.SelectAll = string.IsNullOrEmpty(SearchText) && SelectAll == true;
        FilterState.SearchText = string.Empty;
        FilterState.CustomOperator = SelectedCustomOperator;
        FilterState.CustomValue1 = string.IsNullOrWhiteSpace(CustomValue1) ? null : CustomValue1;
        FilterState.CustomValue2 = string.IsNullOrWhiteSpace(CustomValue2) ? null : CustomValue2;

        OnPropertyChanged(nameof(IsFilterActive));
        _onApply(FilterState);
        OnApply?.Invoke(this, EventArgs.Empty);
    }

    private void ClearFilter()
    {
        FilterState.Clear();
        SearchText = string.Empty;
        SelectAll = true;
        SelectedCustomOperator = null;
        CustomValue1 = string.Empty;
        CustomValue2 = string.Empty;
        IsCustomFilterExpanded = false;
        OnPropertyChanged(nameof(IsFilterActive));
        _onClear();
        OnClear?.Invoke(this, EventArgs.Empty);
    }

    private static void CollectSelectedValues(FilterValueItem item, HashSet<object> selected)
    {
        if (item.Children.Count == 0)
        {
            if (item.IsSelected == true)
            {
                selected.Add(item.Value!);
            }
            return;
        }

        foreach (var child in item.Children)
        {
            CollectSelectedValues(child, selected);
        }
    }

    private void BuildDateTree(IEnumerable<object> distinctValues)
    {
        var dates = new List<DateTime>();
        var hasBlank = false;
        foreach (var value in distinctValues)
        {
            if (value is DateTime dt) dates.Add(dt);
            else if (value is DateTimeOffset dto) dates.Add(dto.DateTime);
            else if (value == null) hasBlank = true;
        }

        foreach (var yearGroup in dates.GroupBy(x => x.Year).OrderBy(x => x.Key))
        {
            var yearNode = new FilterValueItem(yearGroup.Key.ToString(), null, null, false);
            foreach (var monthGroup in yearGroup.GroupBy(x => x.Month).OrderBy(x => x.Key))
            {
                var monthName = new DateTime(2000, monthGroup.Key, 1).ToString("MMMM");
                var monthNode = new FilterValueItem(monthName, null, yearNode, false);
                foreach (var day in monthGroup.OrderBy(x => x.Day))
                {
                    var selected = FilterState.SelectAll || FilterState.SelectedValues.Contains(day);
                    monthNode.AddChild(new FilterValueItem(day.Day.ToString("D2"), day, monthNode, selected));
                }
                monthNode.UpdateStateFromChildren();
                yearNode.AddChild(monthNode);
            }
            yearNode.UpdateStateFromChildren();
            FilterValues.Add(yearNode);
        }

        if (hasBlank)
        {
            var selected = FilterState.SelectAll || FilterState.SelectedValues.Contains(null!);
            FilterValues.Add(new FilterValueItem("(Blanks)", null, null, selected));
        }
    }

    private void InitializeOperators()
    {
        var ops = DataType == FilterDataType.Text
            ? new[] { FilterOperator.Equals, FilterOperator.NotEquals, FilterOperator.Contains, FilterOperator.NotContains, FilterOperator.StartsWith, FilterOperator.EndsWith }
            : new[] { FilterOperator.Equals, FilterOperator.NotEquals, FilterOperator.GreaterThan, FilterOperator.GreaterThanOrEqual, FilterOperator.LessThan, FilterOperator.LessThanOrEqual, FilterOperator.Between };
        foreach (var op in ops) AvailableOperators.Add(op);
    }

    private static FilterDataType ResolveType(Type? type)
    {
        var t = Nullable.GetUnderlyingType(type ?? typeof(string)) ?? type ?? typeof(string);
        if (t == typeof(string)) return FilterDataType.Text;
        if (t == typeof(DateTime)) return FilterDataType.Date;
        if (t == typeof(TimeSpan)) return FilterDataType.Time;
        if (t == typeof(bool)) return FilterDataType.Boolean;
        if (t.IsPrimitive || t == typeof(decimal)) return FilterDataType.Number;
        return FilterDataType.Text;
    }
}
