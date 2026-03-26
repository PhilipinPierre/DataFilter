using DataFilter.WinForms.Controls;
using DataFilter.WinForms.Demo.ViewModels;

namespace DataFilter.WinForms.Demo.Views;

public partial class LocalFilterView : UserControl
{
    private readonly FilterableDataGrid _grid;

    public LocalFilterView()
    {
        _grid = new FilterableDataGrid
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = true
        };
        Controls.Add(_grid);
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
    }
}
