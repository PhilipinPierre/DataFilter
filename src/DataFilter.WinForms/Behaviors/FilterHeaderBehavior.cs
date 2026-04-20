using DataFilter.Localization;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.Controls;

namespace DataFilter.WinForms.Behaviors;

public sealed class FilterHeaderBehavior
{
    public static async Task<FilterPopupControl> CreatePopupAsync(IFilterableDataGridViewModel vm, string propertyName)
    {
        var previousCulture = LocalizationManager.Instance.Culture;
        if (vm.CultureOverride != null)
            LocalizationManager.Instance.SetCulture(vm.CultureOverride);

        var popup = new FilterPopupControl();
        var columnVm = new ColumnFilterViewModel(
            (search) => vm.GetDistinctValuesAsync(propertyName, search),
            (state) => vm.ApplyColumnFilter(propertyName, state),
            () => vm.ClearColumnFilter(propertyName),
            (isDesc) => vm.ApplySort(propertyName, isDesc),
            (isDesc) => vm.AddSubSort(propertyName, isDesc),
            vm.GetPropertyType(propertyName));
        var distinct = await vm.GetDistinctValuesAsync(propertyName, string.Empty);
        await popup.BindAsync(columnVm, distinct);

        popup.RequestClose += () => LocalizationManager.Instance.SetCulture(previousCulture);
        return popup;
    }
}
