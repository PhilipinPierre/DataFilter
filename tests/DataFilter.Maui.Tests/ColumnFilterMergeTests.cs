using DataFilter.Core.Enums;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Tests;

public class ColumnFilterMergeTests
{
    [Fact]
    public async Task AddToExistingFilter_Union_DoesNotThrow()
    {
        var vm = new ColumnFilterViewModel(
            _ => Task.FromResult<IEnumerable<object>>(new object[] { "A", "B" }),
            _ => { },
            () => { },
            propertyType: typeof(string));

        await vm.InitializeAsync(new object[] { "A", "B" });
        vm.AddToExistingFilter = true;
        vm.AccumulationMode = AccumulationMode.Union;

        vm.ApplyCommand.Execute(null);

        Assert.NotNull(vm.FilterState.SelectedValues);
    }
}
