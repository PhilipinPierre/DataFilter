using DataFilter.Core.Enums;
using DataFilter.Localization;
using DataFilter.PlatformShared.ViewModels;
using System.ComponentModel;
using System.Windows.Forms;

namespace DataFilter.WinForms.Controls;

public sealed class FilterPopupControl : UserControl
{
    private readonly TextBox _search = new() { Dock = DockStyle.Top, PlaceholderText = "Search..." };
    private readonly CheckBox _addToExisting = new() { Dock = DockStyle.Top, Text = "Add selection to filter" };
    private readonly ComboBox _accumulationMode = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
    private readonly CheckBox _selectAll = new() { Dock = DockStyle.Top, Text = "Select All", ThreeState = false, Checked = true };
    private readonly Label _loading = new() { Dock = DockStyle.Top, Text = "Loading...", TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Visible = false };
    private readonly TreeView _values = new() { Dock = DockStyle.Fill, CheckBoxes = true };
    private readonly ComboBox _operator = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _custom1 = new() { Dock = DockStyle.Top, PlaceholderText = "Value" };
    private readonly TextBox _custom2 = new() { Dock = DockStyle.Top, PlaceholderText = "To", Visible = false };
    private readonly CheckBox _advanced = new() { Dock = DockStyle.Top, Text = "Advanced Filter" };
    private readonly Button _sortAsc = new() { Text = "Sort A to Z", Dock = DockStyle.Top, Height = 24 };
    private readonly Button _sortDesc = new() { Text = "Sort Z to A", Dock = DockStyle.Top, Height = 24 };
    private readonly Button _addSortAsc = new() { Text = "Add Sort A to Z", Dock = DockStyle.Top, Height = 24 };
    private readonly Button _addSortDesc = new() { Text = "Add Sort Z to A", Dock = DockStyle.Top, Height = 24 };
    private readonly Button _ok = new() { Text = "OK", Dock = DockStyle.Left, Width = 90 };
    private readonly Button _clear = new() { Text = "Clear", Dock = DockStyle.Right, Width = 90 };
    private readonly Panel _advancedPanel = new() { Dock = DockStyle.Top, Height = 92, Visible = false };

    public ColumnFilterViewModel? ViewModel { get; private set; }
    public event Action? RequestClose;

    public FilterPopupControl()
    {
        var sortPanel = new Panel { Dock = DockStyle.Top, Height = 100 };
        sortPanel.Controls.Add(_addSortDesc);
        sortPanel.Controls.Add(_addSortAsc);
        sortPanel.Controls.Add(_sortDesc);
        sortPanel.Controls.Add(_sortAsc);

        _advancedPanel.Controls.Add(_custom2);
        _advancedPanel.Controls.Add(_custom1);
        _advancedPanel.Controls.Add(_operator);

        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 38 };
        buttons.Controls.Add(_ok);
        buttons.Controls.Add(_clear);

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
                _custom2.Visible = op == FilterOperator.Between;
            }
        };
        _custom1.TextChanged += (_, _) => { if (ViewModel != null) ViewModel.CustomValue1 = _custom1.Text; };
        _custom2.TextChanged += (_, _) => { if (ViewModel != null) ViewModel.CustomValue2 = _custom2.Text; };

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
        await ViewModel.InitializeAsync(distinctValues);
        ReloadTree();
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
        _clear.Text = LocalizationManager.Instance["Clear"];
        _custom1.PlaceholderText = LocalizationManager.Instance["ValueText"];
        _custom2.PlaceholderText = LocalizationManager.Instance["ToText"];

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

    public void ApplyTheme(bool isDark)
    {
        this.BackColor = isDark ? Color.FromArgb(45, 45, 48) : SystemColors.Control;
        this.ForeColor = isDark ? Color.White : SystemColors.ControlText;
        UpdateControlTheme(this, isDark);
    }

    private void UpdateControlTheme(Control parent, bool isDark)
    {
        foreach (Control c in parent.Controls)
        {
            c.BackColor = isDark ? Color.FromArgb(45, 45, 48) : SystemColors.Window;
            c.ForeColor = isDark ? Color.White : SystemColors.ControlText;
            if (c is Button b) b.FlatStyle = FlatStyle.Flat;
            UpdateControlTheme(c, isDark);
        }
    }

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
