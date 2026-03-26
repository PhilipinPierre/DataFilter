using System.ComponentModel;
using DataFilter.WinForms.Controls;
using DataFilter.WinForms.Demo.ViewModels;
using DataFilter.Demo.Shared.Models;

namespace DataFilter.WinForms.Demo.Views;

public partial class ListViewFilterView : UserControl
{
    private readonly FilterableDataGrid _grid;

    public ListViewFilterView()
    {
        // To showcase "ListView" in WinForms, we use DataGridView but explicitly define specific columns only.
        _grid = new FilterableDataGrid
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true
        };

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "Id", Width = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "Name", Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Department", HeaderText = "Department", Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Country", HeaderText = "Country", Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Time", HeaderText = "Time", Width = 150 });

        Controls.Add(_grid);
    }

    public void Bind(ListViewScenarioViewModel viewModel)
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
