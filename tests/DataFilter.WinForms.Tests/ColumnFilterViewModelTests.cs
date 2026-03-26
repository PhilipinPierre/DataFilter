using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.WinForms.Tests;

public class ColumnFilterViewModelTests
{
    [Fact]
    public async Task ApplyCommand_SetsCustomOperator_AndClearsSearchText()
    {
        var vm = new ColumnFilterViewModel(
            _ => Task.FromResult<IEnumerable<object>>(new object[] { "A", "B", null! }),
            _ => { },
            () => { },
            propertyType: typeof(string));

        await vm.InitializeAsync(new object[] { "A", "B" });
        vm.SelectedCustomOperator = DataFilter.Core.Enums.FilterOperator.Contains;
        vm.CustomValue1 = "A";
        vm.SearchText = "A";

        vm.ApplyCommand.Execute(null);

        Assert.Equal(DataFilter.Core.Enums.FilterOperator.Contains, vm.FilterState.CustomOperator);
        Assert.Equal(string.Empty, vm.FilterState.SearchText);
    }
}
