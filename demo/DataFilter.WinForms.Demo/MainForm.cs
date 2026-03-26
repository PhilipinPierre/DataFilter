using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.WinForms.Controls;
using DataFilter.WinForms.ViewModels;

namespace DataFilter.WinForms.Demo;

public sealed class MainForm : Form
{
    private readonly FilterableDataGridViewModel<Employee> _viewModel;
    private readonly FilterableDataGrid _grid;

    public MainForm()
    {
        Text = "DataFilter WinForms Demo";
        Width = 1000;
        Height = 600;

        _viewModel = new FilterableDataGridViewModel<Employee>
        {
            LocalDataSource = EmployeeDataGenerator.Employees
        };
        _viewModel.RefreshDataAsync();

        _grid = new FilterableDataGrid
        {
            Dock = DockStyle.Fill,
            ViewModel = _viewModel,
            AutoGenerateColumns = true,
            DataSource = _viewModel.FilteredItems.ToList()
        };

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FilterableDataGridViewModel<Employee>.FilteredItems))
            {
                BeginInvoke(() => _grid.DataSource = _viewModel.FilteredItems.ToList());
            }
        };

        Controls.Add(_grid);
    }
}
