using System.ComponentModel;
using DataFilter.WinForms.Controls;
using DataFilter.WinForms.Demo.ViewModels;
using DataFilter.Demo.Shared.Models;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.WinForms.Demo.Views;

public partial class ListViewFilterView : UserControl
{
    private readonly ListView _listView;
    private ListViewScenarioViewModel? _viewModel;

    public ListViewFilterView()
    {
        var title = new Label
        {
            Text = "Scenario 5 — Native ListView Integration",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font(Font.FontFamily, 11, FontStyle.Bold),
            Padding = new Padding(10, 5, 0, 0)
        };

        var banner = new Label
        {
            Text = "📋  Demonstrating DataFilter integration with a standard WinForms ListView control.",
            Dock = DockStyle.Top,
            Height = 35,
            BackColor = Color.OldLace,
            ForeColor = Color.SaddleBrown,
            Font = new Font(Font.FontFamily, 9, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };

        _listView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            MultiSelect = false
        };

        _listView.Columns.Add("Id 🔍", 60);
        _listView.Columns.Add("Name 🔍", 150);
        _listView.Columns.Add("Department 🔍", 150);
        _listView.Columns.Add("Country 🔍", 120);

        _listView.ColumnClick += OnColumnClick;

        Controls.Add(_listView);
        Controls.Add(banner);
        Controls.Add(title);
    }

    private void OnColumnClick(object? sender, ColumnClickEventArgs e)
    {
        if (_viewModel == null) return;

        string propertyName = e.Column switch
        {
            0 => "Id",
            1 => "Name",
            2 => "Department",
            3 => "Country",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(propertyName)) return;

        ShowFilterPopup(propertyName, _listView.PointToScreen(_listView.GetItemRect(0).Location));
    }

    private async void ShowFilterPopup(string propertyName, Point location)
    {
        if (_viewModel == null) return;

        var popup = new FilterPopupControl();
        var distinctValues = await _viewModel.GridViewModel.GetDistinctValuesAsync(propertyName, string.Empty);
        
        var vm = new ColumnFilterViewModel(
            search => _viewModel.GridViewModel.GetDistinctValuesAsync(propertyName, search),
            state => _viewModel.GridViewModel.ApplyColumnFilter(propertyName, state),
            () => _viewModel.GridViewModel.ClearColumnFilter(propertyName),
            isDesc => _viewModel.GridViewModel.ApplySort(propertyName, isDesc),
            isDesc => _viewModel.GridViewModel.AddSubSort(propertyName, isDesc),
            _viewModel.GridViewModel.GetPropertyType(propertyName));

        await popup.BindAsync(vm, distinctValues);

        var form = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.Manual,
            Width = 250,
            Height = 400,
            Location = Cursor.Position, // Fallback to cursor pos for simplicity in demo
            ShowInTaskbar = false
        };
        form.Controls.Add(popup);
        popup.Dock = DockStyle.Fill;
        popup.RequestClose += () => form.Close();
        form.Deactivate += (s, e) => form.Close();
        form.Show();
    }

    public void Bind(ListViewScenarioViewModel viewModel)
    {
        _viewModel = viewModel;
        
        viewModel.GridViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.GridViewModel.FilteredItems))
            {
                if (IsHandleCreated) BeginInvoke(UpdateList);
            }
        };
        UpdateList();
    }

    private void UpdateList()
    {
        if (_viewModel == null) return;
        _listView.Items.Clear();
        foreach (var item in _viewModel.GridViewModel.FilteredItems)
        {
            var lvi = new ListViewItem(item.Id.ToString());
            lvi.SubItems.Add(item.Name);
            lvi.SubItems.Add(item.Department);
            lvi.SubItems.Add(item.Country);
            _listView.Items.Add(lvi);
        }
    }
}
