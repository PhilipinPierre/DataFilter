using Xunit;
using Moq;
using DataFilter.Wpf.ViewModels;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Core.Enums;
using DataFilter.Wpf.Enums;

namespace DataFilter.Wpf.Tests;

/// <summary>
/// Unit tests for the <see cref="ColumnFilterViewModel"/> class.
/// </summary>
public class ColumnFilterViewModelTests
{
    [Fact]
    public void ApplyCommand_WithAddToExistingFilter_MergesSelectedValues()
    {
        // Arrange
        var appliedState = new ExcelFilterState();
        appliedState.SelectedValues.Add("PreviousValue");
        
        var vm = new ColumnFilterViewModel(
            async (s) => (IEnumerable<object>)new[] { "NewValue" },
            (state) => { /* Capture state if needed */ },
            () => { },
            propertyType: typeof(string)
        );
        
        // Initialize with previous state
        vm.LoadState(appliedState);
        
        // Manual setup of FilterValues (items in the list)
        var item = new FilterValueItem("NewValue", "NewValue", null, false);
        item.IsSelected = true;
        vm.FilterValues.Add(item);
        
        vm.AddToExistingFilter = true;

        // Act
        vm.ApplyCommand.Execute(null);

        // Assert
        Assert.Contains("PreviousValue", vm.FilterState.SelectedValues);
        Assert.Contains("NewValue", vm.FilterState.SelectedValues);
        Assert.False(vm.FilterState.SelectAll);
    }

    [Fact]
    public void ApplyCommand_WithCustomFilter_PersistsToState()
    {
        // Arrange
        ExcelFilterState? resultState = null;
        var vm = new ColumnFilterViewModel(
            async (s) => Enumerable.Empty<object>(),
            (state) => resultState = state,
            () => { },
            propertyType: typeof(int)
        );

        vm.SelectedCustomOperator = FilterOperator.GreaterThan;
        vm.CustomValue1 = "100";

        // Act
        vm.ApplyCommand.Execute(null);

        // Assert
        Assert.NotNull(resultState);
        Assert.Equal(FilterOperator.GreaterThan, resultState!.CustomOperator);
        Assert.Equal("100", resultState.CustomValue1);
    }

    [Fact]
    public void ClearFilter_ResetsCustomFilterProperties()
    {
        // Arrange
        var vm = new ColumnFilterViewModel(
            async (s) => Enumerable.Empty<object>(),
            (state) => { },
            () => { },
            propertyType: typeof(DateTime)
        );

        vm.SelectedCustomOperator = FilterOperator.Between;
        vm.CustomValue1 = "2024-01-01";
        vm.CustomValue2 = "2024-12-31";
        vm.IsCustomFilterExpanded = true;

        // Act
        vm.ClearCommand.Execute(null);

        // Assert
        Assert.Null(vm.SelectedCustomOperator);
        Assert.Equal(string.Empty, vm.CustomValue1);
        Assert.Equal(string.Empty, vm.CustomValue2);
        Assert.False(vm.IsCustomFilterExpanded);
    }

    [Fact]
    public void Constructor_SetsDataTypeAndAvailableOperators()
    {
        // Arrange & Act
        var vm = new ColumnFilterViewModel(
            async (s) => Enumerable.Empty<object>(),
            (state) => { },
            () => { },
            propertyType: typeof(int)
        );

        // Assert
        Assert.Equal(FilterDataType.Number, vm.DataType);
        Assert.Contains(FilterOperator.GreaterThan, vm.AvailableOperators);
        Assert.Contains(FilterOperator.Between, vm.AvailableOperators);
        Assert.DoesNotContain(FilterOperator.Contains, vm.AvailableOperators);
    }
}
