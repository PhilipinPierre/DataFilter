using DataFilter.Blazor.ViewModels;
using DataFilter.Core.Enums;
using DataFilter.Filtering.ExcelLike.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        var vm = new BlazorColumnFilterViewModel(
            s => Task.FromResult<IEnumerable<object>>(distinctValues),
            s => { },
            () => { },
            null,
            null,
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
    public async Task ApplyCommand_UpdatesFilterState()
    {
        // Arrange
        var distinctValues = new List<object> { "A", "B", "C" };
        ExcelFilterState appliedState = null!;
        var vm = new BlazorColumnFilterViewModel(
            s => Task.FromResult<IEnumerable<object>>(distinctValues),
            s => appliedState = s,
            () => { },
            null,
            null,
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
        var vm = new BlazorColumnFilterViewModel(
            s => Task.FromResult<IEnumerable<object>>(distinctValues),
            s => { },
            () => cleared = true,
            null,
            null,
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
}
