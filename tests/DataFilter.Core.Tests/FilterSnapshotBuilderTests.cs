using DataFilter.Core.Enums;
using Xunit;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Models;
using DataFilter.Core.Services;

namespace DataFilter.Core.Tests;

public class FilterSnapshotBuilderTests
{
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public void CreateSnapshot_WithDescriptors_ProducesEntriesMatchingDescriptors()
    {
        FilterContext context = new();
        context.AddOrUpdateDescriptor(new FilterDescriptor("Name", FilterOperator.Contains, "alice"));
        context.AddOrUpdateDescriptor(new FilterDescriptor("Value", FilterOperator.GreaterThan, 5));

        FilterSnapshotBuilder builder = new();
        Core.Abstractions.IFilterSnapshot snapshot = builder.CreateSnapshot(context);

        Assert.Equal(2, snapshot.Entries.Count);
        Assert.Contains(snapshot.Entries, e => e.PropertyName == "Name" && e.Operator == "Contains");
        Assert.Contains(snapshot.Entries, e => e.PropertyName == "Value" && e.Operator == "GreaterThan");
    }

    [Fact]
    public void CreateSnapshot_WithSort_ProducesSortEntries()
    {
        FilterContext context = new();
        context.SetSort("Name", isDescending: true);

        FilterSnapshotBuilder builder = new();
        Core.Abstractions.IFilterSnapshot snapshot = builder.CreateSnapshot(context);

        Assert.Single(snapshot.SortEntries);
        Assert.Equal("Name", snapshot.SortEntries[0].PropertyName);
        Assert.True(snapshot.SortEntries[0].IsDescending);
    }

    [Fact]
    public void RestoreSnapshot_RoundTrip_RebuildsIdenticalContext()
    {
        FilterContext original = new();
        original.AddOrUpdateDescriptor(new FilterDescriptor("Name", FilterOperator.StartsWith, "A"));
        original.SetSort("Value");

        FilterSnapshotBuilder builder = new();
        Core.Abstractions.IFilterSnapshot snapshot = builder.CreateSnapshot(original);

        FilterContext restored = new();
        builder.RestoreSnapshot(restored, snapshot);

        Assert.Single(restored.Descriptors);
        Assert.Equal("Name", restored.Descriptors[0].PropertyName);
        Assert.Equal(FilterOperator.StartsWith, restored.Descriptors[0].Operator);
        Assert.Single(restored.SortDescriptors);
        Assert.Equal("Value", restored.SortDescriptors[0].PropertyName);
    }

    [Fact]
    public void CreateSnapshot_EmptyContext_ProducesEmptySnapshot()
    {
        FilterContext context = new();
        FilterSnapshotBuilder builder = new();
        IFilterSnapshot snapshot = builder.CreateSnapshot(context);

        Assert.Empty(snapshot.Entries);
        Assert.Empty(snapshot.SortEntries);
    }

    [Fact]
    public void RestoreSnapshot_TwoColumnScopedFilterGroups_PreservesBothDescriptors()
    {
        FilterContext original = new();
        var ageGroup = new FilterGroup(LogicalOperator.And, "Age");
        ageGroup.Add(new FilterDescriptor("Age", FilterOperator.GreaterThan, 10));
        original.AddOrUpdateDescriptor(ageGroup);

        var nameGroup = new FilterGroup(LogicalOperator.And, "Name");
        nameGroup.Add(new FilterDescriptor("Name", FilterOperator.Contains, "a"));
        original.AddOrUpdateDescriptor(nameGroup);

        FilterSnapshotBuilder builder = new();
        IFilterSnapshot snapshot = builder.CreateSnapshot(original);

        FilterContext restored = new();
        builder.RestoreSnapshot(restored, snapshot);

        Assert.Equal(2, restored.Descriptors.Count);
        Assert.Equal("Age", restored.Descriptors[0].PropertyName);
        Assert.Equal("Name", restored.Descriptors[1].PropertyName);
    }
}
