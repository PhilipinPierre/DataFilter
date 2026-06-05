using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Services;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels;

public partial class LocalFilterScenarioViewModel : ObservableObject, IDemoItem
{
    private const string DefaultPipelineJson =
        """
        {
          "schemaVersion": 1,
          "rootCombineOperator": "And",
          "nodes": [
            {
              "kind": "criterion",
              "id": "demo-active",
              "isEnabled": true,
              "propertyName": "IsActive",
              "operator": "Equals",
              "value": true
            }
          ]
        }
        """;

    [ObservableProperty]
    private IFilterableDataGridViewModel<Employee> _gridViewModel;

    [ObservableProperty]
    private IEnumerable<Employee> _employees;

    [ObservableProperty]
    private string _pipelineJson = DefaultPipelineJson;

    /// <summary>Mutable in-memory snapshot (criteria + sort) — edit lists directly, then apply.</summary>
    [ObservableProperty]
    private FilterPipelineSnapshot _workingSnapshot = new();

    public LocalFilterScenarioViewModel()
    {
        Regenerate(1000);
    }

    public void Regenerate(int count)
    {
        Employees = EmployeeDataGenerator.Employees;
        if (GridViewModel == null)
        {
            GridViewModel = new FilterableDataGridViewModel<Employee>();
        }
        GridViewModel.LocalDataSource = Employees;
        GridViewModel.RefreshDataAsync();
    }

    [RelayCommand]
    private void SyncPipelineFromGrid()
    {
        WorkingSnapshot = GridViewModel.CreateFilterPipelineSnapshot();
        PipelineJson = FilterPipelineJson.Serialize(WorkingSnapshot);
    }

    [RelayCommand]
    private async Task ApplyPipelineJsonAsync()
    {
        var snapshot = FilterPipelineJson.Deserialize(PipelineJson);
        await GridViewModel.ApplyFilterPipelineSnapshotAsync(snapshot);
    }

    [RelayCommand]
    private async Task ApplyWorkingSnapshotAsync()
    {
        await GridViewModel.ApplyFilterPipelineSnapshotAsync(WorkingSnapshot);
    }

    [RelayCommand]
    private void AddSampleSortToWorkingSnapshot()
    {
        if (WorkingSnapshot.Nodes.Count == 0)
        {
            FilterPipelineSnapshotEditor.AddRootCriterion(
                WorkingSnapshot,
                "IsActive",
                nameof(FilterOperator.Equals),
                true);
        }

        FilterPipelineSnapshotEditor.AddSort(WorkingSnapshot, "Name", isDescending: false);
        FilterPipelineSnapshotEditor.AddSort(WorkingSnapshot, "Department", isDescending: true);
    }
}
