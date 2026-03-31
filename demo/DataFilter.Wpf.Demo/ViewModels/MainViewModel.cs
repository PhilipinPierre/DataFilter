using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataFilter.Demo.Shared.Services;
using System.Windows;
using System.Windows.Controls;

namespace DataFilter.Wpf.Demo.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private int _rowCount = 1000;

    public TabItem SelectedTabControl { get; set; }

    public LocalFilterScenarioViewModel LocalFilterScenario { get; } = new();
    public AsyncFilterScenarioViewModel AsyncFilterScenario { get; } = new();
    public HybridFilterScenarioViewModel HybridFilterScenario { get; } = new();
    public CustomizationScenarioViewModel CustomizationScenario { get; } = new();
    public ListViewScenarioViewModel ListViewScenario { get; } = new();
    public CollectionViewScenarioViewModel CollectionViewScenario { get; } = new();

    public MainViewModel()
    {
    }

    [RelayCommand]
    private void Regenerate()
    {
        EmployeeDataGenerator.Regenerate(RowCount);

        LocalFilterScenario.Regenerate(RowCount);
        AsyncFilterScenario.Regenerate(RowCount);
        HybridFilterScenario.Regenerate(RowCount);
        CustomizationScenario.Regenerate(RowCount);
        ListViewScenario.Regenerate(RowCount);
        CollectionViewScenario.Regenerate(RowCount);

        //SelectedTabControl = LocalFilterScenario;
    }

    [RelayCommand]
    private async Task ClearFilters()
    {
        if (SelectedTabControl != null && SelectedTabControl.Content is FrameworkElement element && element.DataContext is IDemoItem demoItem)
        {
            demoItem.GridViewModel.Context.ClearDescriptors();
            await demoItem.GridViewModel.RefreshDataAsync();
        }
        else
        {
            this.LocalFilterScenario.GridViewModel.Context.ClearDescriptors();
            this.AsyncFilterScenario.GridViewModel.Context.ClearDescriptors();
            this.HybridFilterScenario.GridViewModel.Context.ClearDescriptors();
            this.CustomizationScenario.GridViewModel.Context.ClearDescriptors();
            this.ListViewScenario.GridViewModel.Context.ClearDescriptors();
            this.CollectionViewScenario.GridViewModel.Context.ClearDescriptors();

            await Task.WhenAll(
                this.LocalFilterScenario.GridViewModel.RefreshDataAsync(),
                this.AsyncFilterScenario.GridViewModel.RefreshDataAsync(),
                this.HybridFilterScenario.GridViewModel.RefreshDataAsync(),
                this.CustomizationScenario.GridViewModel.RefreshDataAsync(),
                this.ListViewScenario.GridViewModel.RefreshDataAsync(),
                this.CollectionViewScenario.GridViewModel.RefreshDataAsync()
            );
        }
    }
}
