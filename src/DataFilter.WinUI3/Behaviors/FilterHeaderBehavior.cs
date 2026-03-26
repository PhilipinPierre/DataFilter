using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinUI3.Controls;

namespace DataFilter.WinUI3.Behaviors;

public static class FilterHeaderBehavior
{
    public static FilterPopupControl CreatePopup(IFilterableDataGridViewModel vm, string propertyName)
    {
        var popup = new FilterPopupControl();
        popup.Bind(new ColumnFilterViewModel(
            search => vm.GetDistinctValuesAsync(propertyName, search),
            state => vm.ApplyColumnFilter(propertyName, state),
            () => vm.ClearColumnFilter(propertyName),
            isDesc => vm.ApplySort(propertyName, isDesc),
            isDesc => vm.AddSubSort(propertyName, isDesc),
            vm.GetPropertyType(propertyName)));
        return popup;
    }
}
