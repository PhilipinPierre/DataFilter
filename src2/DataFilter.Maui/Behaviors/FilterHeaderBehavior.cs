using DataFilter.Maui.Controls;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Behaviors;

public static class FilterHeaderBehavior
{
    public static FilterPopupView CreatePopup(IFilterableDataGridViewModel vm, string propertyName)
    {
        var popup = new FilterPopupView();
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
