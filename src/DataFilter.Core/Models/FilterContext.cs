using DataFilter.Core.Abstractions;

namespace DataFilter.Core.Models;

/// <summary>
/// Encapsulates the current state of filtering, sorting, and pagination.
/// </summary>
public class FilterContext : IFilterContext
{
    private readonly List<IFilterDescriptor> _descriptors = new();
    private readonly List<ISortDescriptor> _sortDescriptors = new();

    /// <inheritdoc />
    public IReadOnlyList<IFilterDescriptor> Descriptors => _descriptors.AsReadOnly();

    /// <inheritdoc />
    public IReadOnlyList<ISortDescriptor> SortDescriptors => _sortDescriptors.AsReadOnly();

    /// <inheritdoc />
    public int Page { get; set; } = 1;

    /// <inheritdoc />
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Adds or updates a filter descriptor for a specific property.
    /// </summary>
    /// <param name="descriptor">The filter descriptor to add or update.</param>
    public void AddOrUpdateDescriptor(IFilterDescriptor descriptor)
    {
        if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

        var existing = _descriptors.Find(d => d.PropertyName == descriptor.PropertyName);
        if (existing != null)
        {
            _descriptors.Remove(existing);
        }
        _descriptors.Add(descriptor);
    }

    /// <summary>
    /// Removes the filter descriptor for a specific property.
    /// </summary>
    /// <param name="propertyName">The name of the property to remove the filter for.</param>
    public void RemoveDescriptor(string propertyName)
    {
        _descriptors.RemoveAll(d => d.PropertyName == propertyName);
    }

    /// <summary>
    /// Clears all filter descriptors.
    /// </summary>
    public void ClearDescriptors()
    {
        _descriptors.Clear();
    }

    /// <inheritdoc />
    public void ReplaceDescriptors(IReadOnlyList<IFilterDescriptor> descriptors)
    {
        if (descriptors == null) throw new ArgumentNullException(nameof(descriptors));

        _descriptors.Clear();
        foreach (IFilterDescriptor d in descriptors)
        {
            if (d == null) throw new ArgumentException("Descriptors cannot contain null entries.", nameof(descriptors));
            _descriptors.Add(d);
        }
    }

    /// <summary>
    /// Sets a single sort criterion, replacing any existing sort.
    /// </summary>
    /// <param name="propertyName">The property to sort by.</param>
    /// <param name="isDescending">Whether to sort in descending order.</param>
    public void SetSort(string propertyName, bool isDescending = false)
    {
        if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
        _sortDescriptors.Clear();
        _sortDescriptors.Add(new SortDescriptor(propertyName, isDescending));
    }

    /// <summary>
    /// Adds a sort criterion to the existing ones.
    /// </summary>
    /// <param name="propertyName">The property to sort by.</param>
    /// <param name="isDescending">Whether to sort in descending order.</param>
    public void AddSort(string propertyName, bool isDescending = false)
    {
        if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
        
        // Remove existing sort for same property if any
        _sortDescriptors.RemoveAll(s => s.PropertyName == propertyName);
        _sortDescriptors.Add(new SortDescriptor(propertyName, isDescending));
    }

    /// <summary>
    /// Clears all sort criteria.
    /// </summary>
    public void ClearSort()
    {
        _sortDescriptors.Clear();
    }
}
