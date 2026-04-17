using Xunit;
using Moq;
using DataFilter.Wpf.ViewModels;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Core.Enums;

namespace DataFilter.Wpf.Tests;

/// <summary>
/// Unit tests for the <see cref="ColumnFilterViewModel"/> class.
/// </summary>
public class ColumnFilterViewModelTests
{
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
    [Fact]
    public void ApplyCommand_WithAddToExistingFilterButNoActiveFilter_ShouldReplace()
    {
        // Arrange
        ExcelFilterState? resultState = null;
        var vm = new ColumnFilterViewModel(
            async (s) => new List<object> { "Alice", "Bob" },
            (state) => resultState = state,
            () => { },
            propertyType: typeof(string)
        );

        // Initially no filter
        Assert.False(vm.IsFilterActive);

        vm.AddToExistingFilter = true;
        // Mock selection of Alice
        var aliceItem = new FilterValueItem("Alice", "Alice", null, true);
        vm.FilterValues.Add(aliceItem);

        // Act
        vm.ApplyCommand.Execute(null);

        // Assert
        Assert.NotNull(resultState);
        Assert.Single(resultState!.SelectedValues);
        Assert.Contains("Alice", resultState.SelectedValues);
    }

    [Fact]
    public void ApplyCommand_WithAddToExistingFilterAndActiveFilter_ShouldAccumulate()
    {
        // Arrange
        ExcelFilterState? resultState = null;
        var vm = new ColumnFilterViewModel(
            async (s) => new List<object> { "Alice", "Bob" },
            (state) => resultState = state,
            () => { },
            propertyType: typeof(string)
        );

        // Manually active a filter
        vm.FilterState.SelectedValues.Add("Bob");
        vm.FilterState.SelectAll = false;
        Assert.True(vm.IsFilterActive);

        vm.AddToExistingFilter = true;
        // Mock selection of Alice in current dialog
        var aliceItem = new FilterValueItem("Alice", "Alice", null, true);
        vm.FilterValues.Add(aliceItem);

        // Act
        vm.ApplyCommand.Execute(null);

        // Assert
        Assert.NotNull(resultState);
        Assert.Equal(2, resultState!.SelectedValues.Count);
        Assert.Contains("Alice", resultState.SelectedValues);
        Assert.Contains("Bob", resultState.SelectedValues);
    }

    [Fact]
    public async Task UpdateSelection_WithContainsOperator_SelectsMatchingItems()
    {
        // Arrange
        var vm = new ColumnFilterViewModel(
            async (s) => new List<object> { "Apple", "Banana", "Cherry" },
            (state) => { },
            () => { },
            propertyType: typeof(string)
        );
        await vm.InitializeAsync(new List<object> { "Apple", "Banana", "Cherry" });

        // Act
        vm.SelectedCustomOperator = FilterOperator.Contains;
        vm.CustomValue1 = "a";

        // Assert
        Assert.True(vm.FilterValues.First(x => x.DisplayText == "Apple").IsSelected);
        Assert.True(vm.FilterValues.First(x => x.DisplayText == "Banana").IsSelected);
        Assert.False(vm.FilterValues.First(x => x.DisplayText == "Cherry").IsSelected);
    }

    [Fact]
    public async Task UpdateSelection_WithGreaterThanOperator_SelectsMatchingNumbers()
    {
        // Arrange
        var vm = new ColumnFilterViewModel(
            async (s) => new List<object> { 10, 20, 30 },
            (state) => { },
            () => { },
            propertyType: typeof(int)
        );
        await vm.InitializeAsync(new List<object> { 10, 20, 30 });

        // Act
        vm.SelectedCustomOperator = FilterOperator.GreaterThan;
        vm.CustomValue1 = "15";

        // Assert
        Assert.False(vm.FilterValues.First(x => (int)x.Value! == 10).IsSelected);
        Assert.True(vm.FilterValues.First(x => (int)x.Value! == 20).IsSelected);
        Assert.True(vm.FilterValues.First(x => (int)x.Value! == 30).IsSelected);
    }

    [Fact]
    public async Task UpdateSelection_WithBetweenOperator_SelectsMatchingDates()
    {
        // Arrange
        var d1 = new DateTime(2024, 1, 1);
        var d2 = new DateTime(2024, 6, 1);
        var d3 = new DateTime(2024, 12, 1);

        var vm = new ColumnFilterViewModel(
            async (s) => new List<object> { d1, d2, d3 },
            (state) => { },
            () => { },
            propertyType: typeof(DateTime)
        );
        await vm.InitializeAsync(new List<object> { d1, d2, d3 });

        // Act
        vm.SelectedCustomOperator = FilterOperator.Between;
        vm.CustomValue1 = "2024-01-01";
        vm.CustomValue2 = "2024-07-01";

        // Assert
        // In hierarchical view (DateTree), we check selected values in state or recursive selection
        var selectedValues = new HashSet<object>();
        foreach (var item in vm.FilterValues) item.GetSelectedValues(selectedValues);

        Assert.Contains(d1, selectedValues);
        Assert.Contains(d2, selectedValues);
        Assert.DoesNotContain(d3, selectedValues);
    }

    [Fact]
    public async Task ApplyCommand_WithAddToExistingFilterAndCustomOperator_AccumulatesAndResetsCustomFilter()
    {
        // Arrange
        ExcelFilterState? resultState = null;
        var vm = new ColumnFilterViewModel(
            async (s) => new List<object> { "Alice", "Bob", "Charlie" },
            (state) => resultState = state,
            () => { },
            propertyType: typeof(string)
        );
        await vm.InitializeAsync(new List<object> { "Alice", "Bob", "Charlie" });

        // 1. Initial filter: Select Alice
        vm.FilterValues.First(x => x.DisplayText == "Alice").IsSelected = true;
        vm.FilterValues.First(x => x.DisplayText == "Bob").IsSelected = false;
        vm.FilterValues.First(x => x.DisplayText == "Charlie").IsSelected = false;
        vm.ApplyCommand.Execute(null);
        Assert.Single(resultState!.SelectedValues);

        // 2. Add to existing: Use advanced filter for "Bob"
        vm.AddToExistingFilter = true;
        vm.SelectedCustomOperator = FilterOperator.Equals;
        vm.CustomValue1 = "Bob";
        // UpdateSelectionFromCustomFilter should have selected Bob in FilterValues
        
        // Act
        vm.ApplyCommand.Execute(null);

        // Assert
        Assert.NotNull(resultState);
        Assert.Equal(2, resultState!.SelectedValues.Count);
        Assert.Contains("Alice", resultState.SelectedValues);
        Assert.Contains("Bob", resultState.SelectedValues);
        
        // Custom filter should be reset in VM
        Assert.Null(vm.SelectedCustomOperator);
        Assert.Equal(string.Empty, vm.CustomValue1);
        
        // Custom filter should be null in state
        Assert.Null(resultState.CustomOperator);
    }

    [Fact]
    public async Task ApplyCommand_WithSearchAndSelectAllVisible_AccumulatesCorrectly()
    {
        // Arrange
        ExcelFilterState? resultState = null;
        var vm = new ColumnFilterViewModel(
            async (s) => 
            {
                var all = new List<object> { "Alice", "Bob", "Charlie" };
                if (string.IsNullOrEmpty(s)) return all;
                return all.Where(x => x.ToString()!.Contains(s, StringComparison.OrdinalIgnoreCase));
            },
            (state) => resultState = state,
            () => { },
            propertyType: typeof(string)
        );
        await vm.InitializeAsync(new List<object> { "Alice", "Bob", "Charlie" });

        // 1. Initial filter: Select Alice
        vm.FilterValues.First(x => x.DisplayText == "Alice").IsSelected = true;
        vm.FilterValues.First(x => x.DisplayText == "Bob").IsSelected = false;
        vm.FilterValues.First(x => x.DisplayText == "Charlie").IsSelected = false;
        vm.ApplyCommand.Execute(null);
        Assert.Single(resultState!.SelectedValues);
        Assert.Contains("Alice", resultState.SelectedValues);

        // 2. Load state back as if re-opening popup
        await vm.LoadStateAsync(resultState);
        Assert.Contains("Alice", vm.FilterState.SelectedValues);

        // 3. Search for "Bob"
        vm.SearchText = "Bob";
        await vm.InitializeAsync(new List<object> { "Bob" }); 
        Assert.Contains("Alice", vm.FilterState.SelectedValues);

        // 4. "Bob" should be visible. Check "Add to current selection"
        vm.AddToExistingFilter = true;
        vm.SelectAll = true; 
        Assert.Contains("Alice", vm.FilterState.SelectedValues);

        var filterActiveBefore = vm.IsFilterActive;
        var addToExistingBefore = vm.AddToExistingFilter;
        var selectedCountBefore = vm.FilterState.SelectedValues.Count;
        var selectedItemsBefore = string.Join(", ", vm.FilterState.SelectedValues);

        // Act
        vm.ApplyCommand.Execute(null);

        // Assert
        Assert.NotNull(resultState);
        Assert.Equal(2, resultState!.SelectedValues.Count);
        Assert.Contains("Alice", resultState.SelectedValues);
        Assert.Contains("Bob", resultState.SelectedValues);
    }

    [Fact]
    public async Task ApplyCommand_HighItemCount_AccumulatesCorrectly()
    {
        // Arrange
        ExcelFilterState? resultState = null;
        var vm = new ColumnFilterViewModel(
            async (s) => 
            {
                var all = Enumerable.Range(0, 2000).Select(i => (object)$"Item{i}").ToList();
                if (string.IsNullOrEmpty(s)) return all;
                return all.Where(x => x.ToString()!.Contains(s, StringComparison.OrdinalIgnoreCase));
            },
            (state) => resultState = state,
            () => { },
            propertyType: typeof(string)
        );
        var initialList = Enumerable.Range(0, 2000).Select(i => (object)$"Item{i}").ToList();
        await vm.InitializeAsync(initialList);

        // 1. Select Item0
        vm.FilterValues.First(x => x.DisplayText == "Item0").IsSelected = true;
        // Everything else unselected (SelectAll is false)
        foreach (var item in vm.FilterValues.Where(x => x.DisplayText != "Item0")) item.IsSelected = false;
        
        vm.ApplyCommand.Execute(null);
        Assert.Single(resultState!.SelectedValues);

        // 2. Load state back
        await vm.LoadStateAsync(resultState);

        // 3. Narrow list to "Item1000" (avoid SearchText setter racing with InitializeAsync via SearchCommand)
        await vm.InitializeAsync(new List<object> { "Item1000" });

        // 4. "Add to existing"
        vm.AddToExistingFilter = true;
        vm.SelectAll = true; // Select "Item1000"

        // Act
        vm.ApplyCommand.Execute(null);

        // Assert
        Assert.Equal(2, resultState!.SelectedValues.Count);
        Assert.Contains("Item0", resultState.SelectedValues);
        Assert.Contains("Item1000", resultState.SelectedValues);
    }

    [Fact]
    public async Task ApplyCommand_SalaryRange_SuccessiveFilters()
    {
        // Arrange
        ExcelFilterState? resultState = null;
        var vm = new ColumnFilterViewModel(
            async (s) => 
            {
                return new List<object> { 50000m, 60000m, 70000m, 80000m, 90000m, 100000m };
            },
            (state) => resultState = state,
            () => { },
            propertyType: typeof(decimal)
        );
        var initialList = new List<object> { 50000m, 60000m, 70000m, 80000m, 90000m, 100000m };
        await vm.InitializeAsync(initialList);

        // 1. First Filter: > 65000
        vm.SelectedCustomOperator = FilterOperator.GreaterThan;
        vm.CustomValue1 = "65000"; 
        vm.ApplyCommand.Execute(null); 
        
        Assert.NotNull(resultState);
        Assert.Equal(4, resultState!.SelectedValues.Count); // 70, 80, 90, 100

        // 2. Load state back
        await vm.LoadStateAsync(resultState);
        
        // VERIFY LOAD
        var selectedAfterLoad = vm.FilterValues.Where(v => v.IsSelected == true).Select(v => v.Value).ToList();
        Assert.Equal(4, selectedAfterLoad.Count);

        // 3. Second Filter: < 95000 with "Refine current selection" (Intersection)
        vm.AddToExistingFilter = true;
        vm.AccumulationMode = AccumulationMode.Intersection;
        vm.SelectedCustomOperator = FilterOperator.LessThan;
        vm.CustomValue1 = "95000"; 

        // Diagnostics
        var snapshot = new HashSet<object>();
        foreach(var item in vm.FilterValues) item.GetSelectedValues(snapshot);
        var snapshotItems = string.Join(", ", snapshot.OrderBy(v => (decimal)v));
        var currentSelected = string.Join(", ", resultState.SelectedValues.OrderBy(v => (decimal)v));
        var allItemsStates = string.Join(", ", vm.FilterValues.Select(v => $"{v.Value}:{v.IsSelected}"));

        // Act
        vm.ApplyCommand.Execute(null);

        // Assert — second custom stacks as AND (GreaterThan + LessThan), not In(selected salary floats)
        Assert.Equal(FilterOperator.GreaterThan, resultState!.CustomOperator);
        Assert.Single(resultState.AdditionalCustomCriteria);
        Assert.Equal(FilterOperator.LessThan, resultState.AdditionalCustomCriteria[0].Operator);
        Assert.Equal("95000", resultState.AdditionalCustomCriteria[0].Value1?.ToString());
    }

    /// <summary>
    /// Same as <see cref="ApplyCommand_SalaryRange_SuccessiveFilters"/> but loads a <em>cloned</em> state
    /// (different instance than <see cref="ColumnFilterViewModel.FilterState"/>), as after snapshot restore
    /// or adapter sync — <see cref="ColumnFilterViewModel.LoadStateAsync"/> must copy CustomOperator onto FilterState.
    /// </summary>
    [Fact]
    public async Task ApplyCommand_SalaryRange_SuccessiveFilters_WithClonedState()
    {
        ExcelFilterState? resultState = null;
        var vm = new ColumnFilterViewModel(
            async (s) =>
                new List<object> { 50000m, 60000m, 70000m, 80000m, 90000m, 100000m },
            (state) => resultState = state,
            () => { },
            propertyType: typeof(decimal));

        var initialList = new List<object> { 50000m, 60000m, 70000m, 80000m, 90000m, 100000m };
        await vm.InitializeAsync(initialList);

        vm.SelectedCustomOperator = FilterOperator.GreaterThan;
        vm.CustomValue1 = "65000";
        vm.ApplyCommand.Execute(null);

        Assert.NotNull(resultState);
        Assert.Equal(4, resultState!.SelectedValues.Count);

        var cloned = CloneExcelFilterState(resultState);
        Assert.NotSame(vm.FilterState, cloned);

        await vm.LoadStateAsync(cloned);

        vm.AddToExistingFilter = true;
        vm.AccumulationMode = AccumulationMode.Intersection;
        vm.SelectedCustomOperator = FilterOperator.LessThan;
        vm.CustomValue1 = "95000";

        vm.ApplyCommand.Execute(null);

        Assert.Equal(FilterOperator.GreaterThan, resultState!.CustomOperator);
        Assert.Single(resultState.AdditionalCustomCriteria);
        Assert.Equal(FilterOperator.LessThan, resultState.AdditionalCustomCriteria[0].Operator);
        Assert.Equal("95000", resultState.AdditionalCustomCriteria[0].Value1?.ToString());
    }

    [Fact]
    public async Task ApplyCommand_PreservesCustomOperator_WhenUnionAddToExisting()
    {
        ExcelFilterState? resultState = null;
        var vm = new ColumnFilterViewModel(
            async (s) =>
                new List<object> { 50000m, 60000m, 70000m, 80000m, 90000m, 100000m },
            (state) => resultState = state,
            () => { },
            propertyType: typeof(decimal));

        await vm.InitializeAsync(new List<object> { 50000m, 60000m, 70000m, 80000m, 90000m, 100000m });

        vm.SelectedCustomOperator = FilterOperator.GreaterThan;
        vm.CustomValue1 = "65000";
        vm.ApplyCommand.Execute(null);

        Assert.NotNull(resultState);
        Assert.Equal(FilterOperator.GreaterThan, resultState!.CustomOperator);

        await vm.LoadStateAsync(resultState);

        vm.AddToExistingFilter = true;
        vm.AccumulationMode = AccumulationMode.Union;
        var item50k = vm.FilterValues.First(x => x.Value is decimal d && d == 50000m);
        item50k.IsSelected = true;

        vm.ApplyCommand.Execute(null);

        Assert.Equal(FilterOperator.GreaterThan, resultState!.CustomOperator);
        Assert.Equal("65000", resultState.CustomValue1?.ToString());
        Assert.Contains(50000m, resultState.SelectedValues);
    }

    private static ExcelFilterState CloneExcelFilterState(ExcelFilterState s)
    {
        var clone = new ExcelFilterState
        {
            SearchText = s.SearchText,
            UseWildcards = s.UseWildcards,
            SelectAll = s.SelectAll,
            CustomOperator = s.CustomOperator,
            CustomValue1 = s.CustomValue1,
            CustomValue2 = s.CustomValue2
        };

        foreach (var d in s.DistinctValues)
            clone.DistinctValues.Add(d);
        foreach (var v in s.SelectedValues)
            clone.SelectedValues.Add(v);
        foreach (var c in s.AdditionalCustomCriteria)
        {
            clone.AdditionalCustomCriteria.Add(new ExcelFilterAdditionalCriterion
            {
                Operator = c.Operator,
                Value1 = c.Value1,
                Value2 = c.Value2
            });
        }

        return clone;
    }
}
