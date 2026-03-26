using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.WinUI3.Tests;

public class FilterableDataGridViewModelTests
{
    private sealed class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task ApplySort_SortsFilteredItems()
    {
        var vm = new FilterableDataGridViewModel<Item>
        {
            LocalDataSource = new[] { new Item { Id = 2, Name = "B" }, new Item { Id = 1, Name = "A" } }
        };

        vm.ApplySort(nameof(Item.Id), isDescending: false);
        await Task.Delay(50);

        Assert.Equal(new[] { 1, 2 }, vm.FilteredItems.Select(x => x.Id));
    }
}
