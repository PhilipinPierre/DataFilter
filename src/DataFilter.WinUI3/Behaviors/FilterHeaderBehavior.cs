using DataFilter.Localization;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinUI3.Controls;

namespace DataFilter.WinUI3.Behaviors;

public static class FilterHeaderBehavior
{
    public static FilterPopupControl CreatePopup(IFilterableDataGridViewModel vm, string propertyName)
    {
        var previousCulture = LocalizationManager.Instance.Culture;
        if (vm.CultureOverride != null)
            LocalizationManager.Instance.SetCulture(vm.CultureOverride);

        var popup = new FilterPopupControl();
        var columnVm = new ColumnFilterViewModel(
            search => vm.GetDistinctValuesAsync(propertyName, search),
            state => vm.ApplyColumnFilter(propertyName, state),
            () => vm.ClearColumnFilter(propertyName),
            isDesc => vm.ApplySort(propertyName, isDesc),
            isDesc => vm.AddSubSort(propertyName, isDesc),
            vm.GetPropertyType(propertyName));

        columnVm.OnApply += (_, _) => LocalizationManager.Instance.SetCulture(previousCulture);
        columnVm.OnClear += (_, _) => LocalizationManager.Instance.SetCulture(previousCulture);

        popup.Bind(columnVm);
        return popup;
    }
}
