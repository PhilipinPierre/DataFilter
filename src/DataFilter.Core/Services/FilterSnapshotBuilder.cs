using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;

namespace DataFilter.Core.Services;

/// <summary>
/// Extracts and restores filter snapshots from/to a <see cref="IFilterContext"/>.
/// </summary>
public sealed class FilterSnapshotBuilder : IFilterSnapshotBuilder
{
    /// <inheritdoc />
    public IFilterSnapshot CreateSnapshot(IFilterContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        List<FilterSnapshotEntry> entries = context.Descriptors
            .Select(DescriptorToEntry)
            .ToList();

        List<SortSnapshotEntry> sortEntries = context.SortDescriptors
            .Select(s => new SortSnapshotEntry
            {
                PropertyName = s.PropertyName,
                IsDescending = s.IsDescending
            })
            .ToList();

        return new FilterSnapshot(
            (IReadOnlyList<FilterSnapshotEntry>)entries,
            (IReadOnlyList<SortSnapshotEntry>)sortEntries);
    }

    /// <inheritdoc />
    public void RestoreSnapshot(IFilterContext context, IFilterSnapshot snapshot)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

        if (context is not FilterContext concreteContext)
        {
            throw new ArgumentException(
                $"RestoreSnapshot requires a {nameof(FilterContext)} instance.", nameof(context));
        }

        concreteContext.ClearDescriptors();
        concreteContext.ClearSort();

        foreach (FilterSnapshotEntry entry in snapshot.Entries)
        {
            IFilterDescriptor descriptor = EntryToDescriptor(entry);
            concreteContext.AddOrUpdateDescriptor(descriptor);
        }

        foreach (SortSnapshotEntry sortEntry in snapshot.SortEntries)
        {
            concreteContext.SetSort(sortEntry.PropertyName, sortEntry.IsDescending);
        }
    }

    private static FilterSnapshotEntry DescriptorToEntry(IFilterDescriptor descriptor)
    {
        if (descriptor is IFilterGroup group)
        {
            return new FilterSnapshotEntry
            {
                PropertyName = string.Empty,
                Operator = string.Empty,
                LogicalOperator = group.LogicalOperator.ToString(),
                Children = group.Descriptors.Select(DescriptorToEntry).ToList()
            };
        }

        return new FilterSnapshotEntry
        {
            PropertyName = descriptor.PropertyName,
            Operator = descriptor.Operator.ToString(),
            Value = descriptor.Value
        };
    }

    private static IFilterDescriptor EntryToDescriptor(FilterSnapshotEntry entry)
    {
        if (entry.IsGroup && entry.Children != null)
        {
            LogicalOperator logicalOp = Enum.TryParse<LogicalOperator>(
                entry.LogicalOperator, out LogicalOperator parsed)
                ? parsed
                : LogicalOperator.And;

            FilterGroup group = new(logicalOp);
            foreach (FilterSnapshotEntry child in entry.Children)
            {
                group.Add(EntryToDescriptor(child));
            }
            return group;
        }

        FilterOperator op = (FilterOperator)Enum.Parse(typeof(FilterOperator), entry.Operator);
        return new FilterDescriptor(entry.PropertyName, op, entry.Value);
    }
}
