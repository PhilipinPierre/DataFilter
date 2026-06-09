using DataFilter.PlatformShared.ViewModels;
using DataFilter.Core.Enums;
using DataFilter.Filtering.ExcelLike.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DataFilter.Blazor.Tests.ViewModels;

public class BlazorColumnFilterViewModelTests
{
    [Fact]
    public async Task InitializeAsync_FlatList_CreatesCorrectItems()
    {
        // Arrange
        var distinctValues = new List<object> { "A", "B", "C" };
        var vm = new ColumnFilterViewModel(
            s => Task.FromResult<IEnumerable<object>>(distinctValues),
            _ => { },
            () => { },
            _ => { },
            _ => { },
            typeof(string)
        );

        // Act
        await vm.InitializeAsync(distinctValues);

        // Assert
        Assert.Equal(3, vm.FilterValues.Count);
        Assert.Contains(vm.FilterValues, x => x.DisplayText == "A");
        Assert.Contains(vm.FilterValues, x => x.DisplayText == "B");
        Assert.Contains(vm.FilterValues, x => x.DisplayText == "C");
        Assert.All(vm.FilterValues, x => Assert.True(x.IsSelected));
    }

    [Fact]
    public async Task InitializeAsync_IncludesBlanksCheckbox_WhenNullIsDistinct()
    {
        var distinctValues = new List<object?> { "A", null, "B" };
        var vm = new ColumnFilterViewModel(
            s => Task.FromResult<IEnumerable<object>>(distinctValues!),
            _ => { },
            () => { },
            _ => { },
            _ => { },
            typeof(string),
            blanksDisplayText: "(Blanks)");

        await vm.InitializeAsync(distinctValues!);

        Assert.Equal(3, vm.FilterValues.Count);
        var blanksItem = Assert.Single(vm.FilterValues, x => x.Value == null);
        Assert.Equal("(Blanks)", blanksItem.DisplayText);
    }

    [Fact]
    public async Task ApplyCommand_PersistsNullSelection_InSelectedValues()
    {
        var distinctValues = new List<object?> { "A", null, "B" };
        ExcelFilterState? appliedState = null;
        var vm = new ColumnFilterViewModel(
            s => Task.FromResult<IEnumerable<object>>(distinctValues!),
            s => appliedState = s,
            () => { },
            _ => { },
            _ => { },
            typeof(string),
            blanksDisplayText: "(Blanks)");

        await vm.InitializeAsync(distinctValues!);
        vm.SelectAll = false;
        foreach (var item in vm.FilterValues)
            item.IsSelected = item.Value == null;

        vm.ApplyCommand.Execute(null);

        Assert.NotNull(appliedState);
        Assert.Single(appliedState!.SelectedValues);
        Assert.Null(appliedState.SelectedValues.First());
    }

    [Fact]
    public async Task InitializeAsync_DateTree_IncludesBlanksCheckbox_WhenNullIsDistinct()
    {
        var d1 = new DateTime(2024, 3, 15);
        var distinctValues = new List<object?> { d1, null };

        var vm = new ColumnFilterViewModel(
            s => Task.FromResult<IEnumerable<object>>(distinctValues!),
            _ => { },
            () => { },
            _ => { },
            _ => { },
            typeof(DateTime?),
            blanksDisplayText: "(Blanks)");

        await vm.InitializeAsync(distinctValues!);

        var blanksItem = vm.FilterValues.Single(x => x.Value == null && x.Children.Count == 0);
        Assert.Equal("(Blanks)", blanksItem.DisplayText);
        Assert.True(vm.FilterState.DistinctValues.Any(v => v == null));
    }

    [Fact]
    public async Task ApplyCommand_UpdatesFilterState()
    {
        // Arrange
        var distinctValues = new List<object> { "A", "B", "C" };
        ExcelFilterState appliedState = null!;
        var vm = new ColumnFilterViewModel(
            s => Task.FromResult<IEnumerable<object>>(distinctValues),
            s => appliedState = s,
            () => { },
            _ => { },
            _ => { },
            typeof(string)
        );
        await vm.InitializeAsync(distinctValues);

        // Act
        vm.FilterValues[0].IsSelected = false; // Deselect "A"
        vm.ApplyCommand.Execute(null);

        // Assert
        Assert.NotNull(appliedState);
        Assert.False(appliedState.SelectedValues.Contains("A"));
        Assert.True(appliedState.SelectedValues.Contains("B"));
        Assert.True(appliedState.SelectedValues.Contains("C"));
    }

    [Fact]
    public async Task ClearCommand_ResetsState()
    {
        // Arrange
        var distinctValues = new List<object> { "A", "B", "C" };
        bool cleared = false;
        var vm = new ColumnFilterViewModel(
            s => Task.FromResult<IEnumerable<object>>(distinctValues),
            _ => { },
            () => cleared = true,
            _ => { },
            _ => { },
            typeof(string)
        );
        await vm.InitializeAsync(distinctValues);
        vm.FilterValues[0].IsSelected = false;

        // Act
        vm.ClearCommand.Execute(null);

        // Assert
        Assert.True(cleared);
        Assert.All(vm.FilterValues, x => Assert.True(x.IsSelected));
        Assert.True(vm.SelectAll);
    }

    [Fact]
    public async Task ApplyCommand_WithSearchAndSelectAll_PersistsAsStartsWith()
    {
        // Arrange
        var distinctValues = new List<object> { "Alice 1", "Alice 2", "Henry 1" };
        ExcelFilterState appliedState = null!;
        var vm = new ColumnFilterViewModel(
            _ => Task.FromResult<IEnumerable<object>>(distinctValues),
            s => appliedState = s,
            () => { },
            _ => { },
            _ => { },
            typeof(string)
        );
        await vm.InitializeAsync(distinctValues);

        // Act
        vm.SearchText = "Alice";
        await vm.SearchCommand.ExecuteAsync("Alice");
        vm.SelectAll = true;
        vm.ApplyCommand.Execute(null);

        // Assert
        Assert.NotNull(appliedState);
        Assert.Equal(FilterOperator.StartsWith, appliedState.CustomOperator);
        Assert.Equal("Alice", appliedState.CustomValue1);
    }

    [Fact]
    public async Task ApplyCommand_UnionOfSearches_PersistsAsOrSearchPatterns()
    {
        // Arrange
        var distinctValues = new List<object> { "Alice 1", "Henry 1", "Henry 2" };
        ExcelFilterState appliedState = null!;
        var vm = new ColumnFilterViewModel(
            _ => Task.FromResult<IEnumerable<object>>(distinctValues),
            s => appliedState = s,
            () => { },
            _ => { },
            _ => { },
            typeof(string)
        );
        await vm.InitializeAsync(distinctValues);

        // First search
        vm.SearchText = "Alice";
        await vm.SearchCommand.ExecuteAsync("Alice");
        vm.SelectAll = true;
        vm.ApplyCommand.Execute(null);

        // Second search as UNION
        vm.AddToExistingFilter = true;
        vm.AccumulationMode = AccumulationMode.Union;
        vm.SearchText = "Henry";
        await vm.SearchCommand.ExecuteAsync("Henry");
        vm.SelectAll = true;
        vm.ApplyCommand.Execute(null);

        // Assert
        Assert.NotNull(appliedState);
        Assert.Contains("Alice", appliedState.OrSearchPatterns);
        Assert.Contains("Henry", appliedState.OrSearchPatterns);
    }

    [Fact]
    public async Task SearchCommand_OverlappingInvocations_DoNotThrowOrCorruptState()
    {
        // Arrange
        var distinctValues = new List<object> { "Alice 1", "Alice 2", "Henry 1", "Henry 2" };
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        int started = 0;

        var vm = new ColumnFilterViewModel(
            async s =>
            {
                if (Interlocked.Increment(ref started) == 2)
                    gate.TrySetResult(true);

                await gate.Task;
                await Task.Delay(5);

                if (string.IsNullOrWhiteSpace(s))
                    return distinctValues;

                return distinctValues.Where(v => v.ToString()!.StartsWith(s, System.StringComparison.OrdinalIgnoreCase));
            },
            _ => { },
            () => { },
            _ => { },
            _ => { },
            typeof(string)
        );
        await vm.InitializeAsync(distinctValues);

        // Act
        vm.SearchText = "Alice";
        var searchAlice = vm.SearchCommand.ExecuteAsync("Alice");

        vm.SearchText = "Henry";
        var searchHenry = vm.SearchCommand.ExecuteAsync("Henry");

        var allSearches = Task.WhenAll(searchAlice, searchHenry);
        var completed = await Task.WhenAny(allSearches, Task.Delay(2000));
        Assert.Same(allSearches, completed);
        await allSearches;
        var ex = Record.Exception(() => vm.ApplyCommand.Execute(null));

        // Assert
        Assert.Null(ex);
        Assert.NotEmpty(vm.FilterValues);
    }
}
