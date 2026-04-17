namespace DataFilter.PlatformShared.ViewModels;

/// <summary>
/// Optional scope for <see cref="IFilterableDataGridViewModel.FilterDescriptorsChanged"/> when only one column changed.
/// </summary>
public sealed class FilterDescriptorsChangedEventArgs : EventArgs
{
    /// <summary>
    /// When set, only the column with this property name was affected (e.g. clear filter).
    /// When <c>null</c>, listeners should assume any column may have changed (pipeline apply, snapshot restore).
    /// </summary>
    public string? AffectedPropertyName { get; init; }
}
