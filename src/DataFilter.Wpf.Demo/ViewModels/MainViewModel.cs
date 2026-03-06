using CommunityToolkit.Mvvm.ComponentModel;

namespace DataFilter.Wpf.Demo.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentScenario;

    public MainViewModel()
    {
        CurrentScenario = new CustomizationScenarioViewModel();
    }
}
