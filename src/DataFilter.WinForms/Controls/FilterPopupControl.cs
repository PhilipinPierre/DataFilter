using DataFilter.Core.Enums;
using DataFilter.Localization;
using DataFilter.PlatformShared.FilterValues;
using DataFilter.PlatformShared.Theming;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.Theming;
using System.ComponentModel;
using System.Windows.Forms;

namespace DataFilter.WinForms.Controls;

public sealed class FilterPopupControl : UserControl
{
    private readonly TextBox _search = new() { Dock = DockStyle.Top, PlaceholderText = "Search..." };
    private readonly CheckBox _addToExisting = new() { Dock = DockStyle.Top, Text = "Add selection to filter" };
    private readonly ComboBox _accumulationMode = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
    private readonly CheckBox _selectAll = new() { Dock = DockStyle.Top, Text = "Select All", ThreeState = false, Checked = true, AccessibleName = "df-select-all" };
    private readonly Label _loading = new() { Dock = DockStyle.Top, Text = "Loading...", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Visible = false };
    private readonly TreeView _values = new() { Dock = DockStyle.Fill, CheckBoxes = true, AccessibleName = "df-values-tree" };
    private readonly ComboBox _operator = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _custom1Text = new() { Dock = DockStyle.Top, PlaceholderText = "Value" };
    private readonly DateTimePicker _custom1Date = new() { Dock = DockStyle.Top, Format = DateTimePickerFormat.Custom, CustomFormat = FilterCustomValueFormats.DateFormat, Visible = false };
    private readonly DateTimePicker _custom1Time = new() { Dock = DockStyle.Top, Format = DateTimePickerFormat.Time, ShowUpDown = true, Visible = false };
    private readonly TextBox _custom2Text = new() { Dock = DockStyle.Top, PlaceholderText = "To", Visible = false };
    private readonly DateTimePicker _custom2Date = new() { Dock = DockStyle.Top, Format = DateTimePickerFormat.Custom, CustomFormat = FilterCustomValueFormats.DateFormat, Visible = false };
    private readonly DateTimePicker _custom2Time = new() { Dock = DockStyle.Top, Format = DateTimePickerFormat.Time, ShowUpDown = true, Visible = false };
    private readonly CheckBox _advanced = new() { Dock = DockStyle.Top, Text = "Advanced Filter" };
    private readonly Button _sortAsc = new() { Text = "Sort A to Z", Dock = DockStyle.Top, Height = 24 };
    private readonly Button _sortDesc = new() { Text = "Sort Z to A", Dock = DockStyle.Top, Height = 24 };
    private readonly Button _addSortAsc = new() { Text = "Add Sort A to Z", Dock = DockStyle.Top, Height = 24 };
    private readonly Button _addSortDesc = new() { Text = "Add Sort Z to A", Dock = DockStyle.Top, Height = 24 };
    private readonly Button _ok = new() { Text = "OK", Dock = DockStyle.Left, Width = 90, AccessibleName = "df-ok" };
    private readonly Button _cancel = new() { Text = "Cancel", Dock = DockStyle.Right, Width = 90, AccessibleName = "df-cancel" };
    private readonly Button _clear = new() { Text = "Clear", Dock = DockStyle.Top, Height = 24, AccessibleName = "df-clear" };
    private readonly Panel _advancedPanel = new() { Dock = DockStyle.Top, Height = 92, Visible = false };

    private bool _syncingCustomValues;
    public ColumnFilterViewModel? ViewModel { get; private set; }
    public event Action? RequestClose;

    public FilterPopupControl()
    {
        var sortPanel = new Panel { Dock = DockStyle.Top, Height = 124 };
        sortPanel.Controls.Add(_clear);
        sortPanel.Controls.Add(_addSortDesc);
        sortPanel.Controls.Add(_addSortAsc);
        sortPanel.Controls.Add(_sortDesc);
        sortPanel.Controls.Add(_sortAsc);

        _advancedPanel.Controls.Add(_custom2Time);
        _advancedPanel.Controls.Add(_custom2Date);
        _advancedPanel.Controls.Add(_custom2Text);
        _advancedPanel.Controls.Add(_custom1Time);
        _advancedPanel.Controls.Add(_custom1Date);
        _advancedPanel.Controls.Add(_custom1Text);
        _advancedPanel.Controls.Add(_operator);

        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 38 };
        buttons.Controls.Add(_ok);
        buttons.Controls.Add(_cancel);

        Controls.Add(_values);
        Controls.Add(_loading);
        Controls.Add(_selectAll);
        Controls.Add(_advancedPanel);
        Controls.Add(_advanced);
        Controls.Add(_accumulationMode);
        Controls.Add(_addToExisting);
        Controls.Add(_search);
        Controls.Add(sortPanel);
        Controls.Add(buttons);
        BorderStyle = BorderStyle.FixedSingle;

        _search.TextChanged += async (_, _) =>
        {
            if (ViewModel != null) await ViewModel.SearchCommand.ExecuteAsync(_search.Text);
        };
        _addToExisting.CheckedChanged += (_, _) =>
        {
            if (ViewModel != null) ViewModel.AddToExistingFilter = _addToExisting.Checked;
            _accumulationMode.Visible = _addToExisting.Checked;
        };
        _ok.Click += (_, _) => ViewModel?.ApplyCommand.Execute(null);
        _cancel.Click += (_, _) => RequestClose?.Invoke();
        _clear.Click += (_, _) => ViewModel?.ClearCommand.Execute(null);
        _selectAll.CheckedChanged += (_, _) => OnSelectAllChanged(_selectAll.Checked);
        _advanced.CheckedChanged += (_, _) => _advancedPanel.Visible = _advanced.Checked;

        _accumulationMode.DisplayMember = nameof(LocalizedItem.Text);
        _accumulationMode.ValueMember = nameof(LocalizedItem.Value);
        _accumulationMode.Items.Add(new LocalizedItem(AccumulationMode.Union, LocalizationManager.Instance["ModeUnion"]));
        _accumulationMode.Items.Add(new LocalizedItem(AccumulationMode.Intersection, LocalizationManager.Instance["ModeIntersection"]));
        _accumulationMode.SelectedIndex = 0;
        _accumulationMode.SelectedIndexChanged += (_, _) =>
        {
            if (ViewModel == null) return;
            if (_accumulationMode.SelectedItem is LocalizedItem { Value: AccumulationMode mode })
                ViewModel.AccumulationMode = mode;
        };
        _values.AfterCheck += (s, e) =>
        {
            if (e.Action != TreeViewAction.ByMouse && e.Action != TreeViewAction.ByKeyboard) return;
            if (e.Node?.Tag is DataFilter.Filtering.ExcelLike.Models.FilterValueItem item)
            {
                item.IsSelected = e.Node.Checked;
                // If it has children, check them too
                foreach (TreeNode child in e.Node.Nodes) SetCheckedRecursive(child, e.Node.Checked);
            }
        };

        _operator.SelectedIndexChanged += (_, _) =>
        {
            if (ViewModel == null) return;
            if (_operator.SelectedItem is LocalizedItem { Value: FilterOperator op })
            {
                ViewModel.SelectedCustomOperator = op;
                UpdateCustomValue2Visibility(op == FilterOperator.Between);
            }
        };
        _custom1Text.TextChanged += (_, _) => { if (ViewModel != null && !_syncingCustomValues) ViewModel.CustomValue1 = _custom1Text.Text; };
        _custom2Text.TextChanged += (_, _) => { if (ViewModel != null && !_syncingCustomValues) ViewModel.CustomValue2 = _custom2Text.Text; };
        _custom1Date.ValueChanged += (_, _) => SyncDatePickerToViewModel(_custom1Date, v => ViewModel!.CustomValue1 = v);
        _custom2Date.ValueChanged += (_, _) => SyncDatePickerToViewModel(_custom2Date, v => ViewModel!.CustomValue2 = v);
        _custom1Time.ValueChanged += (_, _) => SyncTimePickerToViewModel(_custom1Time, v => ViewModel!.CustomValue1 = v);
        _custom2Time.ValueChanged += (_, _) => SyncTimePickerToViewModel(_custom2Time, v => ViewModel!.CustomValue2 = v);

        _sortAsc.Click += (_, _) => ViewModel?.SortAscendingCommand.Execute(null);
        _sortDesc.Click += (_, _) => ViewModel?.SortDescendingCommand.Execute(null);
        _addSortAsc.Click += (_, _) => ViewModel?.AddSubSortAscendingCommand.Execute(null);
        _addSortDesc.Click += (_, _) => ViewModel?.AddSubSortDescendingCommand.Execute(null);

        LocalizationManager.Instance.CultureChanged += (_, _) =>
        {
            if (IsDisposed) return;
            if (InvokeRequired)
                BeginInvoke(new Action(ApplyLocalization));
            else
                ApplyLocalization();
        };

        ApplyLocalization();
    }

    public async Task BindAsync(ColumnFilterViewModel viewModel, IEnumerable<object> distinctValues)
    {
        ViewModel = viewModel;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        ViewModel.OnApply += (_, _) => RequestClose?.Invoke();
        ViewModel.OnClear += (_, _) => RequestClose?.Invoke();
        _operator.Items.Clear();
        _operator.DisplayMember = nameof(LocalizedItem.Text);
        _operator.ValueMember = nameof(LocalizedItem.Value);
        foreach (var op in ViewModel.AvailableOperators)
            _operator.Items.Add(new LocalizedItem(op, LocalizationManager.Instance[$"FilterOperator_{op}"]));
        ConfigureValueEditors(ViewModel.DataType);
        await ViewModel.InitializeAsync(distinctValues);
        SyncCustomValuesFromViewModel();
        ReloadTree();
    }

    private void ConfigureValueEditors(FilterDataType dataType)
    {
        _custom1Text.Visible = dataType is not FilterDataType.Date and not FilterDataType.Time;
        _custom1Date.Visible = dataType == FilterDataType.Date;
        _custom1Time.Visible = dataType == FilterDataType.Time;
        _custom2Text.Visible = false;
        _custom2Date.Visible = false;
        _custom2Time.Visible = false;
    }

    private void UpdateCustomValue2Visibility(bool visible)
    {
        if (ViewModel == null) return;
        switch (ViewModel.DataType)
        {
            case FilterDataType.Date:
                _custom2Date.Visible = visible;
                break;
            case FilterDataType.Time:
                _custom2Time.Visible = visible;
                break;
            default:
                _custom2Text.Visible = visible;
                break;
        }
    }

    private void SyncCustomValuesFromViewModel()
    {
        if (ViewModel == null) return;

        _syncingCustomValues = true;
        try
        {
            _custom1Text.Text = ViewModel.CustomValue1;
            _custom2Text.Text = ViewModel.CustomValue2;

            if (FilterCustomValueFormats.TryParseDate(ViewModel.CustomValue1, out var date1))
                _custom1Date.Value = date1;
            if (FilterCustomValueFormats.TryParseDate(ViewModel.CustomValue2, out var date2))
                _custom2Date.Value = date2;
            if (FilterCustomValueFormats.TryParseTime(ViewModel.CustomValue1, out var time1))
                _custom1Time.Value = DateTime.Today.Add(time1);
            if (FilterCustomValueFormats.TryParseTime(ViewModel.CustomValue2, out var time2))
                _custom2Time.Value = DateTime.Today.Add(time2);

            UpdateCustomValue2Visibility(ViewModel.SelectedCustomOperator == FilterOperator.Between);
        }
        finally
        {
            _syncingCustomValues = false;
        }
    }

    private static void SyncDatePickerToViewModel(DateTimePicker picker, Action<string> assign)
    {
        assign(FilterCustomValueFormats.FormatDate(picker.Value.Date));
    }

    private static void SyncTimePickerToViewModel(DateTimePicker picker, Action<string> assign)
    {
        assign(FilterCustomValueFormats.FormatTime(picker.Value.TimeOfDay));
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (ViewModel == null) return;
        if (e.PropertyName == nameof(ColumnFilterViewModel.FilterValues))
        {
            ReloadTree();
        }
        else if (e.PropertyName == nameof(ColumnFilterViewModel.IsLoading))
        {
            _loading.Visible = ViewModel.IsLoading;
            _values.Visible = !ViewModel.IsLoading;
        }
        else if (e.PropertyName is nameof(ColumnFilterViewModel.CustomValue1) or nameof(ColumnFilterViewModel.CustomValue2))
        {
            SyncCustomValuesFromViewModel();
        }
    }

    private void ReloadTree()
    {
        if (ViewModel == null) return;
        _values.Nodes.Clear();
        foreach (var item in ViewModel.FilterValues)
        {
            _values.Nodes.Add(BuildNode(item));
        }
    }

    private static TreeNode BuildNode(DataFilter.Filtering.ExcelLike.Models.FilterValueItem item)
    {
        var node = new TreeNode(item.DisplayText) { Checked = item.IsSelected == true, Tag = item };
        foreach (var child in item.Children)
        {
            node.Nodes.Add(BuildNode(child));
        }
        return node;
    }

    private void ApplyLocalization()
    {
        _search.PlaceholderText = LocalizationManager.Instance["SearchPlaceholder"];
        _addToExisting.Text = LocalizationManager.Instance["AddToFilter"];
        _selectAll.Text = LocalizationManager.Instance["SelectAll"];
        _loading.Text = LocalizationManager.Instance["LoadingText"];
        _advanced.Text = LocalizationManager.Instance["AdvancedFilter"];
        _sortAsc.Text = LocalizationManager.Instance["SortAscending"];
        _sortDesc.Text = LocalizationManager.Instance["SortDescending"];
        _addSortAsc.Text = LocalizationManager.Instance["AddSubSortAscending"];
        _addSortDesc.Text = LocalizationManager.Instance["AddSubSortDescending"];
        _ok.Text = LocalizationManager.Instance["Ok"];
        _cancel.Text = LocalizationManager.Instance["Cancel"];
        _clear.Text = LocalizationManager.Instance["Clear"];
        _custom1Text.PlaceholderText = LocalizationManager.Instance["ValueText"];
        _custom2Text.PlaceholderText = LocalizationManager.Instance["ToText"];

        for (int i = 0; i < _accumulationMode.Items.Count; i++)
        {
            if (_accumulationMode.Items[i] is LocalizedItem { Value: AccumulationMode mode } item)
                item.Text = mode == AccumulationMode.Union ? LocalizationManager.Instance["ModeUnion"] : LocalizationManager.Instance["ModeIntersection"];
        }
        _accumulationMode.Refresh();

        for (int i = 0; i < _operator.Items.Count; i++)
        {
            if (_operator.Items[i] is LocalizedItem { Value: FilterOperator op } item)
                item.Text = LocalizationManager.Instance[$"FilterOperator_{op}"];
        }
        _operator.Refresh();

        ReloadTree();
    }

    private sealed class LocalizedItem
    {
        public LocalizedItem(object value, string text)
        {
            Value = value;
            Text = text;
        }

        public object Value { get; }
        public string Text { get; set; }
        public override string ToString() => Text;
    }

    /// <summary>Applies <paramref name="theme"/> (or <see cref="FilterTheme.Current"/> when null).</summary>
    public void ApplyTheme(FilterTheme? theme = null)
    {
        theme ??= FilterTheme.Current;
        BackColor = FilterThemeApplier.ToDrawingColor(theme.PopupBackground);
        ForeColor = FilterThemeApplier.ToDrawingColor(theme.PopupForeground);

        var surface = FilterThemeApplier.ToDrawingColor(theme.SecondaryBackground);
        var border = FilterThemeApplier.ToDrawingColor(theme.SecondaryBorder);
        var primary = FilterThemeApplier.ToDrawingColor(theme.PrimaryColor);
        var primaryFg = FilterThemeApplier.ToDrawingColor(theme.PrimaryButtonForeground);

        foreach (var input in new Control[] { _search, _custom1Text, _custom2Text, _values, _operator, _accumulationMode, _custom1Date, _custom2Date, _custom1Time, _custom2Time })
        {
            input.BackColor = surface;
            input.ForeColor = ForeColor;
        }

        foreach (var btn in new Button[] { _sortAsc, _sortDesc, _addSortAsc, _addSortDesc, _clear, _cancel })
        {
            btn.BackColor = BackColor;
            btn.ForeColor = ForeColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = border;
        }

        _ok.BackColor = primary;
        _ok.ForeColor = primaryFg;
        _ok.FlatStyle = FlatStyle.Flat;
        _ok.FlatAppearance.BorderColor = border;
    }

    /// <summary>Applies built-in <see cref="FilterTheme.Dark"/> or <see cref="FilterTheme.Light"/>.</summary>
    public void ApplyTheme(bool isDark) => ApplyTheme(isDark ? FilterTheme.Dark : FilterTheme.Light);

    private void OnSelectAllChanged(bool checkedState)
    {
        if (ViewModel == null) return;
        ViewModel.SelectAll = checkedState;
        foreach (TreeNode node in _values.Nodes)
        {
            SetCheckedRecursive(node, checkedState);
        }
    }

    private void SetCheckedRecursive(TreeNode node, bool checkedState)
    {
        node.Checked = checkedState;
        foreach (TreeNode child in node.Nodes)
        {
            SetCheckedRecursive(child, checkedState);
        }
    }
}
