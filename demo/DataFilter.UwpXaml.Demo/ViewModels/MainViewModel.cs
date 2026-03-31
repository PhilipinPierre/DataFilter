using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataFilter.Demo.Shared.Services;

namespace DataFilter.UwpXaml.Demo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public double RowCount { get; set; } = 1000;

    public LocalFilterScenarioViewModel LocalFilterScenario { get; }
    public AsyncFilterScenarioViewModel AsyncFilterScenario { get; }
    public HybridFilterScenarioViewModel HybridFilterScenario { get; }
    public CustomizationScenarioViewModel CustomizationScenario { get; }
    public ListViewScenarioViewModel ListViewScenario { get; }
    public CollectionViewScenarioViewModel CollectionViewScenario { get; }

    public MainViewModel(
        LocalFilterScenarioViewModel localFilterScenario,
        AsyncFilterScenarioViewModel asyncFilterScenario,
        HybridFilterScenarioViewModel hybridFilterScenario,
        CustomizationScenarioViewModel customizationScenario,
        ListViewScenarioViewModel listViewScenario,
        CollectionViewScenarioViewModel collectionViewScenario)
    {
        LocalFilterScenario = localFilterScenario;
        AsyncFilterScenario = asyncFilterScenario;
        HybridFilterScenario = hybridFilterScenario;
        CustomizationScenario = customizationScenario;
        ListViewScenario = listViewScenario;
        CollectionViewScenario = collectionViewScenario;
    }

    [RelayCommand]
    private void Regenerate()
    {
        int count = (int)RowCount;
        EmployeeDataGenerator.Regenerate(count);

        LocalFilterScenario.Regenerate(count);
        AsyncFilterScenario.Regenerate(count);
        HybridFilterScenario.Regenerate(count);
        CustomizationScenario.Regenerate(count);
        ListViewScenario.Regenerate(count);
        CollectionViewScenario.Regenerate(count);

    }

    [RelayCommand]
    private async Task ClearFilters()
    {
        LocalFilterScenario.GridViewModel.Context.ClearDescriptors();
        AsyncFilterScenario.GridViewModel.Context.ClearDescriptors();
        HybridFilterScenario.GridViewModel.Context.ClearDescriptors();
        CustomizationScenario.GridViewModel.Context.ClearDescriptors();
        ListViewScenario.GridViewModel.Context.ClearDescriptors();
        CollectionViewScenario.GridViewModel.Context.ClearDescriptors();

        await Task.WhenAll(
            LocalFilterScenario.GridViewModel.RefreshDataAsync(),
            AsyncFilterScenario.GridViewModel.RefreshDataAsync(),
            HybridFilterScenario.GridViewModel.RefreshDataAsync(),
            CustomizationScenario.GridViewModel.RefreshDataAsync(),
            ListViewScenario.GridViewModel.RefreshDataAsync(),
            CollectionViewScenario.GridViewModel.RefreshDataAsync()
        );
    }
}
