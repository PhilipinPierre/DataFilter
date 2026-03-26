using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.UwpXaml.Tests;

public class SnapshotTests
{
    private sealed class Item
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task ExtractAndRestoreSnapshot_RetainsSortDescriptors()
    {
        var vm = new FilterableDataGridViewModel<Item> { LocalDataSource = new[] { new Item { Name = "B" }, new Item { Name = "A" } } };
        vm.ApplySort(nameof(Item.Name), false);
        await Task.Delay(50);

        var snap = vm.ExtractSnapshot();
        var vm2 = new FilterableDataGridViewModel<Item> { LocalDataSource = vm.LocalDataSource };
        vm2.RestoreSnapshot(snap);
        await Task.Delay(50);

        Assert.Single(vm2.Context.SortDescriptors);
    }
}
