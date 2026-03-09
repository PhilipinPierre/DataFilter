using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DataFilter.Wpf.Demo.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private int _rowCount = 1000;

    public LocalFilterScenarioViewModel LocalFilterScenario { get; } = new();
    public AsyncFilterScenarioViewModel AsyncFilterScenario { get; } = new();
    public HybridFilterScenarioViewModel HybridFilterScenario { get; } = new();
    public CustomizationScenarioViewModel CustomizationScenario { get; } = new();
    public ListViewScenarioViewModel ListViewScenario { get; } = new();

    public MainViewModel()
    {
    }

    [RelayCommand]
    private void Regenerate()
    {
        LocalFilterScenario.Regenerate(RowCount);
        AsyncFilterScenario.Regenerate(RowCount);
        HybridFilterScenario.Regenerate(RowCount);
        CustomizationScenario.Regenerate(RowCount);
        ListViewScenario.Regenerate(RowCount);
    }
}
