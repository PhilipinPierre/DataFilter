using DataFilter.PlatformShared.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace DataFilter.UwpXaml.Controls;

public sealed class FilterableDataGrid : Control
{
    public IFilterableDataGridViewModel? ViewModel { get; set; }
}
