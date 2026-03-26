using DataFilter.PlatformShared.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DataFilter.WinUI3.Controls;

public sealed partial class FilterableDataGrid : ListView
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
