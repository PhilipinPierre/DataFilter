using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        PipelineJson = FilterPipelineJson.Serialize(
            FilterPipelineSnapshotMapper.ToSnapshot(GridViewModel.CreatePipelineFromCurrentSnapshot()));
    }

    [RelayCommand]
    private async Task ApplyPipelineJsonAsync()
    {
        var snapshot = FilterPipelineJson.Deserialize(PipelineJson);
        var pipeline = FilterPipelineSnapshotMapper.ToPipeline(snapshot);
        await GridViewModel.ApplyFilterPipelineAsync(pipeline);
    }
}
