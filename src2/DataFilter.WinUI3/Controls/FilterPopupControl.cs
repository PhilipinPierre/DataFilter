using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.WinUI3.Controls;

public sealed class FilterPopupControl
{
    public ColumnFilterViewModel? ViewModel { get; private set; }

    public void Bind(ColumnFilterViewModel vm) => ViewModel = vm;
}
