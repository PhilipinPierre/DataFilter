using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Controls;

public sealed class FilterableDataGrid : ListView
{
    public static readonly BindableProperty ViewModelProperty =
        BindableProperty.Create(nameof(ViewModel), typeof(IFilterableDataGridViewModel), typeof(FilterableDataGrid));

    public IFilterableDataGridViewModel? ViewModel
    {
        get => (IFilterableDataGridViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public FilterableDataGrid()
    {
        SeparatorVisibility = SeparatorVisibility.Default;
        SelectionMode = ListViewSelectionMode.None;
    }
}
