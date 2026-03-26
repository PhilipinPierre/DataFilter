using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataFilter.Demo.Shared.Services;

namespace DataFilter.WinUI3.Demo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private double _rowCount = 1000;

    public LocalFilterScenarioViewModel LocalFilterScenario { get; } = new();
    public AsyncFilterScenarioViewModel AsyncFilterScenario { get; } = new();
    public HybridFilterScenarioViewModel HybridFilterScenario { get; } = new();
    public CustomizationScenarioViewModel CustomizationScenario { get; } = new();
    public ListViewScenarioViewModel ListViewScenario { get; } = new();
    public CollectionViewScenarioViewModel CollectionViewScenario { get; } = new();

    public MainViewModel()
    {
        Regenerate();
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

        await LocalFilterScenario.GridViewModel.RefreshDataAsync();
        await AsyncFilterScenario.GridViewModel.RefreshDataAsync();
        await HybridFilterScenario.GridViewModel.RefreshDataAsync();
        await CustomizationScenario.GridViewModel.RefreshDataAsync();
        await ListViewScenario.GridViewModel.RefreshDataAsync();
        await CollectionViewScenario.GridViewModel.RefreshDataAsync();
    }
}
