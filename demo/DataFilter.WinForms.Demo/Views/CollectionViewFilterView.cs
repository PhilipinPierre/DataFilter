using System.ComponentModel;
using DataFilter.WinForms.Controls;
using DataFilter.WinForms.Demo.ViewModels;
using DataFilter.Demo.Shared.Models;

namespace DataFilter.WinForms.Demo.Views;

public partial class CollectionViewFilterView : UserControl
{
    private readonly FilterableDataGrid _grid;

    public CollectionViewFilterView()
    {
        _grid = new FilterableDataGrid
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = true
        };

        Controls.Add(_grid);
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
    }
}
