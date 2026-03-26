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
        _selectAll.CheckedChanged += (_, _) =>
        {
            if (ViewModel != null) ViewModel.SelectAll = _selectAll.Checked;
        };
        _advanced.CheckedChanged += (_, _) => _advancedPanel.Visible = _advanced.Checked;

        _accumulationMode.Items.Add("Union");
        _accumulationMode.Items.Add("Intersection");
        _accumulationMode.SelectedIndex = 0;
        _accumulationMode.SelectedIndexChanged += (_, _) =>
        {
            if (ViewModel == null) return;
            ViewModel.AccumulationMode = _accumulationMode.SelectedIndex == 0
                ? DataFilter.Core.Enums.AccumulationMode.Union
                : DataFilter.Core.Enums.AccumulationMode.Intersection;
        };
        _values.AfterCheck += (_, e) =>
        {
            if (e.Node?.Tag is DataFilter.Filtering.ExcelLike.Models.FilterValueItem item)
            {
                item.IsSelected = e.Node.Checked;
            }
        };

        _operator.SelectedIndexChanged += (_, _) =>
        {
            if (ViewModel == null || _operator.SelectedItem is not DataFilter.Core.Enums.FilterOperator op) return;
            ViewModel.SelectedCustomOperator = op;
            _custom2.Visible = op == DataFilter.Core.Enums.FilterOperator.Between;
        };
        _custom1.TextChanged += (_, _) => { if (ViewModel != null) ViewModel.CustomValue1 = _custom1.Text; };
        _custom2.TextChanged += (_, _) => { if (ViewModel != null) ViewModel.CustomValue2 = _custom2.Text; };

        _sortAsc.Click += (_, _) => ViewModel?.SortAscendingCommand.Execute(null);
        _sortDesc.Click += (_, _) => ViewModel?.SortDescendingCommand.Execute(null);
        _addSortAsc.Click += (_, _) => ViewModel?.AddSubSortAscendingCommand.Execute(null);
        _addSortDesc.Click += (_, _) => ViewModel?.AddSubSortDescendingCommand.Execute(null);
    }

    public async Task BindAsync(ColumnFilterViewModel viewModel, IEnumerable<object> distinctValues)
    {
        ViewModel = viewModel;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        ViewModel.OnApply += (_, _) => RequestClose?.Invoke();
        ViewModel.OnClear += (_, _) => RequestClose?.Invoke();
        _operator.Items.Clear();
        foreach (var op in ViewModel.AvailableOperators) _operator.Items.Add(op);
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
}
