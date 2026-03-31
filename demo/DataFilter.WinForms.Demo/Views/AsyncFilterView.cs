using DataFilter.WinForms.Controls;
using DataFilter.WinForms.Demo.ViewModels;

namespace DataFilter.WinForms.Demo.Views;

public partial class AsyncFilterView : UserControl
{
    private readonly FilterableDataGrid _grid;

    public AsyncFilterView()
    {
        var title = new Label
        {
            Text = "Scenario 2 — Async Filtering",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font(Font.FontFamily, 11, FontStyle.Bold),
            Padding = new Padding(10, 5, 0, 0)
        };

        var banner = new Label
        {
            Text = "⏳  Async Data Loading Enabled — Fetches data from a mock API service",
            Dock = DockStyle.Top,
            Height = 35,
            BackColor = Color.AliceBlue,
            ForeColor = Color.RoyalBlue,
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

    public void Bind(AsyncFilterScenarioViewModel viewModel)
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
