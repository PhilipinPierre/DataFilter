using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataFilter.Demo.Shared.Services;

namespace DataFilter.Maui.Demo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private int _rowCount = 1000;

    [ObservableProperty]
    private string _rowCountText = "1000";

    partial void OnRowCountChanged(int value)
    {
        var normalized = value <= 0 ? 1 : value;
        if (normalized != value)
        {
            RowCount = normalized;
            return;
        }

        var str = RowCount.ToString();
        if (!string.Equals(RowCountText, str, StringComparison.Ordinal))
            RowCountText = str;
    }

    partial void OnRowCountTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var digitsOnly = new string(value.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length == 0)
            return;

        if (!string.Equals(value, digitsOnly, StringComparison.Ordinal))
        {
            RowCountText = digitsOnly;
            return;
        }

        if (int.TryParse(digitsOnly, out var parsed) && parsed > 0 && parsed != RowCount)
            RowCount = parsed;
    }

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

        RowCountText = RowCount.ToString();
        Regenerate();
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
    }

    [RelayCommand]
    private async Task ClearFilters()
    {
        LocalFilterScenario.GridViewModel?.Context.ClearDescriptors();
        AsyncFilterScenario.GridViewModel?.Context.ClearDescriptors();
        HybridFilterScenario.GridViewModel?.Context.ClearDescriptors();
        CustomizationScenario.GridViewModel?.Context.ClearDescriptors();
        ListViewScenario.GridViewModel?.Context.ClearDescriptors();
        CollectionViewScenario.GridViewModel?.Context.ClearDescriptors();

        var tasks = new List<Task>();
        if (LocalFilterScenario.GridViewModel != null) tasks.Add(LocalFilterScenario.GridViewModel.RefreshDataAsync());
        if (AsyncFilterScenario.GridViewModel != null) tasks.Add(AsyncFilterScenario.GridViewModel.RefreshDataAsync());
        if (HybridFilterScenario.GridViewModel != null) tasks.Add(HybridFilterScenario.GridViewModel.RefreshDataAsync());
        if (CustomizationScenario.GridViewModel != null) tasks.Add(CustomizationScenario.GridViewModel.RefreshDataAsync());
        if (ListViewScenario.GridViewModel != null) tasks.Add(ListViewScenario.GridViewModel.RefreshDataAsync());
        if (CollectionViewScenario.GridViewModel != null) tasks.Add(CollectionViewScenario.GridViewModel.RefreshDataAsync());

        await Task.WhenAll(tasks);
    }
}
