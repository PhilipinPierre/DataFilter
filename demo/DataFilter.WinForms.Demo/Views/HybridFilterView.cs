using DataFilter.WinForms.Controls;
using DataFilter.WinForms.Demo.ViewModels;

namespace DataFilter.WinForms.Demo.Views;

public partial class HybridFilterView : UserControl
{
    private readonly FilterableDataGrid _grid;

    public HybridFilterView()
    {
        var title = new Label
        {
            Text = "Scenario 3 — Hybrid Filtering",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font(Font.FontFamily, 11, FontStyle.Bold),
            Padding = new Padding(10, 5, 0, 0)
        };

        var banner = new Label
        {
            Text = "🔀  Hybrid mode — local results + async distinct values from API",
            Dock = DockStyle.Top,
            Height = 35,
            BackColor = Color.Honeydew,
            ForeColor = Color.DarkGreen,
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
    }

    public void Bind(HybridFilterScenarioViewModel viewModel)
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
    }
}
