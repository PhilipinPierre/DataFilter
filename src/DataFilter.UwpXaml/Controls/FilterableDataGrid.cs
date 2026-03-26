using DataFilter.PlatformShared.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DataFilter.UwpXaml.Controls;

public sealed class FilterableDataGrid : ListView
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(IFilterableDataGridViewModel),
            typeof(FilterableDataGrid),
            new PropertyMetadata(null));

    public IFilterableDataGridViewModel? ViewModel
    {
        get => (IFilterableDataGridViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
}
