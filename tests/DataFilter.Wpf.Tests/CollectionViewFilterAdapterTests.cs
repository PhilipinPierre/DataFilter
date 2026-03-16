using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using DataFilter.Core.Enums;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Wpf.Adapters;
using Moq;
using Xunit;

namespace DataFilter.Wpf.Tests;

public class CollectionViewFilterAdapterTests
{
    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_WithNullCollectionView_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CollectionViewFilterAdapter<TestItem>(null!));
    }

    [Fact]
    public void ApplyColumnFilter_AppliesFilterToCollectionView()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Alice" },
            new TestItem { Id = 2, Name = "Bob" },
            new TestItem { Id = 3, Name = "Charlie" }
        };
        var collectionView = CollectionViewSource.GetDefaultView(items);
        var adapter = new CollectionViewFilterAdapter<TestItem>(collectionView);
        var state = new ExcelFilterState();
        state.SelectedValues.Add("Alice");
        state.SelectedValues.Add("Bob");
        state.SelectAll = false;

        // Act
        adapter.ApplyColumnFilter("Name", state);

        // Assert
        var filteredList = collectionView.Cast<TestItem>().ToList();
        Assert.Equal(2, filteredList.Count);
        Assert.Contains(filteredList, i => i.Name == "Alice");
        Assert.Contains(filteredList, i => i.Name == "Bob");
        Assert.DoesNotContain(filteredList, i => i.Name == "Charlie");
    }

    [Fact]
    public void ClearColumnFilter_RemovesFilterFromCollectionView()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Alice" },
            new TestItem { Id = 2, Name = "Bob" }
        };
        var collectionView = CollectionViewSource.GetDefaultView(items);
        var adapter = new CollectionViewFilterAdapter<TestItem>(collectionView);
        var state = new ExcelFilterState();
        state.SelectedValues.Add("Alice");
        state.SelectAll = false;
        adapter.ApplyColumnFilter("Name", state);

        // Act
        adapter.ClearColumnFilter("Name");

        // Assert
        var filteredList = collectionView.Cast<TestItem>().ToList();
        Assert.Equal(2, filteredList.Count);
    }

    [Fact]
    public void ApplySort_UpdatesSortDescriptions()
    {
        // Arrange
        var items = new List<TestItem> { new TestItem { Id = 1 }, new TestItem { Id = 2 } };
        var collectionView = CollectionViewSource.GetDefaultView(items);
        var adapter = new CollectionViewFilterAdapter<TestItem>(collectionView);

        // Act
        adapter.ApplySort("Id", true); // Descending

        // Assert
        Assert.Single(collectionView.SortDescriptions);
        Assert.Equal("Id", collectionView.SortDescriptions[0].PropertyName);
        Assert.Equal(ListSortDirection.Descending, collectionView.SortDescriptions[0].Direction);
    }

    [Fact]
    public void AddSubSort_AppendsToSortDescriptions()
    {
        // Arrange
        var items = new List<TestItem> { new TestItem { Id = 1 } };
        var collectionView = CollectionViewSource.GetDefaultView(items);
        var adapter = new CollectionViewFilterAdapter<TestItem>(collectionView);
        adapter.ApplySort("Name", false);

        // Act
        adapter.AddSubSort("Id", true);

        // Assert
        Assert.Equal(2, collectionView.SortDescriptions.Count);
        Assert.Equal("Name", collectionView.SortDescriptions[0].PropertyName);
        Assert.Equal("Id", collectionView.SortDescriptions[1].PropertyName);
    }

    [Fact]
    public void ClearSort_RemovesAllSortDescriptions()
    {
        // Arrange
        var items = new List<TestItem> { new TestItem { Id = 1 } };
        var collectionView = CollectionViewSource.GetDefaultView(items);
        var adapter = new CollectionViewFilterAdapter<TestItem>(collectionView);
        adapter.ApplySort("Id", true);

        // Act
        adapter.ClearSort();

        // Assert
        Assert.Empty(collectionView.SortDescriptions);
    }

    [Fact]
    public async Task GetDistinctValuesAsync_ReturnsUniqueValues()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Alice" },
            new TestItem { Id = 2, Name = "Alice" },
            new TestItem { Id = 3, Name = "Bob" }
        };
        var collectionView = CollectionViewSource.GetDefaultView(items);
        var adapter = new CollectionViewFilterAdapter<TestItem>(collectionView);

        // Act
        var result = await adapter.GetDistinctValuesAsync("Name", "");

        // Assert
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains("Alice", list);
        Assert.Contains("Bob", list);
    }

    [Fact]
    public void GetColumnFilterState_ReturnsCorrectState()
    {
        // Arrange
        var items = new List<TestItem>();
        var collectionView = CollectionViewSource.GetDefaultView(items);
        var adapter = new CollectionViewFilterAdapter<TestItem>(collectionView);
        var state = new ExcelFilterState { CustomValue1 = "Test" };
        adapter.ApplyColumnFilter("Name", state);

        // Act
        var result = adapter.GetColumnFilterState("Name");

        // Assert
        Assert.Same(state, result);
    }

    [Fact]
    public void FilteredItems_WithInconsistentTypes_DoesNotThrowAndSkipsNonMatching()
    {
        // Arrange
        var items = new ArrayList { new TestItem { Id = 1 }, "NotATestItem", new TestItem { Id = 2 } };
        var collectionView = CollectionViewSource.GetDefaultView(items);
        var adapter = new CollectionViewFilterAdapter<TestItem>(collectionView);

        // Act & Assert
        var filteredList = adapter.FilteredItems.ToList();
        Assert.Equal(2, filteredList.Count);
        Assert.All(filteredList, i => Assert.IsType<TestItem>(i));
    }
}
