using System.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.WinForms.Controls;
using DataFilter.WinForms.Demo.Services;
using DataFilter.WinForms.Demo.ViewModels;

namespace DataFilter.WinForms.Demo.Views;

public partial class CollectionViewFilterView : UserControl, IDemoHeaderSettingsView
{
    private readonly FilterableDataGrid _grid;

    public CollectionViewFilterView()
    {
        var title = new Label
        {
            Text = "Scenario 6 — Collection View Integration",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font(Font.FontFamily, 11, FontStyle.Bold),
            Padding = new Padding(10, 5, 0, 0)
        };

        var banner = new Label
        {
            Text = "📦  Integrated with WinForms BindingSource — demonstrating standard collection view abstraction.",
            Dock = DockStyle.Top,
            Height = 35,
            BackColor = Color.GhostWhite,
            ForeColor = Color.MediumSlateBlue,
            Font = new Font(Font.FontFamily, 9, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };

        _grid = new FilterableDataGrid
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = true
        };

        Controls.Add(_grid);
        Controls.Add(banner);
        Controls.Add(title);
    }

    public void Bind(CollectionViewScenarioViewModel viewModel)
    {
        _grid.ViewModel = viewModel.GridViewModel;
        
        // Use the BindingSource as the data source to demonstrate WinForms collection view binding
        _grid.DataSource = viewModel.BindingSource;
        
        viewModel.GridViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.GridViewModel.FilteredItems))
            {
                if (IsHandleCreated) BeginInvoke(() => _grid.DataSource = viewModel.GridViewModel.FilteredItems.ToList());
            }
        };

        ApplyHeaderSettings(viewModel.HeaderSettings);
    }

    public void ApplyHeaderSettings(DemoHeaderSettings settings)
    {
        _grid.AreColumnFiltersEnabled = settings.AreColumnFiltersEnabled;
        _grid.ColumnFilterTriggerMode = settings.ColumnFilterTriggerMode;
    }
}
