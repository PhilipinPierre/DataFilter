using DataFilter.Demo.Shared.Services;
using DataFilter.WinForms.Controls;
using DataFilter.WinForms.Demo.Services;
using DataFilter.WinForms.Demo.ViewModels;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DataFilter.WinForms.Demo.Views;

public partial class LocalFilterView : UserControl, IDemoHeaderSettingsView
{
    private readonly FilterableDataGrid _grid;

    public LocalFilterView()
    {
        var title = new Label
        {
            Text = "Scenario 1 — Local Filtering",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font(Font.FontFamily, 11, FontStyle.Bold),
            Padding = new Padding(10, 5, 0, 0)
        };

        var banner = new Label
        {
            Text = "💻  All data is loaded locally. Filters are applied in-memory instantly against the collection.",
            Dock = DockStyle.Top,
            Height = 35,
            BackColor = Color.OldLace,
            ForeColor = Color.SaddleBrown,
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

    public void Bind(LocalFilterScenarioViewModel viewModel)
    {
        _grid.ViewModel = viewModel.GridViewModel;
        viewModel.GridViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.GridViewModel.FilteredItems))
            {
                if (IsHandleCreated) BeginInvoke(() => _grid.DataSource = viewModel.GridViewModel.FilteredItems.ToList());
            }
        };
        _grid.DataSource = viewModel.GridViewModel.FilteredItems.ToList();
        ApplyHeaderSettings(viewModel.HeaderSettings);
    }

    public void ApplyHeaderSettings(DemoHeaderSettings settings)
    {
        _grid.AreColumnFiltersEnabled = settings.AreColumnFiltersEnabled;
        _grid.ColumnFilterTriggerMode = settings.ColumnFilterTriggerMode;
    }
}
