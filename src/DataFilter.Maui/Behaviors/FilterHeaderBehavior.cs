using DataFilter.Maui.Controls;
using DataFilter.Localization;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Behaviors;

public static class FilterHeaderBehavior
{
    public static FilterPopupView CreatePopup(IFilterableDataGridViewModel vm, string propertyName)
    {
        var previousCulture = LocalizationManager.Instance.Culture;
        if (vm.CultureOverride != null)
            LocalizationManager.Instance.SetCulture(vm.CultureOverride);

        var popup = new FilterPopupView();
        var columnVm = new ColumnFilterViewModel(
            search => vm.GetDistinctValuesAsync(propertyName, search),
            state => vm.ApplyColumnFilter(propertyName, state),
            () => vm.ClearColumnFilter(propertyName),
            isDesc => vm.ApplySort(propertyName, isDesc),
            isDesc => vm.AddSubSort(propertyName, isDesc),
            vm.GetPropertyType(propertyName));

        // Match WPF/WinUI behavior: load existing column state into the popup VM.
        var existingState = vm.GetColumnFilterState(propertyName);
        if (existingState != null)
        {
            _ = columnVm.LoadStateAsync(existingState);
        }

        popup.Bind(columnVm);
        popup.CloseRequested += (_, _) => LocalizationManager.Instance.SetCulture(previousCulture);
        return popup;
    }
}
